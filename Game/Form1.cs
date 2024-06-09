using Guna.UI2.WinForms;
using Newtonsoft.Json.Linq;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Toolkit.Uwp.Notifications;
using Button = System.Windows.Forms.Button;

namespace Game
{
    public partial class Form1 : Form
    {
        private CircularButton[] buttons;
        private bool[] buttonStates;
        private DirectInput directInput;
        private Joystick joystick;
        private JoystickState joystickState;
        private Guna2Panel acrylicPanel;
        private ContextMenuStrip sharedMenu;
        private ToolStripTextBox textBox;
        private Button currentButton; // 用于记录当前点击的按钮
        private NotifyIcon trayIcon;//任务栏图标
        private ContextMenu trayMenu;//任务栏图标菜单
        public Form1()
        {
            InitializeComponent();
            InitializeAcrylicEffect();
            LoadImage();
            InitializeSharedMenu();
        
            #region 任务栏图标
            // 创建一个右键菜单
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Setting", OnSetting);
            trayMenu.MenuItems.Add("Plugins", OnPlugins);
            trayMenu.MenuItems.Add("Updata", OnUpdata);
            trayMenu.MenuItems.Add("About", OnAbout);
            trayMenu.MenuItems.Add("Exit", OnExit);
            
            // 创建系统托盘图标
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Game Pad Test";
            trayIcon.Icon = new Icon("C:\\Program Files (x86)\\Gamepad Tester\\Resources\\banlizai.ico"); // 任务栏图标
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            // 添加双击事件处理程序
            trayIcon.DoubleClick += TrayIconDoubleClick;
            #endregion
        }
       
        #region 任务栏图标
        // 双击系统托盘图标时触发的事件处理程序
        private void TrayIconDoubleClick(object sender, EventArgs e)
        {
            // 在此处添加双击图标时的操作
            // 例如，显示主窗体或者执行其他操作
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        // 点击退出菜单项时触发的事件处理程序
        private void OnExit(object sender, EventArgs e)
        {
            // 在此处添加退出操作
            System.Windows.Forms.Application.Exit();
        }
        private void OnSetting(object sender, EventArgs e)
        {
            Options options = new Options();
            options.Show();
        }
        private void OnPlugins(object sender, EventArgs e)
        {
            Plugins plugins = new Plugins();
            plugins.Show();
        }
        private void OnAbout(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }
        private async void OnUpdata(object sender, EventArgs e)
        {
            string latestVersion = await Task.Run(() => GetLatestReleaseVersionAsync("qizhoward", "GamePad"));
            string localVersion = await Task.Run(() => GetLocalVersion());
            if (IsNewerVersion(localVersion, latestVersion))
            {
                await DownloadLatestReleaseAsync("qizhoward", "GamePad", latestVersion);
            }
            else
            {
                MessageBox.Show("You already have the latest version.");
            }
        }
        // 重写窗体关闭事件，以便将窗体最小化到系统托盘
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                ShowToastNotification("友好提示", "双击鼠标左键还原");
                e.Cancel = true;
                this.Hide();
            }
          
        }
        #region 消息提示

        private void ShowToastNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText(title)
                .AddText(message)
                .Show();
        }

        #endregion
        // 在窗体加载时显示系统托盘图标
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            trayIcon.Visible = true;
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            buttons = new CircularButton[12];
            buttonStates = new bool[12];
            int buttonPerRow = 6; // 每行按钮数
            int buttonWidth = 30; // 按钮宽度
            int buttonHeight = 30; // 按钮高度
            int spacing = 12; // 按钮之间的间距

            // 创建12个圆形按钮并添加到窗口中
            for (int i = 0; i < 12; i++)
            {
                CircularButton button = new CircularButton();
                button.Location = new Point(spacing + (i % buttonPerRow) * (buttonWidth + spacing), spacing + (i / buttonPerRow) * (buttonHeight + spacing));
                button.Size = new Size(buttonWidth, buttonHeight); // 设置按钮大小
                button.Text = (i + 1).ToString(); // 设置按钮文本为序号
                button.Font = new Font(button.Font.FontFamily, 12); // 设置按钮字体大小
                Console.WriteLine($"Button {i + 1}: Location ({button.Location.X}, {button.Location.Y}), Size ({button.Width}, {button.Height})");
                this.Controls.Add(button);
                buttons[i] = button;
                groupBox1.Controls.Add(button);
            }
            // 初始化手柄输入状态
            InitializeJoystick();
            //右键删除图标
            InitializeContextMenuStrip();
        }
        private void InitializeJoystick()
        {
            directInput = new DirectInput();

            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                joystickGuid = deviceInstance.InstanceGuid;
            }

            if (joystickGuid == Guid.Empty)
            {
                MessageBox.Show("未找到连接的手柄！");
                return;
            }

            joystick = new Joystick(directInput, joystickGuid);
            joystick.Acquire();

            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            joystick.Poll();
            joystickState = joystick.GetCurrentState();

            ProcessJoystickInput(joystickState);
        }
        private void ProcessJoystickInput(JoystickState state)
        {
            // 根据手柄输入控制按钮高亮显示
            buttonStates[0] = state.Buttons[0];
            buttonStates[1] = state.Buttons[1];
            buttonStates[2] = state.Buttons[2];
            buttonStates[3] = state.Buttons[3];
            buttonStates[4] = false; // 这里根据需要设置其他按钮的状态

            for (int i = 0; i < 5; i++)
            {
                if (buttonStates[i])
                    buttons[i].BackColor = Color.LightGray;
                else
                    buttons[i].BackColor = Color.Gray;
            }
        }


        public class CircularButton : Control
        {
            private Color hoverColor = Color.LightGray;
            private Color defaultColor = Color.Gray;

            public CircularButton()
            {
                this.BackColor = defaultColor;
                this.Cursor = Cursors.Hand;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 绘制圆形按钮
                g.FillEllipse(new SolidBrush(this.BackColor), 0, 0, this.Width - 1, this.Height - 1);
                g.DrawEllipse(new Pen(Color.Black), 0, 0, this.Width - 1, this.Height - 1);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                this.BackColor = hoverColor;
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                this.BackColor = defaultColor;
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
        
        #region 版本更新
        private async void updataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "检查更新中...";
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            try
            {
                string latestVersion = await Task.Run(() => GetLatestReleaseVersionAsync("qizhoward", "GamePad"));
                string localVersion = GetLocalVersion();

                if (!string.IsNullOrEmpty(latestVersion))
                {
                    toolStripStatusLabel1.Text = $"最新版本: {latestVersion}";
                    if (IsNewerVersion(localVersion, latestVersion))
                    {
                        toolStripStatusLabel1.Text = $"发现新版本: {latestVersion}，正在下载...";
                        await DownloadLatestReleaseAsync("qizhoward", "GamePad", latestVersion);
                        toolStripStatusLabel1.Text = "下载完成，请安装新版本。";
                    }
                    else
                    {
                        toolStripStatusLabel1.Text = "当前已是最新版本。";
                    }
                }
                else
                {
                    toolStripStatusLabel1.Text = "检查更新失败";
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = $"检查更新失败: {ex.Message}";
            }

            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;


        }
        private async Task<string> GetLatestReleaseVersionAsync(string owner, string repo)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request"); // GitHub API 需要用户代理

                string url = $"https://api.github.com/repos/qizhoward/GamePad/releases/latest";

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                return json["tag_name"]?.ToString();
            }
        }
        private string GetLocalVersion()
        {
            // 获取当前执行的程序集
            var assembly = Assembly.GetExecutingAssembly();

            // 获取程序集版本信息
            var version = assembly.GetName().Version;

            return version.ToString();
        }
        private bool IsNewerVersion(string localVersion, string latestVersion)
        {
            Version local = new Version(localVersion);
            Version latest = new Version(latestVersion);

            return latest.CompareTo(local) > 0;
        }
        private async Task DownloadLatestReleaseAsync(string owner, string repo, string version)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request"); // GitHub API 需要用户代理

                string url = $"https://api.github.com/repos/qizhoward/GamePad/releases/tags/{version}";

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                string downloadUrl = json["assets"]?[0]?["browser_download_url"]?.ToString();
                if (downloadUrl != null)
                {
                    HttpResponseMessage downloadResponse = await client.GetAsync(downloadUrl);
                    downloadResponse.EnsureSuccessStatusCode();

                    byte[] fileBytes = await downloadResponse.Content.ReadAsByteArrayAsync();

                    // 保存文件
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GamePad_latest_release.exe");
                    await Task.Run(() => File.WriteAllBytes(filePath, fileBytes));
                    // 提示保存路径（可选）
                    MessageBox.Show($"新版本已下载到 {filePath}", "下载完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        #endregion
        
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void LoadImage()
        {
            // 设置图片的路径
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "20240525a.png");
               
            // 加载图片
            pictureBox1.Image = System.Drawing.Image.FromFile(imagePath);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom; // 根据需要调整
            pictureBox1.BackColor = Color.DimGray;

        }

        #region 亚克力
        private void InitializeAcrylicEffect()
        {   
            // 初始化Guna2Panel
            acrylicPanel = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Transparent,
                FillColor = System.Drawing.Color.FromArgb(128, 255, 255, 255), // 半透明白色
                BorderColor = Color.Transparent,
                ShadowDecoration = { Parent = acrylicPanel }
            };

            pictureBox1.Controls.Add(acrylicPanel);
            acrylicPanel.BringToFront();
        }
        #endregion

        #region 菜单按钮
        private void InitializeSharedMenu()
        {
            // 创建共享菜单对象
            sharedMenu = new ContextMenuStrip();
            // 创建文本输入框
            textBox = new ToolStripTextBox();
            textBox.AutoSize = false;
            textBox.Width = 100;
            
            // 添加菜单项
            ToolStripMenuItem menuItem1 = new ToolStripMenuItem("显示:");
            ToolStripMenuItem menuItem2 = new ToolStripMenuItem("键盘设置");
            ToolStripMenuItem menuItem3 = new ToolStripMenuItem("鼠标移动");
            ToolStripMenuItem menuItem4 = new ToolStripMenuItem("鼠标点击");
            ToolStripSeparator separator1 = new ToolStripSeparator(); // 添加分隔符
            ToolStripMenuItem menuItem5 = new ToolStripMenuItem("震动");
            ToolStripMenuItem menuItem6 = new ToolStripMenuItem("按放切换");
            ToolStripSeparator separator2 = new ToolStripSeparator(); // 添加分隔符
            ToolStripMenuItem menuItem7 = new ToolStripMenuItem("复制键");
            ToolStripMenuItem menuItem8 = new ToolStripMenuItem("粘贴键");
            ToolStripMenuItem menuItem9 = new ToolStripMenuItem("清空键");
            menuItem1.DropDownItems.Add(textBox);
            menuItem1.Click += MenuItem_Click;
            menuItem2.Click += MenuItem2_Click;
            menuItem3.Click += MenuItem_Click;
            menuItem4.Click += MenuItem_Click;
            menuItem5.Click += MenuItem5_Click;
            menuItem6.Click += MenuItem_Click;
            menuItem7.Click += MenuItem7_Click;
            menuItem8.Click += MenuItem8_Click;
            menuItem9.Click += MenuItem9_Click;
            sharedMenu.Items.Add(menuItem1);
            sharedMenu.Items.Add(menuItem2);
            sharedMenu.Items.Add(menuItem3);
            sharedMenu.Items.Add(menuItem4);
            sharedMenu.Items.Add(separator1); // 添加分隔符
            sharedMenu.Items.Add(menuItem5);
            sharedMenu.Items.Add(menuItem6);
            sharedMenu.Items.Add(separator2); // 添加分隔符
            sharedMenu.Items.Add(menuItem7);
            sharedMenu.Items.Add(menuItem8);
            sharedMenu.Items.Add(menuItem9);
            // 将菜单与所有按钮关联
          
            button1.MouseDown += Button_MouseDown;
            button2.MouseDown += Button_MouseDown;
            button3.MouseDown += Button_MouseDown;
            button4.MouseDown += Button_MouseDown;
            button5.MouseDown += Button_MouseDown;
            button6.MouseDown += Button_MouseDown;
            button7.MouseDown += Button_MouseDown;
            button8.MouseDown += Button_MouseDown;
            button9.MouseDown += Button_MouseDown;
            button10.MouseDown += Button_MouseDown;
            button11.MouseDown += Button_MouseDown;
            button12.MouseDown += Button_MouseDown;
            button13.MouseDown += Button_MouseDown;
            button14.MouseDown += Button_MouseDown;
            button15.MouseDown += Button_MouseDown;
            button16.MouseDown += Button_MouseDown;
            button17.MouseDown += Button_MouseDown;
            button18.MouseDown += Button_MouseDown;
            button19.MouseDown += Button_MouseDown;
            button20.MouseDown += Button_MouseDown;
            button21.MouseDown += Button_MouseDown;
            button22.MouseDown += Button_MouseDown;
            button23.MouseDown += Button_MouseDown;
            button24.MouseDown += Button_MouseDown;
            button25.MouseDown += Button_MouseDown;
            button26.MouseDown += Button_MouseDown;
            button27.MouseDown += Button_MouseDown;
            button28.MouseDown += Button_MouseDown;
            button29.MouseDown += Button_MouseDown;
            button30.MouseDown += Button_MouseDown;
            button31.MouseDown += Button_MouseDown;
            // 监听文本输入框内容变化事件
            this.textBox.KeyDown += TextBox_KeyDown;
        }
        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                currentButton = sender as System.Windows.Forms.Button;
                Point cursorPosition = System.Windows.Forms.Cursor.Position;
                sharedMenu.Show(cursorPosition);
            }
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 检查是否按下了 Enter 键
            if (e.KeyCode == Keys.Enter)
            {
                // 更新当前按钮的文本为文本框中的内容
                if (currentButton != null)
                {
                    currentButton.Text = textBox.Text;
                }
                // 防止 Enter 键被控件默认处理
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
      
        private void MenuItem_Click(object sender, EventArgs e)
        {
            // 处理菜单项点击事件
            MessageBox.Show(((ToolStripMenuItem)sender).Text);
        }

        private void MenuItem2_Click(object sender, EventArgs e)
        {
            KeyboardSetting keyboardSetting = new KeyboardSetting();
            keyboardSetting.ShowDialog();
        }
        private void MenuItem5_Click(object sender, EventArgs e)
        {
            // 获取触发事件的菜单项
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

            // 切换菜单项的选中状态
            menuItem.Checked = !menuItem.Checked;
        }
        private void MenuItem9_Click(object sender, EventArgs e)
        {
            // 弹出对话框询问用户是否清除内容
            DialogResult result = MessageBox.Show("是否清除内容？", "清空确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            // 如果用户点击 "是"，则清空文本框中的内容
            if (result == DialogResult.Yes)
            {
                currentButton.Text = string.Empty;
            }
        }
        private void MenuItem7_Click(object sender, EventArgs e)
        {
            if (currentButton != null)
            {
                Clipboard.SetText(currentButton.Text);
            }
        }
        private void MenuItem8_Click(object sender, EventArgs e)
        {
            if (currentButton != null && Clipboard.ContainsText())
            {
                currentButton.Text = Clipboard.GetText();
            }
        }
        #endregion

        List<PictureBox> pictureBoxes = new List<PictureBox>();
        ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
        int pictureBoxCount = 0;
        int iconsInCurrentRow = 0;
        int iconWidth = 60; // 图标宽度
        int iconHeight = 60; // 图标高度
        int iconSpacing = 10; // 图标间距
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 设置文件选择器的属性
            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = "所有文件 (*.*)|*.*|可执行文件 (*.exe)|*.exe|快捷方式 (*.lnk)|*.lnk";
            openFileDialog.Multiselect = false; // 只允许选择一个文件

            // 打开文件选择器并检查用户是否选择了文件
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取用户选择的文件路径并进行处理
                string selectedFilePath = openFileDialog.FileName;

                // 在这里可以使用选定的文件路径进行后续操作，例如加载图片到 pictureBox 中等等
                Icon fileIcon = Icon.ExtractAssociatedIcon(selectedFilePath);
                if (fileIcon != null)
                {
                    // 将图标转换为位图
                    Bitmap bitmap = fileIcon.ToBitmap();

                    // 创建一个新的PictureBox控件
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Size = new Size(60, 60);
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox.Image = bitmap;

                    /*int newX = pictureBox2.Location.X + pictureBox2.Width + 10; // 加上一些间隔
                      int newY = pictureBox2.Location.Y + (pictureBoxCount * (pictureBox.Height + 10)); // 根据控件数量计算Y位置
                    */
                    int newX;
                    int newY;
                    ///换行
                    if (iconsInCurrentRow >= 4)
                    {
                        // 换行
                        newX = pictureBox2.Location.X; // 放在第一列
                        newY = pictureBox2.Location.Y + iconHeight + iconSpacing; // 放在下一行
                        iconsInCurrentRow = 0; // 重置图标数量
                    }
                    else
                    {
                        // 不换行
                        newX = pictureBox2.Location.X + (iconWidth + iconSpacing) * iconsInCurrentRow;
                        newY = pictureBox2.Location.Y + (iconHeight + iconSpacing) * (iconsInCurrentRow / 4); // 更新 Y 坐标以实现换行
                    }
                    ///

                    // 设置新PictureBox的位置
                    pictureBox.Location = new Point(newX, newY);

                    // 将新的PictureBox添加到panel1中
                    panel1.Controls.Add(pictureBox);
                    pictureBox.Click += PictureBox_Click;
                    // 添加右键菜单
                    pictureBox.ContextMenuStrip = contextMenuStrip;

                    // 保存程序路径
                    pictureBox.Tag = selectedFilePath;
                    // 将新的PictureBox添加到集合中
                    pictureBoxes.Add(pictureBox);
                    //pictureBoxCount++;
                    iconsInCurrentRow++;
                    // 调整已有图标的位置
                    //AdjustPictureBoxesPosition();
                }
                else
                {
                    // 如果无法获取文件的图标，则显示默认图标或者给出提示
                    // 这里可以根据实际情况进行处理
                }

            }
        }
        private void AdjustPictureBoxesPosition()
        {
            int x = pictureBox2.Location.X + pictureBox2.Width + 10; // 初始 x 位置为第一个图标的右边加上间距
            int y = pictureBox2.Location.Y; // y 位置与第一个图标一致

            // 遍历已添加的 PictureBox 控件，调整它们的位置
            foreach (PictureBox pb in pictureBoxes)
            {
                pb.Location = new Point(x, y);
                x += pb.Width + 10; // 更新下一个 PictureBox 的 x 位置，考虑到间距
            }
        }
        //“删除”菜单
        private void InitializeContextMenuStrip()
        {
            // 创建右键菜单项
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("删除");
            deleteMenuItem.Click += DeleteMenuItem_Click;

            // 将菜单项添加到右键菜单
            contextMenuStrip.Items.Add(deleteMenuItem);
        }
        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            // 获取触发右键菜单事件的菜单项
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                // 获取与菜单项关联的右键菜单
                ContextMenuStrip menu = menuItem.Owner as ContextMenuStrip;
                if (menu != null)
                {
                    // 获取触发右键菜单事件的PictureBox控件
                    PictureBox pictureBox = menu.SourceControl as PictureBox;
                    if (pictureBox != null)
                    {
                        // 从panel1中移除PictureBox控件
                        panel1.Controls.Remove(pictureBox);

                        // 进行其他清理操作，例如删除关联的文件等等

                        // 从集合中移除对应的PictureBox控件
                        pictureBoxes.Remove(pictureBox);
                    }
                }
            }
        }
      
        private void PictureBox_Click(object sender, EventArgs e)
        {
                // 获取点击的PictureBox控件
                PictureBox pictureBox = sender as PictureBox;

                // 获取程序路径
                string programPath = pictureBox.Tag.ToString();

                // 启动程序
                System.Diagnostics.Process.Start(programPath);
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/qizhoward/GamePad";

            try
            {
                // 使用默认浏览器打开URL
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // 如果出现错误，显示错误消息
                MessageBox.Show("无法打开浏览器: " + ex.Message);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.InitialDirectory = System.Windows.Forms.Application.ExecutablePath;
            openFile.Filter =
                "xml files (*.xml)|*.xml|" +
                "txt files (*.txt)|*.txt|" +
                "Game project files(*.Game)| *.game |" +
                "OK family files(*.ok)|*.ok |" +
                "YuPeng family template files(*.yupeng) | *.yupeng |" +
                "All files(=.=) | *.* ";
            openFile.FilterIndex = 1;
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string fullName = openFile.FileName;
                string fileName = Path.GetFileName(fullName);

            }
            string filePath = @"C:\Users\Administrator\Documents\settings.xml";
            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    AppSettings settings = (AppSettings)serializer.Deserialize(reader);

                    Console.WriteLine("Setting1: " + settings.Name);
                    Console.WriteLine("Setting2: " + settings.Text);
                    Console.WriteLine("Setting4: " + settings.Note);
                    Console.WriteLine("Setting3: " + settings.Setting3);
                }
            }
            else
            {
                Console.WriteLine("设置文件不存在。");
            }
        }
       
        /// <summary>
        /// Save As 另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // 设置保存文件的筛选器
            saveFileDialog.Filter = "YuPeng family template files(*.yupeng)|*.yupeng|Text Files (*.txt)|*.txt|Game project files(*.Game)|*.game|All Files (*.*)|*.*";
            // 可以添加更多的筛选器，以支持不同类型的文件格式，格式为："描述1|扩展名1|描述2|扩展名2|..."

            // 设置默认的文件名
            saveFileDialog.FileName = "filenameVer2024.yupeng";

            // 显示对话框，并检查用户是否点击了保存按钮
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取用户选择的文件名
                string filePath = saveFileDialog.FileName;
                try
                {

                    // 执行保存文件的逻辑
                    // 这里只是一个示例，实际保存文件的逻辑需要根据你的需求来实现
                    File.WriteAllText(filePath, "保存内容");

                    MessageBox.Show("文件保存成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存文件时出现错误：{ex.Message}");
                }
                // 执行保存文件的逻辑，例如保存文件到指定路径
                // 这里只是一个示例，实际保存文件的逻辑需要根据你的需求来实现
                // File.WriteAllText(filePath, "Content of the file");
            }

        }
        /// <summary>
        /// Save 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            string filePath = @"C:\Users\Administrator\Documents\settings.xml";

            AppSettings settings = new AppSettings
            {
                Name = "Button23",
                Text = "1",
                Note = "备注",
                Setting3 = true
            };

            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, settings);
            }

            Console.WriteLine("设置已保存到XML文件。");
        }
        /// <summary>
        /// Exit 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Plugins 插件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pluginsPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Plugins plugins = new Plugins();
            plugins.Show();
        }
        /// <summary>
        /// SDK Download 扩展
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/qizhoward/GamePad";

            try
            {
                // 使用默认浏览器打开URL
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // 如果出现错误，显示错误消息
                MessageBox.Show("无法打开浏览器: " + ex.Message);
            }
        }
        /// <summary>
        /// Options 选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            Options options = new Options();
            options.Show(); 
        }
        /// <summary>
        /// CHM 打开帮助
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            string helpFilePath = "C:\\Program Files (x86)\\Gamepad Tester\\Gamepad Tester help.chm";
            Help.ShowHelp(this, helpFilePath);
        }
    }
}

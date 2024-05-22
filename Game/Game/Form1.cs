using SharpDX.DirectInput;
using System;
using System.Drawing;
using System.Windows.Forms;
using static Game.Form1;

namespace Game
{
    public partial class Form1 : Form
    {
        private CircularButton[] buttons;
        private bool[] buttonStates;
        private DirectInput directInput;
        private Joystick joystick;
        private JoystickState joystickState;
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            buttons = new CircularButton[12];
            buttonStates = new bool[12];
            int buttonPerRow = 6; // 每行按钮数
            int buttonWidth = 30; // 按钮宽度
            int buttonHeight = 30; // 按钮高度
            int spacing = 10; // 按钮之间的间距
            
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

       
    }

}


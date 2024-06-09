using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace Game
{
    public partial class Plugins : Form
    {
        private SplitContainer splitContainer1;
        private DataGridView topDataGridView;
        private DataGridView bottomDataGridView;
        /*private TextBox txtFilterDescription;
        private TextBox txtFilterAuthor;*/
        private ComboBox cmbFilterVersion;
        BindingList<Plugin> pluginBindingList = new BindingList<Plugin>();
        private List<Plugin> cachedPlugins = new List<Plugin>(); // 缓存已加载的数据
        private List<Plugin> additionalPlugins = new List<Plugin>(); // 用于存储右键菜单添加的数据
        public Plugins()
        {
            InitializeComponent();
            InitializeSplitContainer();
            InitializeTopDataGridView();
            InitializeBottomDataGridView();
            /*InitializeFilterControls();*/

            LoadPlugins(@"C:\Program Files (x86)\Gamepad Tester\Stdplugs");
        }
        private void LoadPlugins(string folderPath)
        {
            List<Plugin> plugins = new List<Plugin>();

            // 遍历文件夹中的所有 .game 文件
            foreach (string file in Directory.GetFiles(folderPath, "*.game"))
            {
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 8)
                    {
                        Plugin plugin = new Plugin
                        {
                            Marker = parts[0].Trim(),
                            PluginName = parts[1].Trim(),
                            Description = parts[2].Trim(), // 将索引改为 0
                            Version = parts[3].Trim(),     // Version
                            Author = parts[4].Trim(),       // Author
                            Status = parts[5].Trim(),
                            Size = parts[6].Trim(),
                            FullPath = parts[7].Trim()
                        };
                        plugins.Add(plugin);
                    }
                }
            }
           
            

            cachedPlugins = plugins;// 更新缓存
            topDataGridView.DataSource = null; // 确保刷新数据源
            topDataGridView.DataSource = plugins;
            topDataGridView.Refresh(); // 刷新DataGridView
            LoadDataIntoBindingList(plugins);// 加载数据到 BindingList
           
        }

        public class Plugin
        {
            public string Marker { get; set; } // 标记
            public string PluginName { get; set; } // 插件名称
            public string Description { get; set; } // 描述
            public string Version { get; set; } // 版本
            public string Author { get; set; } // 作者
            public string Status { get; set; } // 状态
            public string Size { get; set; } // 大小
            public string FullPath { get; set; } // 完整路径

            
        }
        private void InitializeSplitContainer()
        {
            splitContainer1 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = System.Windows.Forms.Orientation.Horizontal,
                SplitterDistance = 250
            };

            this.Controls.Add(splitContainer1);
        }
        private void InitializeTopDataGridView()
        {
            topDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                RowHeadersVisible = false, // 隐藏行标题
                SelectionMode = DataGridViewSelectionMode.FullRowSelect // 设置选中整行
            };
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle
            {
                BackColor = SystemColors.AppWorkspace, // 设置单元格背景颜色
                ForeColor = Color.White, // 设置单元格字体颜色
                SelectionBackColor = SystemColors.Highlight, // 设置选中单元格背景颜色
                SelectionForeColor = SystemColors.HighlightText // 设置选中单元格字体颜色
            };

            topDataGridView.DefaultCellStyle = cellStyle;

            var markColumn = new DataGridViewTextBoxColumn { HeaderText = "标记" };
            markColumn.Name = "Marker"; // 列的名称
            markColumn.Width = 40;
            markColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁用自动调整
            markColumn.DataPropertyName = "Marker";
            topDataGridView.Columns.Add(markColumn);
            var nameColumn = new DataGridViewTextBoxColumn { HeaderText = "名称", DataPropertyName = "PluginName" };
            nameColumn.Name = "PluginName";
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁用自动调整
            nameColumn.Width = 80;
            topDataGridView.Columns.Add(nameColumn);
            var descriptionColumn = new DataGridViewTextBoxColumn { HeaderText = "描述", DataPropertyName = "Description" };
            descriptionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁用自动调整
            descriptionColumn.Width = 80;
            descriptionColumn.Name = "Description";
            topDataGridView.Columns.Add(descriptionColumn);   
            var statusColumn = new DataGridViewTextBoxColumn { HeaderText = "状态", DataPropertyName = "Status" };
            topDataGridView.Columns.Add(statusColumn);
            statusColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁用自动调整
            statusColumn.Width = 60;
            var sizeColumn = new DataGridViewTextBoxColumn { HeaderText = "大小", DataPropertyName = "Size" };
            topDataGridView.Columns.Add(sizeColumn);
            sizeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁用自动调整
            sizeColumn.Width = 40;
            var fullPathColumn = new DataGridViewTextBoxColumn { HeaderText = "完整路径", DataPropertyName = "FullPath" };
            topDataGridView.Columns.Add(fullPathColumn);
           

            // 添加示例数据


            splitContainer1.Panel1.Controls.Add(topDataGridView);
        }
        private void InitializeBottomDataGridView()
        {
            bottomDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, // 禁用自动调整列宽度
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                RowHeadersVisible = false, // 隐藏行标题
                BackgroundColor = SystemColors.Control,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect // 设置选中整行
            };
            // 添加复选框列
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn
            {
                HeaderText = "",
                Name = "Column1",
                ReadOnly = false, // 设置为可编辑
                Width = 30 // 设置列宽度
            };
            bottomDataGridView.Columns.Add(checkBoxColumn);
            // 添加示例数据
            var descriptionColumn = new DataGridViewTextBoxColumn { HeaderText = "描述", DataPropertyName = "Description" };
            descriptionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁用自动调整
            descriptionColumn.Width = 80;
            descriptionColumn.Name = "Description";
            bottomDataGridView.Columns.Add(descriptionColumn);
            var AuthorColumn = new DataGridViewTextBoxColumn { HeaderText = "作者", DataPropertyName = "AuthorColumn" };
            AuthorColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁用自动调整
            AuthorColumn.Width = 80;
            AuthorColumn.Name = "Author";
            bottomDataGridView.Columns.Add(AuthorColumn);
            var VersionColumn = new DataGridViewTextBoxColumn { HeaderText = "版本", DataPropertyName = "VersionColumn" };
            VersionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 禁用自动调整
            VersionColumn.Width = 80;
            VersionColumn.Name = "Version";
            bottomDataGridView.Columns.Add(VersionColumn);
            var fullPathColumn = new DataGridViewTextBoxColumn { HeaderText = "位置", DataPropertyName = "Location" };
            bottomDataGridView.Columns.Add(fullPathColumn);
            fullPathColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // 自动调整
            fullPathColumn.Name = "Location";
         
            bottomDataGridView.Rows.Add(true,"标准插件", "余鹏", "1.0.0", "C:\\Program Files (x86)\\Gamepad Tester\\Stdplugs");
            bottomDataGridView.Rows.Add(true, "扩展插件", "", "", "C:\\Program Files (x86)\\Gamepad Tester\\Plugins");
            // 订阅 CellContentClick 事件以捕获复选框状态更改
            bottomDataGridView.CellContentClick += BottomDataGridView_CellContentClick;
            splitContainer1.Panel2.Controls.Add(bottomDataGridView);
        }
        private void BottomDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == bottomDataGridView.Columns["Column1"].Index && e.RowIndex >= 0)
            {
                // 获取点击的复选框单元格
                DataGridViewCheckBoxCell checkBoxCell = bottomDataGridView[e.ColumnIndex, e.RowIndex] as DataGridViewCheckBoxCell;

                // 获取复选框的状态
                bool isChecked = (bool)checkBoxCell.Value;
                // 获取选中复选框行的标记
                string fullPath = bottomDataGridView.Rows[e.RowIndex].Cells["Location"].Value.ToString();
                Console.WriteLine($"Description to search for: {fullPath}");
                // 查找所有标记相同的插件
                var pluginsToProcess = cachedPlugins.Where(p => p.FullPath == fullPath).ToList();
                Console.WriteLine($"Number of plugins to process: {pluginsToProcess.Count}");
                // 根据复选框状态显示或隐藏topDataGridView中的数据
                foreach (var plugin in pluginsToProcess)
                {
                    Console.WriteLine($"Plugin to process: {plugin.Description}");

                    if (isChecked)
                    {
                        // 隐藏数据：移除相应的数据
                        if (pluginBindingList.Contains(plugin))
                        {
                            pluginBindingList.Remove(plugin);
                        }
                    }
                    else
                    {
                        // 显示数据：添加新的数据
                        if (!pluginBindingList.Contains(plugin))
                        {
                            pluginBindingList.Add(plugin);
                        }

                    }

                    // 刷新数据源以更新显示
                    topDataGridView.Refresh();
                    
                }
                checkBoxCell.Value = !isChecked;
            }
        }
        private void LoadDataIntoBindingList(List<Plugin> plugins)
        {
            pluginBindingList.Clear();
            foreach (var plugin in plugins)
            {
                pluginBindingList.Add(plugin);
            }
            topDataGridView.DataSource = pluginBindingList;
        }


        /*private void InitializeFilterControls()
        {
            // 描述筛选框
            Label lblDescription = new Label { Text = "描述:", Location = new System.Drawing.Point(12, 220) };
            txtFilterDescription = new TextBox { Location = new System.Drawing.Point(60, 220), Width = 150 };
            txtFilterDescription.TextChanged += FilterData;

            // 作者筛选框
            Label lblAuthor = new Label { Text = "作者:", Location = new System.Drawing.Point(220, 220) };
            txtFilterAuthor = new TextBox { Location = new System.Drawing.Point(270, 220), Width = 150 };
            txtFilterAuthor.TextChanged += FilterData;

            // 版本筛选框
            Label lblVersion = new Label { Text = "版本:", Location = new System.Drawing.Point(430, 220) };
            cmbFilterVersion = new ComboBox { Location = new System.Drawing.Point(480, 220), Width = 100 };
            cmbFilterVersion.Items.AddRange(new string[] { "所有版本", "1.0", "2.0" });
            cmbFilterVersion.SelectedIndex = 0;
            cmbFilterVersion.SelectedIndexChanged += FilterData;

            this.Controls.Add(lblDescription);
            this.Controls.Add(txtFilterDescription);
            this.Controls.Add(lblAuthor);
            this.Controls.Add(txtFilterAuthor);
            this.Controls.Add(lblVersion);
            this.Controls.Add(cmbFilterVersion);
        }

        private void FilterData(object sender, EventArgs e)
        {
            string descriptionFilter = txtFilterDescription.Text.ToLower();
            string authorFilter = txtFilterAuthor.Text.ToLower();
            string versionFilter = cmbFilterVersion.SelectedItem.ToString();

            foreach (DataGridViewRow row in bottomDataGridView.Rows)
            {
                bool descriptionMatch = row.Cells["DescriptionColumn"].Value.ToString().ToLower().Contains(descriptionFilter);
                bool authorMatch = row.Cells["AuthorColumn"].Value.ToString().ToLower().Contains(authorFilter);
                bool versionMatch = versionFilter == "所有版本" || row.Cells["VersionColumn"].Value.ToString() == versionFilter;

                row.Visible = descriptionMatch && authorMatch && versionMatch;
            }
        }
*/
        #region 右键菜单
        private void Plugins_load(object sender, EventArgs e) {
            // 创建右键菜单
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem selectAllMenuItem = new ToolStripMenuItem("选中所有行");
            selectAllMenuItem.Click += SelectAllRows;
            contextMenuStrip.Items.Add(selectAllMenuItem);
            ToolStripMenuItem deselectAllMenuItem = new ToolStripMenuItem("取消选中所有行");
            deselectAllMenuItem.Click += DeselectAllRows;
            contextMenuStrip.Items.Add(deselectAllMenuItem);
            contextMenuStrip.Items.Add(new ToolStripSeparator());///分隔符
            ToolStripMenuItem subMenuItem = new ToolStripMenuItem("Selected Plug-ins");
            ToolStripMenuItem subMenuItem1 = new ToolStripMenuItem("load");
            subMenuItem1.Click += SubMenuItem1_Click;
            subMenuItem.DropDownItems.Add(subMenuItem1);
            contextMenuStrip.Items.Add(subMenuItem);
            /*ToolStripMenuItem subMenuItem2 = new ToolStripMenuItem("操作2");
            subMenuItem2.Click += SubMenuItem2_Click;
            subMenuItem.DropDownItems.Add(subMenuItem2)*/
            ToolStripMenuItem ClearselectMenuItem = new ToolStripMenuItem("Clear Selection");
            contextMenuStrip.Items.Add(ClearselectMenuItem);
            contextMenuStrip.Items.Add(new ToolStripSeparator());///分隔符
            ToolStripMenuItem MenuItemTAG = new ToolStripMenuItem("Tagged Plug-ins");
            ToolStripMenuItem TAGMenuItem2 = new ToolStripMenuItem("load");
            TAGMenuItem2.Click += TAGMenuItem_Click;
            MenuItemTAG.DropDownItems.Add(TAGMenuItem2);
            contextMenuStrip.Items.Add(MenuItemTAG);
            ToolStripMenuItem TAGselectMenuItem = new ToolStripMenuItem("Tag Selected");
            TAGselectMenuItem.Click += TAGselectMenuItem_Click;
            contextMenuStrip.Items.Add(TAGselectMenuItem);
            ToolStripMenuItem ClearTAGMenuItem = new ToolStripMenuItem("Clear Tags");
            ClearTAGMenuItem.Click += ClearTAGMenuItem_Click;
            contextMenuStrip.Items.Add(ClearTAGMenuItem);
            contextMenuStrip.Items.Add(new ToolStripSeparator());///分隔符
            ToolStripMenuItem LoadMenuItem = new ToolStripMenuItem("Load New Plug-in....");
            LoadMenuItem.Click += LoadMenuItem_Click;
            contextMenuStrip.Items.Add(LoadMenuItem);
            contextMenuStrip.Items.Add(new ToolStripSeparator());///分隔符
            ToolStripMenuItem ReviewMenuItem = new ToolStripMenuItem("Refresh View");
            ReviewMenuItem.Click += RefreshMenuItem_Click;
            contextMenuStrip.Items.Add(ReviewMenuItem);


            // DataGridView 绑定 MouseClick 事件
            topDataGridView.MouseClick += DataGridView_MouseClick;

            // 将右键菜单绑定到 DataGridView
            topDataGridView.ContextMenuStrip = contextMenuStrip;

        }
        private void SelectAllRows(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in topDataGridView.Rows)
            {
                row.Selected = true;
            }
        }

        private void DeselectAllRows(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in topDataGridView.Rows)
            {
                row.Selected = false;
            }
        }

        private void SubMenuItem1_Click(object sender, EventArgs e)
        {
            // 处理操作1的逻辑
        }

        private void TAGMenuItem_Click(object sender, EventArgs e)
        {
            // 处理操作2的逻辑
        }
        /// <summary>
        /// 标记
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TAGselectMenuItem_Click(object sender, EventArgs e) {

            foreach (DataGridViewRow row in topDataGridView.SelectedRows)
            {
                row.Cells["Marker"].Value = "√"; // 将选定行的 "Marker" 列的值设置为对号
            }
        }
        private void ClearTAGMenuItem_Click(object sender, EventArgs e) {
            foreach (DataGridViewRow row in topDataGridView.SelectedRows)
            {
                row.Cells["Marker"].Value = ""; // 将选定行的 "Marker" 列的值设置为空字符串，取消标记
            }
        }
        /// <summary>
        /// LoadMenuItem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 
        private void LoadMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\Program Files (x86)\Gamepad Tester\Plugins"; // 指定初始目录
                openFileDialog.Filter = "Game files (*.game)|*.game|All files (*.*)|*.*";
                openFileDialog.Multiselect = true; // 允许选择多个文件

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    List<Plugin> plugins = new List<Plugin>();

                    foreach (string file in openFileDialog.FileNames)
                    {
                        var lines = File.ReadAllLines(file);
                        // 取消跳过第一行标题
                        for (int i = 0; i < lines.Length; i++)
                        {
                            {
                                var line = lines[i];
                                // 添加调试输出，查看读取到的每一行内容
                                Console.WriteLine($"Read line: {line}");
                                var parts = line.Split(',');
                                Console.WriteLine($"Number of parts: {parts.Length}");
                                if (parts.Length == 8)
                                {
                                    Plugin plugin = new Plugin
                                    {
                                        Marker = parts[0].Trim(),
                                        PluginName = parts[1].Trim(), // PluginName
                                        Description = parts[2].Trim(), //                                    
                                        Version = parts[3].Trim(),     // Version
                                        Author = parts[4].Trim(),       // Author
                                        Status = parts[5].Trim(),
                                        Size = parts[6].Trim(),
                                        FullPath = parts[7].Trim()
                                    };
                                    plugins.Add(plugin);
                                }
                                else
                                {
                                    Console.WriteLine("Line does not have 8 parts");
                                }
                            }
                        }

                        additionalPlugins.AddRange(plugins);
                        foreach (var plugin in additionalPlugins)
                        {
                            if (!pluginBindingList.Contains(plugin))
                            {
                                pluginBindingList.Add(plugin);
                            }
                        }
                        topDataGridView.DataSource = null; // 确保刷新数据源
                        topDataGridView.DataSource = additionalPlugins;
                        topDataGridView.Refresh(); // 刷新DataGridView
                       
                        // 添加调试输出，查看绑定的数据源
                        Console.WriteLine($"DataGridView Rows Count: {topDataGridView.Rows.Count}");
                    }
                }
            }

        }
        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshMenuItem_Click(object sender, EventArgs e)
        {
            // 在此处重新加载数据到 DataGridView
            DataTable newDataTable = LoadData(); // 加载新的数据
            topDataGridView.DataSource = newDataTable;
        }
        private DataTable LoadData()
        {
            // 在这里加载数据到 DataTable
            DataTable dataTable = new DataTable();

            // 假设这里执行了数据加载逻辑，填充了 dataTable

            return dataTable;
        }

        private void DataGridView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                topDataGridView.ContextMenuStrip.Show(topDataGridView, e.Location);
            }
        }
        #endregion
    }
}

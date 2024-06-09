using System;
using System.Windows.Forms;

namespace Game
{
    public partial class KeyboardSetting : Form
    {
        public KeyboardSetting()
        {
            InitializeComponent();
            this.Load += KeyboardSetting_load;
        }

        public void KeyboardSetting_load(object sender, EventArgs e)
        { // 调整菜单项之间的间距
            foreach (ToolStripItem item in menuStrip1.Items)
            {
                if (item is ToolStripMenuItem)
                {
                    ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
                    menuItem.Margin = new Padding(5, 0, 5, 0); // 设置间距
                }
            }
        }
    
    
    }
}

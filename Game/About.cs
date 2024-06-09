using System;
using System.Windows.Forms;


namespace Game
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            DisplayReleaseDate();
        }

        private void DisplayReleaseDate()
        {
            // 获取当前日期并转换为字符串格式
            string releaseDate = DateTime.Today.ToShortDateString();
            // 将发布日期显示在 Label6 上
            label6.Text = "Release Date：" + releaseDate;
        }

        private void About_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
            this.ControlBox = true;
            labelAppVersion.Text = typeof(Form1).Assembly.GetName().Version.ToString();
            
        }
    }
}

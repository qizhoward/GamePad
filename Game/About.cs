using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }


        private void About_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
            this.ControlBox = true;
        }
    }
}

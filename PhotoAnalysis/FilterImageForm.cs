using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoAnalysis
{
    public partial class FilterImageForm : Form
    {
        public FilterImageForm()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();

            fd.FileName = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            fd.DefaultExt = "jpg";
            fd.Filter = "JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(fd.FileName);
            }  
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopIconsApp
{
    public partial class Form1 : Form
    {

        public static Form1 mainForm;

        public static ListBox listbox;

        public Form1()
        {
            InitializeComponent();
            mainForm = this;
            listbox = listBox1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IntPtr handle = Class1.GetSysListView32();
            listBox1.Items.Add(handle.ToInt32().ToString("X8"));
            int count = Class1.GetDesktopItemCount(handle);
            listBox1.Items.Add(count.ToString());
        }

    }
}

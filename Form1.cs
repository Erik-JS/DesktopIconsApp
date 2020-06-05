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
            Text += IntPtr.Size == 8 ? " 64-bit" : " 32-bit";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.ForeColor = Color.Lime;
            listBox1.Items.Clear();
            IntPtr handle = Class1.GetSysListView32();
            listBox1.Items.Add("Desktop ListView Handle: " + handle.ToInt32().ToString("X8"));
            int count = Class1.GetDesktopItemCount(handle);
            listBox1.Items.Add("Item count: " + count.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox1.ForeColor = Color.Cyan;
            List<string> lstDesktopItems = Class1.GetDesktopItemTextList();
            foreach (var item in lstDesktopItems)
                listBox1.Items.Add(item);
            System.Diagnostics.Debug.Print("lstDesktopItems.Count: " + lstDesktopItems.Count.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox1.ForeColor = Color.Yellow;
            List<string> lstPositions = Class1.GetDesktopItemPositionList();
            foreach (var pos in lstPositions)
                listBox1.Items.Add(pos);
            System.Diagnostics.Debug.Print("lstPositions.Count: " + lstPositions.Count.ToString());
        }
    }
}

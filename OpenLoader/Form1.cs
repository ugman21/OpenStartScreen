using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenLoader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Opacity = 0;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("OpenStartScreen");
            if (pname.Length >= 1)
            {
                // already open in bg, unhide it.
                this.Hide();
                //Thread.Sleep(500);
                timer1.Stop();
                this.Close();
            }
            else
            {
                timer1.Stop();
                Process.Start(@"C:\Users\actium\Desktop\OpenStartScreen-master\OpenStartScreen\bin\Debug\net8.0-windows\OpenStartScreen.exe");
                this.Close();
            }
        }
    }
}

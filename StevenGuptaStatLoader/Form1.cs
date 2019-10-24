using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StevenGuptaStatLoader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // start up the routine from this button.
        private void btnLoad_Click(object sender, EventArgs e)
        {
            btnLoad.Enabled = false;
            textBox2.Text = "Loading Data";

            Application.DoEvents();

            LoadStats loadStats = new LoadStats(int.Parse(textBox1.Text));

            textBox2.Text = loadStats.Message;

            btnLoad.Enabled = true;


        }
    }
}

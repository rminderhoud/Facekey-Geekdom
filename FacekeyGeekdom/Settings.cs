using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FacekeyGeekdom
{
    public partial class Settings : Form
    {
        public string ip = "192.168.0.2";
        public int port = 4370;
        public int interval = 3000; // Milliseconds
        public TextBox[] textboxes;

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            // A list of all the textboxes or our seting
            textboxes = new TextBox[] {textIp, textPort, textInterval};

            // Initialize our variables based on hardcoded defaults
            // NOTE:    Ideally settings should be loaded from a file not
            //          hardcoded defaults
            textIp.Text = ip;
            textPort.Text = port.ToString();
            textInterval.Text = interval.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Make sure there are no blank values
            for (int i = 0; i < textboxes.Length; i++)
            {
                // If there is a blank warn the user and cancel the save
                if (textboxes[i].Text == "")
                {
                    System.Windows.Forms.MessageBox.Show("Please make sure no values are blank");
                    return;
                }
            }

            // Otherwise we update all our variables
            ip = textIp.Text;
            port = Int32.Parse(textPort.Text);
            interval = Int32.Parse(textInterval.Text);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace FacekeyGeekdom
{
    public partial class AdminForm : Form
    {
        public zkemkeeper.CZKEM Device = new zkemkeeper.CZKEM();
        public string ip = "192.168.10.2";
        public int port = 4370;

        public GeekdomAPI api = new GeekdomAPI();
        public string api_key = "RZSOY7O0FSAO65NZYF68W2OZ4BDBRBJY";

        public FacekeyDatabaseAPI db = new FacekeyDatabaseAPI();
        public string database_name = "facekeygeekdom.sqlite";

        private Thread poll_thread;
        public int poll_interval = 3000;
        private bool poll_loop = true;

        public bool user_registering;

        public AdminForm()
        {
            InitializeComponent();
        }

        // EVENT-FIRED FUNCTIONS
        private void AdminForm_Load(object sender, EventArgs e)
        {
            lbLog.Items.Add("Connecting to database "+ database_name +"...");

            // Load the database (if it doesn't exist create it)
            if (!db.LoadDatabase(database_name))
            {
                lbLog.Items.Add("Database file not found");
                lbLog.Items.Add("Creating database file...");
                if (!db.CreateDatabase(database_name))
                {
                    lbLog.Items.Add("Error creating database " + database_name);
                    return;
                }

                lbLog.Items.Add("Database file facekeygeekdom.sqlite created");
                lbLog.Items.Add("Connected to database");
                lbLog.Items.Add("Creating tables...");
                db.ExecuteNonQuery("CREATE TABLE ids (geekdom_id INT, facekey_id INT)");
                lbLog.Items.Add("Table ids<geekdom_id INT, facekey_id INT> created");
            }

            lbLog.Items.Add("Connected to database");
            
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Connecting to the device
            if (btnConnect.Text == "Connect")
            {
                bool connected;

                // Connect to the device using the library function
                connected = Device.Connect_Net(ip, port);

                // If we are successfully connected then we want
                // update the log and enable the device specific features
                if (connected)
                {
                    lbLog.Items.Add("Connected to device at " + ip + ":" + Convert.ToString(port));
                    btnConnect.Text = "Disconnect";
                    EnableDeviceButtons(true);
                }
                // Otherwise we warn the user that the connection failed
                else
                {

                    lbLog.Items.Add("Failed to connect to device at " + ip + ":" + Convert.ToString(port));
                }
            }
            // Disconnecting from the device
            else
            {
                // Disconnected from the device using the library function
                Device.Disconnect();

                // Disable the device specific features and update the log
                lbLog.Items.Add("Disconnected from the device");
                btnConnect.Text = "Connect";
                EnableDeviceButtons(false);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Start")
            {
                poll_thread = new Thread(new ThreadStart(PollDevice));
                poll_thread.Start();
                btnStart.Text = "Stop";
            }
            else
            {
                poll_loop = false;
                btnStart.Text = "Start";
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show( "This action will clear the log box. Do you wish to continue?",
                                                    "Warning",
                                                    MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                lbLog.Items.Clear();
            }
        }

        private void btnWipe_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("This action will wipe all the access logs from the device. Do you wish to continue?",
                                                    "Warning",
                                                    MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                Device.ClearData(1, 1);
            }
        }
        
        private void btnView_Click(object sender, EventArgs e)
        {
            User user_view = new User(this);
            user_view.Show();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            Settings Settings = new Settings();
            Settings.Show();
            ip = Settings.ip;
            port = Settings.port;
            poll_interval = Settings.interval;
        }
        
        // THREAD FUNCTIONS
        private void PollDevice()
        {
            string sdwEnrollNumber = "";
            int idwVerifyMode = 0;
            int idwInOutMode = 0;
            int idwYear = 0;
            int idwMonth = 0;
            int idwDay = 0;
            int idwHour = 0;
            int idwMinute = 0;
            int idwSecond = 0;
            int idwWorkcode = 0;

            int geekdom_id;

            while (poll_loop)
            {
                while (user_registering)
                {
                    ListBoxAdd(lbLog, "User is regstering, sleeping...");
                    Thread.Sleep(10000);
                }

                ListBoxAdd(lbLog, "Polling...");
                
                // Disable the device to avoid any problems getting the logs
                ListBoxAdd(lbLog, "Disabled the device...");
                Device.EnableDevice(1, false);
                ListBoxAdd(lbLog, "Device disabled");

                ListBoxAdd(lbLog, "Loading logs...");
                if (Device.ReadGeneralLogData(1))
                {
                    ListBoxAdd(lbLog, "Logs loaded");
                    ListBoxAdd(lbLog, "Enabling device...");
                    // We only need the device disabled when getting the access log
                    Device.EnableDevice(1, true);
                    ListBoxAdd(lbLog, "Device enabled");

                    ListBoxAdd(lbLog, "Checking users in...");
                    int counter = 0;
                    while (Device.SSR_GetGeneralLogData(1, out sdwEnrollNumber, out idwVerifyMode,
                            out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))
                    {

                        // Get a geekdom_id
                        geekdom_id = db.GetGeekdomID(Int32.Parse(sdwEnrollNumber));

                        if (geekdom_id != -1)
                        {
                            // Check the user in
                            api.CheckUserIn(geekdom_id, api_key);
                            counter++;
                        }   
                    }
                    ListBoxAdd(lbLog, counter + " users checked in");
                }
                else
                {
                    ListBoxAdd(lbLog, "No Logs found");
                }

                ListBoxAdd(lbLog, "Clearing logs...");
                Device.ClearData(1, 1);
                ListBoxAdd(lbLog, "Logs cleared");
                ListBoxAdd(lbLog, "Enabling device...");
                Device.EnableDevice(1, true);
                ListBoxAdd(lbLog, "Device enabled");

                ListBoxAdd(lbLog, "Sleeping...");
                Thread.Sleep(poll_interval);
            }
        }

        private void ListBoxAdd(ListBox lb, string data)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lb.Items.Add(data);
            });
        }

        // UTILITY FUNCTIONS
        private void EnableDeviceButtons(bool flag)
        {
            btnStart.Enabled = flag;
            btnClear.Enabled = flag;
            btnWipe.Enabled = flag;
            btnView.Enabled = flag;
        }
    }
}

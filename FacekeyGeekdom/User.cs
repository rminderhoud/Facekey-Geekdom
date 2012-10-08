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
    public partial class User : Form
    {
        private AdminForm admin;
        private GeekdomAPI api = new GeekdomAPI();
        private string api_key = "RZSOY7O0FSAO65NZYF68W2OZ4BDBRBJY";

        private string username;
        private int user_id;

        public User()
        {
            InitializeComponent();
        }

        public User(AdminForm form1): this()
        {
            // TODO: Complete member initialization
            this.admin = form1;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            admin.user_registering = true;

            HtmlElementCollection links = webBrowser1.Document.GetElementsByTagName("a");

            foreach (HtmlElement link in links)
            {
                string href = link.GetAttribute("href");
                string[] href_array = href.Split('/');

                
                if (String.IsNullOrEmpty(href) || href_array[3] != "accounts")
                    continue;

                
                username = href_array[4];
                user_id = api.GetUserID(username, api_key);
                
                // Create a facekey user for them
                admin.Device.SSR_SetUserInfo(1, user_id.ToString(), username, "", 0, true);

                // Create a new row in database for them
                admin.db.ExecuteNonQuery("INSERT INTO ids (geekdom_id, facekey_id) VALUES ("+user_id+","+user_id+")");
                // Pop up message box with their user id and instructions
                System.Windows.Forms.MessageBox.Show(   "Facekey ID: "+user_id + "\n\n" + 
                                                        "To finish the registration process: \n" +
                                                        "1) Go to the device Menu -> Usr Mgmt. -> Query \n" +
                                                        "2) Enter your facekey id and then select your username from the list \n" +
                                                        "3) Select FP and enroll your fingerprint \n" +
                                                        "4) Select Face and enroll your face \n" +
                                                        "Exit the menu and checkin using Facekey! \n");

                btnRegister.Enabled = false;
                webBrowser1.Navigate(new Uri("http://members.geekdom.com/logout/"));
                webBrowser1.Navigate(new Uri("http://members.geekdom.com/accounts/signin/"));
                admin.user_registering = false;
                return;
            }   
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            string url = webBrowser1.Url.ToString();
            string url_sub = url.Substring(0, 36);

            // List of acceptable URLS
            if (url == "http://members.geekdom.com/logout/")
                return;

            // Blocks all urls except ones starting with the substring below
            if (url_sub != "http://members.geekdom.com/accounts/")
                webBrowser1.Navigate(new Uri("http://members.geekdom.com/accounts/signin/"));

            if (url != "http://members.geekdom.com/accounts/signin/")
                btnRegister.Enabled = true;
        }
    }
}

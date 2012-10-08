using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace FacekeyGeekdom
{
    public class GeekdomAPI
    {
        public bool CheckUserIn(int geekdom_id, string api_key)
        {
            // this is what we are sending
            string data = "user_id=" + geekdom_id + "&api_key=" + api_key;

            // this is where we will send it
            string uri = "http://members.geekdom.com/api/users/checkin/";

            // create a request
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(uri); request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            // turn our request string into a byte stream
            byte[] postBytes = Encoding.ASCII.GetBytes(data);

            // this is important - make sure you specify type this way
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab the response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Console.WriteLine(new StreamReader(response.GetResponseStream()).ReadToEnd());
            Console.WriteLine(response.StatusCode);

            return true;
        }

        public int GetUserID(string username, string api_key)
        {
            string url;

            url = "http://members.geekdom.com/api/users/get/user_id/" + username + "/?api_key=" + api_key;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream objStream;
            objStream = response.GetResponseStream();

            StreamReader objReader = new StreamReader(objStream);
            string response_data = objReader.ReadLine();

            string[] parsed_data = response_data.Split('\"');
            string parsed_id = parsed_data[14];
            parsed_id = parsed_id.Remove(0, 2);
            parsed_id = parsed_id.Remove(parsed_id.Length - 2, 2);
            
            return Int32.Parse(parsed_id);
        }
    }
}

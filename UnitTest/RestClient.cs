using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace UnitTest
{
    public class RestClient
    {
        public String Method { get; set; }
        public String EndPoint { get; set; }
        public String ContentType { get; set; }

        public RestClient(String method = "GET", String endPoint = "", String contentType = "text/plain")
        {
            this.EndPoint = endPoint;
            this.Method = method;
            this.ContentType = contentType;
        }

        public HttpWebResponse MakeRequest(String parameters = "", String postData = "", String token = "")
        {
            HttpWebRequest request = WebRequest.Create(EndPoint + parameters) as HttpWebRequest;

            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;

            if (!String.IsNullOrEmpty(token)) request.Headers.Add("X-Auth-Token", token);

            if (Method == "POST" && !String.IsNullOrEmpty(postData))
            {
                Byte[] bytes = Encoding.Default.GetBytes(postData);
                request.ContentLength = bytes.Length;

                using (Stream writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }

            // System.Diagnostics.Debug.WriteLine(request);

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            return response;
        }
    }
}
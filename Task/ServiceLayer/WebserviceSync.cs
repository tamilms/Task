using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net;
using System.IO;
using System.Xml.Linq;
using Android.Graphics;


namespace Task.ServiceLayer
{
    public class WebserviceSync
    {
        String OutputString = string.Empty;
         XDocument doc;
         Bitmap bitmapImage;
      
        public Bitmap GetImageURLFromURL(String ServiceAddress)
        {
            string ParameterString = string.Empty;
            try
            {

                #region

                try
                {
                    if (ServiceAddress != null)
                    {

                        var url = new System.Uri(ServiceAddress);
                        var request = HttpWebRequest.Create(url);
                        request.Method = "POST";
                        request.Timeout = 180000;
                        var sw = new StreamWriter(request.GetRequestStream());
                        sw.Write(url.ToString());
                        sw.Close();
                        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                var content = reader.ReadToEnd();
                                if (content != "")
                                {
                                    XDocument doc = XDocument.Parse(content);

                                    foreach (XElement ele in doc.Root.Elements("pattern"))
                                    {
                                        OutputString = (string)ele.Element("imageUrl");
                                    }
                                    bitmapImage = GetImageBitmapFromUrl(OutputString);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                }

                #endregion
            }
            catch (Exception e)
            {
            }
            return bitmapImage;
        }

        public String GetHexcodeURLFromURL(String ServiceAddress)
        {
            string ParameterString = string.Empty;
            try
            {

                #region

                try
                {
                   
                    if (ServiceAddress != null)
                    {

                        var url = new System.Uri(ServiceAddress);
                        var request = HttpWebRequest.Create(url);
                        request.Method = "POST";
                        request.Timeout = 180000;
                        var sw = new StreamWriter(request.GetRequestStream());
                        sw.Write(url.ToString());
                        sw.Close();
                        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                var content = reader.ReadToEnd();
                                if (content != "")
                                {
                                    XDocument doc = XDocument.Parse(content);

                                    foreach (XElement ele in doc.Root.Elements("color"))
                                    {
                                        OutputString = (string)ele.Element("hex");
                                    }
                                   
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    OutputString = "ServerError";
                }

                #endregion
            }
            catch (Exception e)
            {
                
            }
            return OutputString;
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            try
            {
                using (var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(url);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return imageBitmap;
        }

     

    }
}
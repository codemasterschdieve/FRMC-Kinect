

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;

namespace FRMC_Kinect
{
   public class Ftp
    {
        public void modelupload(string user_Id, string filename)
        {

            try { 
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://www.frmc.wi-stuttgart.de/" + user_Id + "model.jpg");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("f009fabd", "qwa1qwa1");

            // Copy the contents of the file to the request stream.
          //  StreamReader sourceStream = new StreamReader(@"F:\test.png");
                 // byte [] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());

           byte[] fileContents = File.ReadAllBytes(filename);

          //  sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
            System.Diagnostics.Debug.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
            response.Close();
        }

            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }


        public void scanupload(string filename)
        {


             try {
                 // Get the object used to communicate with the server.
                 FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://www.frmc.wi-stuttgart.de/scan.jpg");
                 request.Method = WebRequestMethods.Ftp.UploadFile;

                 // This example assumes the FTP site uses anonymous logon.
                 request.Credentials = new NetworkCredential("f009fabd", "qwa1qwa1");

                 // Copy the contents of the file to the request stream.
                 //  StreamReader sourceStream = new StreamReader(@"F:\test.png");
                 // byte [] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());

                 byte[] fileContents = File.ReadAllBytes(filename);

                 //  sourceStream.Close();
                 request.ContentLength = fileContents.Length;

                 Stream requestStream = request.GetRequestStream();
                 requestStream.Write(fileContents, 0, fileContents.Length);
                 requestStream.Close();

                 FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                 Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                 System.Diagnostics.Debug.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                 response.Close();
        }
    

      catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        public bool checkScanUploadImage()
        {
            bool isUploaded = false;

            var request = (HttpWebRequest)WebRequest.Create(Constants.uploadScanImageURL);
            request.Method = "HEAD";

            var response = (HttpWebResponse)request.GetResponse();

            isUploaded = response.StatusCode == HttpStatusCode.OK;

            return isUploaded;
        }


    }
}

       

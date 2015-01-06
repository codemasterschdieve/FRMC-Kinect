

using System;
using System.IO;
using System.Net;
using System.Text;

namespace FRMC_Kinect
{
    public class ftp
    {
        public void bildupload()
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://www.frmc.wi-stuttgart.de/scan3.png");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("f009fabd", "qwa1qwa1");

            // Copy the contents of the file to the request stream.
          //  StreamReader sourceStream = new StreamReader(@"F:\test.png");
                 // byte [] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());

           byte[] fileContents = File.ReadAllBytes("F:\\test.png");

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
    }
}


       

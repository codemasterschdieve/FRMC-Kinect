using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using KeyLemon;
using System.Text.RegularExpressions;
using System.Windows.Navigation;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Drawing;




namespace FRMC_Kinect
{
 
    public partial class FRMC_Window : Window, INotifyPropertyChanged
    {

     
       


     // api.CreateFaceModel

        #region membervariablen definition

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;


        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;


        /// <summary>
        /// Description of the data contained in the color index frame
        /// </summary>
        private FrameDescription colorFrameDescription = null;


        /// <summary>
        /// Description of the data contained in the color index frame
        /// </summary>
        private DepthFrameReader depthFrameReader = null;


        /// <summary>
        /// Description of the data contained in the depth index frame
        /// </summary>
        private FrameDescription depthFrameDescription = null;


        /// <summary>
        /// Array of DepthData
        /// </summary>
        private ushort[] depthData;


        /// <summary>
        /// Reader for body index frames
        /// </summary>
        private BodyIndexFrameReader bodyIndexFrameReader = null;


        /// <summary>
        /// Description of the data contained in the body index frame
        /// </summary>
        private FrameDescription bodyIndexFrameDescription = null;


        /// <summary>
        /// Single Body Representation
        /// </summary>
        private Body body = null;


        /// <summary>
        /// Array of Body Representations
        /// </summary>
        private Body[] bodies = null;


        /// <summary>
        /// Reader for face index frames
        /// </summary>
        private FaceFrameReader faceFrameReader = null;


        /// <summary>
        /// Description of the data contained in the face index frame
        /// </summary>
        private FrameDescription faceFrameDescription = null;


        /// <summary>
        /// Reader for depth/color/body index frames
        /// </summary>
        private MultiSourceFrameReader multiFrameSourceReader = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;


        /// <summary>
        /// bytesPerPixel
        /// </summary>
        private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// BodyBitmap to display
        /// </summary>
        private WriteableBitmap bodyBitmap = null;


        /// <summary>
        /// FaceBitmap to display
        /// </summary>
        private WriteableBitmap faceBitmap = null;

        /// <summary>
        /// FaceBitmap to display
        /// </summary>
        private BitmapImage faceImage = null;


        /// <summary>
        /// Bytearray to save image in mySQL DB
        /// </summary>
        private Byte[] imagedata = null;


        public ImageSource kinectImageSource
        {
            get
            {
                return this.bodyBitmap;
            }
        }

        public ImageSource faceImageSource
        {
            get
            {
                return this.faceImage;
            }
        }



        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

       


        public FRMC_Window()
        {

          


             // Connect to MySQL Database

                    //generate the connection string
                    string connStr = CreateConnStr("www.wi-stuttgart.de", "d01c6657", "d01c6657", "hdm123!");

                    //create a MySQL connection with a query string
                    MySqlConnection connection = new MySqlConnection(connStr);

                    //open the connection
                    connection.Open();

                    MySqlCommand cmd = connection.CreateCommand();

            cmd.CommandText="SELECT Picture FROM User WHERE UserId='17'";
            cmd.Prepare();
            var imageData = cmd.ExecuteNonQuery();
            System.Diagnostics.Debug.WriteLine("--> "+imageData.GetType());
           

     //       KLAPI api = new KLAPI("tobi0604@me.com", "qwa1qwa1", "https://api.keylemon.com");

     //       // Create a model using the URLs, the data and the face_list
     //dynamic response = api.CreateFaceModel(null, new byte[1][] { imageData2 }, null, "Tobi Model");


     //       // To read back the model at a later time:
     //      response = api.GetModel(response["model_id"]);

     //       // And delete the model:
     //       response = api.DeleteModel(response["model_id"]);


            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();


            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // get colorFrameDescription
            this.colorFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;


            // open the reader for the depth frames
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();

            // get depthFrameDescription
            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;


            // open the reader for the body index frames
            this.bodyIndexFrameReader = this.kinectSensor.BodyIndexFrameSource.OpenReader();

            // get bodyIndexFrameDescription
            this.bodyIndexFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;


            // open the reader for the multispouce frames
            this.multiFrameSourceReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);


            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescriptionBgra = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create faceBitmap
            this.faceBitmap = new WriteableBitmap(colorFrameDescriptionBgra.Width, colorFrameDescriptionBgra.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

            // create bodyBitmap
            this.bodyBitmap = new WriteableBitmap(colorFrameDescriptionBgra.Width, colorFrameDescriptionBgra.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

            // get coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            //// Handler for frame arrival
           this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
            //this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_FrameArrived;


            // open the sensor
            this.kinectSensor.Open();

            // use the window object as the view model in this simple example
            this.DataContext = this;



            InitializeComponent();
        }



        #region mysql connection
        /// <summary>
        /// Generates a connection string
        /// </summary>
        /// <param name="server">The name or IP of the server where the MySQL server is running</param>
        /// <param name="databaseName">The name of the database </param>
        /// <param name="user">The user id - root if there are no new users which have been created</param>
        /// <param name="pass">The user's password</param>
        /// <returns></returns>
        public static string CreateConnStr(string server, string databaseName, string user, string pass)
        {
            //build the connection string
            string connStr = "server=" + server + ";database=" + databaseName + ";uid=" +
                user + ";password=" + pass + ";";

            //return the connection string
            return connStr;
        }
        #endregion

       


        #region ColorFrameArrived Handler to get bodyBitmap
        /// <summary>
        /// Triggered for each color frame to get a writable bitmap for each color frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.bodyBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.bodyBitmap.PixelWidth) && (colorFrameDescription.Height == this.bodyBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.bodyBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.bodyBitmap.AddDirtyRect(new Int32Rect(0, 0, this.bodyBitmap.PixelWidth, this.bodyBitmap.PixelHeight));

                        }

                        this.bodyBitmap.Unlock();
                    }
                }
            }
        }
        #endregion


        #region create thumbnail
        /// <summary>
        /// Saves the writable bitmap in the local file system
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="image"></param>
        public void CreateThumbnail(string filename, BitmapSource image)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                    stream.Close();
                }
            }
        }
        #endregion

    }
        
}   
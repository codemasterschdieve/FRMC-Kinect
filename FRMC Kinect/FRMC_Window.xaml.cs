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
using System.Windows.Threading;







namespace FRMC_Kinect
{
 
    public partial class FRMC_Window : Window, INotifyPropertyChanged
    {





        #region members

        /// <summary>
        /// Instantiate mySqlController to get access to the remote mysql database
        /// </summary>
        MySqlController mySqlController = new MySqlController();
       

        /// <summary>
        /// Instantiate active Kinect sensor
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
        /// Single body representation
        /// </summary>
        private Body body = null;


        /// <summary>
        /// Array of body representations
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
        /// Define bytesPerPixel
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
        /// FaceImage to display
        /// </summary>
        private BitmapImage faceImage = null;


        /// <summary>
        /// Bytearray to save image in mySQL DB
        /// </summary>
        private Byte[] imagedata = null;

        //Stellte Gesten Commands für den MediaPlayer zu verfügung
        
        /// <summary>
        /// Instantiate gestureCommands to make the gesture commands for the media player available
        /// </summary>
        private GestureCommands gestureCommands = new GestureCommands();


        /// <summary>
        /// Instantiate keylemon to get access to the keylemon api  
        /// </summary>
        KeyLemon klemon = new KeyLemon();

        /// <summary>
        /// Instantiate userList to save user objects
        /// </summary>
        List<User> userList = new List<User>();
        
        /// <summary>
        /// Instantiate timer for exectue functions in time intervalls
        /// </summary>
        DispatcherTimer timerUpload = new DispatcherTimer();

        /// <summary>
        /// Instantiate timer for exectue functions in time intervalls
        /// </summary>
        DispatcherTimer timerasyncRecognizeUserFace = new DispatcherTimer();

        // <summary>
        /// Instantiate timer for exectue functions in time intervalls
        /// </summary>
        DispatcherTimer timerasyncScanSaveLocal= new DispatcherTimer();

        /// <summary>
        /// Starting timer flag
        /// </summary>
        bool starttimer = true;

        /// <summary>
        /// Declare filename path
        /// </summary>
        string filename;

        /// <summary>
        /// Instantiate ftp object to get access to the ftp server functions
        /// </summary>
        Ftp ftpup2 = new Ftp();


        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        BitmapImage currentGestureImage = new BitmapImage(new Uri(@"/Images/handup.png", UriKind.Relative));

        /// <summary>
        /// Returns bodyBitmap as image source
        /// </summary>
        public ImageSource kinectImageSource
        {
            get
            {
                return this.bodyBitmap;
            }
        }

        /// <summary>
        /// Returns faceImage as image source
        /// </summary>
        public ImageSource faceImageSource
        {
            get
            {
                return this.faceImage;
            }
        }

        /// <summary>
        /// Returns faceImage as image source
        /// </summary>
        public BitmapImage GestureImageSource
        {
            get
            {
                return currentGestureImage;
            }
    
        }


        


            

        

        

        #endregion

       

        /// <summary>
        /// Constructor
        /// </summary>
        public FRMC_Window()
        {

          


             // Connect to MySQL Database

                    //generate the connection string
            //        string connStr = CreateConnStr("www.wi-stuttgart.de", "d01c6657", "d01c6657", "hdm123!");

            //        //create a MySQL connection with a query string
            //        MySqlConnection connection = new MySqlConnection(connStr);

            //        //open the connection
            //        connection.Open();

            //        MySqlCommand cmd = connection.CreateCommand();

            //cmd.CommandText="SELECT Picture FROM User WHERE UserId='17'";
            //cmd.Prepare();
            //var imageData = cmd.ExecuteNonQuery();
            //System.Diagnostics.Debug.WriteLine("--> "+imageData.GetType());
           

     //       KLAPI api = new KLAPI("tobi0604@me.com", "qwa1qwa1", "https://api.keylemon.com");

     //       // Create a model using the URLs, the data and the face_list
     //dynamic response = api.CreateFaceModel(null, new byte[1][] { imageData2 }, null, "Tobi Model");


     //       // To read back the model at a later time:
     //      response = api.GetModel(response["model_id"]);

     //       // And delete the model:
     //       response = api.DeleteModel(response["model_id"]);


           



            InitializeComponent();
        }

        /// <summary>
        /// Window Closing Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            mySqlController.closeConnection();

        }

        /// <summary>
        /// Window Loading Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataWindow_Open(object sender, RoutedEventArgs e)
        {


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
            //Für GestureCommands
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
            //this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            this.multiFrameSourceReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            //this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_FrameArrived;


            // open the sensor
            this.kinectSensor.Open();

            // use the window object as the view model in this simple example
            this.DataContext = this;



        }

        #region mysql connection
        ///// <summary>
        ///// Generates a connection string
        ///// </summary>
        ///// <param name="server">The name or IP of the server where the MySQL server is running</param>
        ///// <param name="databaseName">The name of the database </param>
        ///// <param name="user">The user id - root if there are no new users which have been created</param>
        ///// <param name="pass">The user's password</param>
        ///// <returns></returns>
        //public static string CreateConnStr(string server, string databaseName, string user, string pass)
        //{
        //    //build the connection string
        //    string connStr = "server=" + server + ";database=" + databaseName + ";uid=" +
        //        user + ";password=" + pass + ";";

        //    //return the connection string
        //    return connStr;
        //}
        #endregion

       


        #region ColorFrameArrived Handler to get bodyBitmap
        /// <summary>
        /// Triggered for each color frame to get a writable bitmap for each color frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        //{
        //    // ColorFrame is IDisposable
        //    using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
        //    {
        //        if (colorFrame != null)
        //        {
        //            FrameDescription colorFrameDescription = colorFrame.FrameDescription;

        //            using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
        //            {
        //                this.bodyBitmap.Lock();

        //                // verify data and write the new color frame data to the display bitmap
        //                if ((colorFrameDescription.Width == this.bodyBitmap.PixelWidth) && (colorFrameDescription.Height == this.bodyBitmap.PixelHeight))
        //                {
        //                    colorFrame.CopyConvertedFrameDataToIntPtr(
        //                        this.bodyBitmap.BackBuffer,
        //                        (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
        //                        ColorImageFormat.Bgra);

        //                    this.bodyBitmap.AddDirtyRect(new Int32Rect(0, 0, this.bodyBitmap.PixelWidth, this.bodyBitmap.PixelHeight));

        //                }

        //                this.bodyBitmap.Unlock();
        //            }
        //        }
        //    }


        //    filename = "C:\\Kinect\\scan.jpg";
        //    //CreateThumbnail2(filename, bodyBitmap);

        //    //ftpup2.scanupload(filename);
        //    //if(starttimer)
        //    if (false)
        //    {
        //        timerasyncScanSaveLocal.Tick += new EventHandler(executeScanLocalImageTimerAsynch);
        //        timerasyncScanSaveLocal.Interval = new TimeSpan(0, 0, 8);
        //        timerasyncScanSaveLocal.Start();

        //        timerUpload.Tick += new EventHandler(executeUploadTimer);
        //        timerUpload.Interval = new TimeSpan(0, 0, 15);
        //        timerUpload.Start();

        //        timerasyncRecognizeUserFace.Tick += new EventHandler(exectuteRecognizeUserFaceTimerAsync);
        //        timerasyncRecognizeUserFace.Interval = new TimeSpan(0, 0, 21);
        //        timerasyncRecognizeUserFace.Start();
        //        this.starttimer = false;
        //    }

        //}
        #endregion

        /// <summary>
        /// Für GestureCommands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            using (ColorFrame colorFrame = reference.ColorFrameReference.AcquireFrame())
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


                filename = "C:\\Kinect\\scan.jpg";
                //CreateThumbnail2(filename, bodyBitmap);

                //ftpup2.scanupload(filename);
                //if(starttimer)
                if (starttimer)
                {
                    timerasyncScanSaveLocal.Tick += new EventHandler(executeScanLocalImageTimerAsynch);
                    timerasyncScanSaveLocal.Interval = new TimeSpan(0, 0, 8);
                    timerasyncScanSaveLocal.Start();

                    timerUpload.Tick += new EventHandler(executeUploadTimer);
                    timerUpload.Interval = new TimeSpan(0, 0, 15);
                    timerUpload.Start();

                    timerasyncRecognizeUserFace.Tick += new EventHandler(exectuteRecognizeUserFaceTimerAsync);
                    timerasyncRecognizeUserFace.Interval = new TimeSpan(0, 0, 21);
                    timerasyncRecognizeUserFace.Start();
                    this.starttimer = false;
                }
            
            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(bodies);

                    foreach (var body in bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];                             

                                // Find the hand states
                                string rightHandState = "-";

                                bool isValidGesture = false;

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rightHandState = "Open";
                                        isValidGesture = true;
                                        break;
                                    case HandState.Closed:
                                        rightHandState = "Closed";
                                        isValidGesture = true;
                                        break;
                                    case HandState.Lasso:
                                        rightHandState = "Lasso";
                                        isValidGesture = true;
                                        break;
                                    default:
                                        break;
                                }
                                
                                try
                                {
                                    if (isValidGesture)
                                    {
                                        string currentCommmand = gestureCommands.InitializeMediaPlayerActions(rightHandState, userList);
                                        CurrentGestureTextBlock.Text = currentCommmand;
                                        ChangeGestureImage(rightHandState);

                                    }
                                    else
                                    {
                                        CurrentGestureTextBlock.Text = "Keine Geste erkannt";
                                        currentGestureImage = null;
                                    }
                                    
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                                }
                                //gestureCommands.LogArea = logArea;
                            }
                        }
                    }
                }
            }

        }

        private void ChangeGestureImage(string gesture)
        {
            var binding = new Binding { Source = currentGestureImage };
            GestureIcon.SetBinding(System.Windows.Controls.Image.SourceProperty, binding);
            

            switch (gesture)
            {
                case "Open":
                    currentGestureImage = new BitmapImage(new Uri(@"/Images/FlacheHand.png", UriKind.Relative));
                    break;
                case "Closed":
                    currentGestureImage = new BitmapImage(new Uri(@"/Images/StopGesture.png", UriKind.Relative));
                    break;
                case "Lasso":
                    currentGestureImage = new BitmapImage(new Uri(@"/Images/handup.png", UriKind.Relative));
                    break;
                default:
                    currentGestureImage = null;
                    break;
            }
        }

        #region time triggered functions

        private void executeScanLocalImageTimerAsynch(object sender, EventArgs e)
        {
            CreateThumbnail2(filename, bodyBitmap);
        }

        /// <summary>
        /// Uploads the picure every 7 seconds to the ftp server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void executeUploadTimer(object sender, EventArgs e)
        {
            timerasyncScanSaveLocal.Stop();

            ftpup2.scanupload(filename);

            timerasyncScanSaveLocal.Start();

        }

        /// <summary>
        /// Activates the facerecognition function of keylemon every 10 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exectuteRecognizeUserFaceTimerAsync(object sender, EventArgs e)
        {
            //schau ob bild erreibar auf ftp
            //wenn erreichner -> executeTimer Pause
            //recognize()
            //wenn fertig starte timer wieder

            bool pictureAvailable = ftpup2.checkScanUploadImage();

            if (pictureAvailable)
            {
                timerasyncScanSaveLocal.Stop();
                timerUpload.Stop();
                timerasyncRecognizeUserFace.Stop();

                klemon.RecognizeUserFace();
                findUserIdbyModelId();

                timerasyncScanSaveLocal.Start();
                timerUpload.Start();
                timerasyncRecognizeUserFace.Start();
            }


        }

        
        #endregion


        #region create thumbnail
        ///// <summary>
        ///// Saves the writable bitmap in the local file system
        ///// </summary>
        ///// <param name="filename"></param>
        ///// <param name="image"></param>
        //public void CreateThumbnail(string filename, BitmapSource image)
        //{
        //    if (filename != string.Empty)
        //    {
        //        using (FileStream stream = new FileStream(filename, FileMode.Create))
        //        {
        //            PngBitmapEncoder encoder = new PngBitmapEncoder();
        //            encoder.Frames.Add(BitmapFrame.Create(image));
        //            encoder.Save(stream);
        //            stream.Close();
        //        }
        //    }
        //}
        #endregion

        #region navigation
        /// <summary>
        /// Navigate to the main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void navigateBtnMainWindow_Click(object sender, RoutedEventArgs e)
        {
            var newwindow = new MainWindow(); //create your new form.
            newwindow.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }


        /// <summary>
        /// Navigate to the register window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void navigateBtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var newwindow = new Register();
            newwindow.Show();
            this.Close();
        }
        #endregion



        


        #region create thumbnail
        /// <summary>
        /// Saves the writable bitmap in the local file system
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="image"></param>
        public void CreateThumbnail2(string filename, BitmapSource image)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                    stream.Close();
                }
            }
        }
        #endregion

        public void findUserIdbyModelId() {

            while (ListBox1.SelectedItems.Count > 0)
            {
                ListBox1.Items.Remove(ListBox1.Items);
            }

            //Alle erkannten user löschen
            userList.Clear();
                      
            foreach (var modelId in klemon.ErkannteModels)
            {
                User user = new User();
                user.ModelId = modelId;
                user = mySqlController.findUserWithGenreByModelId(user);  
         
                userList.Add(user);

                ListBox1.Items.Add(user.Vorname + " " + user.Nachname);
            }          
        }

        public void test_Click(Object sender, RoutedEventArgs args)
        {
            //GenreFinder genreFinder = new GenreFinder();
            List<string> genresUser1 = new List<string>() { "hip-hop", "rock", "classic" };
            List<string> genresUser2 = new List<string>() { "rock", "electro", "funk", "classic" };

            List<List<string>> genreLists = new List<List<string>>();
            genreLists.Add(genresUser1);
            genreLists.Add(genresUser2);

            string match = GenreFinder.FindMatch(genreLists);

            MessageBox.Show(match);

            //User user = new User();
            //user.ModelId = "0b3dba48-9779-421f-b06c-743e06c21e15";
            //user = mySqlController.findUserWithGenreByModelId(user);

            //string genreIdsString = string.Join(",", user.MusicGenres.ToArray());
            //string genreNamesString = string.Join(",", user.MusicGenreNames.ToArray());

            //MessageBox.Show("user: " + user.Vorname + " ids: " + genreIdsString + " genre names: " + genreNamesString);
        }       
    }
        
}   
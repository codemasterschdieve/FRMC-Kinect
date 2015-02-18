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



///@author Tobias Moser, Jan Plank, Stefan Sonntag



namespace FRMC_Kinect
{

    /// <summary>
    /// Interaction logic for FRMC_Window.xaml
    /// </summary>
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
        KeyLemonController klemon = new KeyLemonController();

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
        DispatcherTimer timerRecognizeUserFace = new DispatcherTimer();

        // <summary>
        /// Instantiate timer for exectue functions in time intervalls
        /// </summary>
        DispatcherTimer timerScanSaveLocal = new DispatcherTimer();

        /// <summary>
        /// Starting timer flag
        /// </summary>
        bool starttimer = false;

        /// <summary>
        /// Declare filename path
        /// </summary>
        string filename;

        /// <summary>
        /// Instantiate ftp object to get access to the ftp server functions
        /// </summary>
        FTPController ftpup2 = new FTPController();


        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        BitmapImage currentGestureImage;

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


        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public FRMC_Window()
        {
            InitializeComponent();
            this.Show();
        }
        #endregion


        #region closing handler
        /// <summary>
        /// Window Closing Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FRMCDataWindow_Closing(object sender, CancelEventArgs e)
        {
            mySqlController.closeConnection();
        }
        #endregion


        #region loading handler
        /// <summary>
        /// Window Loading Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FRMCDataWindow_Open(object sender, RoutedEventArgs e)
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

            // open the sensor
            this.kinectSensor.Open();

            // use the window object as the view model in this simple example
            this.DataContext = this;

            /**********************************************************************************************
             * Timer initialisieren
             * ********************************************************************************************/
            timerScanSaveLocal.Tick += new EventHandler(executeScanLocalImageTimerAsynch);
            timerScanSaveLocal.Interval = new TimeSpan(0, 0, 1);

            timerUpload.Tick += new EventHandler(executeUploadTimerAsynch);
            timerUpload.Interval = new TimeSpan(0, 0, 4);

            timerRecognizeUserFace.Tick += new EventHandler(exectuteRecognizeUserFaceTimerAsync);
            timerRecognizeUserFace.Interval = new TimeSpan(0, 0, 10);


        }
        #endregion


        #region MultiSourceFrameArrived Handler
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

            // local filepath where the scan picture is saved
            filename = "C:\\Kinect\\scan.jpg";


            //Timer Starten wenn der Scan Button gedrückt wurde
            if (starttimer)
            {
                timerScanSaveLocal.Start();

                timerUpload.Start();

                timerRecognizeUserFace.Start();

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
        #endregion


        #region change gesture image
        /// <summary>
        /// Change the gesture image in dependeny on the gesture
        /// </summary>
        /// <param name="gesture"></param>
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
        #endregion


        #region time triggered functions

        private void executeScanLocalImageTimerAsynch(object sender, EventArgs e)
        {
            timerScanSaveLocal.Stop();

            CreateThumbnail2(filename, bodyBitmap);
            Console.WriteLine("Lokale Scan Datei gespeichert.");

        }

        /// <summary>
        /// Uploads the picure every 7 seconds to the ftp server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void executeUploadTimerAsynch(object sender, EventArgs e)
        {
            timerScanSaveLocal.Stop();
            timerUpload.Stop();

            ftpup2.scanupload(filename);
            Console.WriteLine("Lokale Scan Datei auf FTP hochgeladen");

            //timerScanSaveLocal.Start();

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
                timerScanSaveLocal.Stop();
                timerUpload.Stop();
                timerRecognizeUserFace.Stop();

                klemon.RecognizeUserFace();
                Console.WriteLine("Gesichter von KeyLemon erkannt.");
                //var task = klemon.RunRecognizeUserFaceAsAsync();
                //await task;

                findUserIdbyModelId();
                findUserIdbyModelId2();

                //timerasyncScanSaveLocal.Start();
                //timerUpload.Start();
                //timerasyncRecognizeUserFace.Start();
            }


        }


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
            var newwindow = new Registration_Window();
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


        #region find user by modelid
        /// <summary>
        /// get user by modelid
        /// </summary>
        public void findUserIdbyModelId2()
        {
            List<string> vornachuser = new List<string>();

            if (Listviewscore.Items.Count > 0)
            {

                Listviewscore.Items.Clear();
            }

            //Alle erkannten user löschen


            foreach (var modelId in klemon.Allemodelids)
            {
                User detectedUser2 = new User();
                detectedUser2.ModelId = modelId;
                detectedUser2 = mySqlController.findUserWithGenreByModelId(detectedUser2);


                vornachuser.Add(detectedUser2.Vorname + " " + detectedUser2.Nachname);



            }
            for (int i = 0; i < klemon.Score.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine(vornachuser.ElementAt(i));

                Listviewscore.Items.Add(vornachuser.ElementAt(i) + " Score:" + klemon.Score.ElementAt(i).ToString());


            }

        }

        public void findUserIdbyModelId()
        {


            if (ListBox1.Items.Count > 0)
            {

                ListBox1.Items.Clear();
            }

            //Alle erkannten user löschen
            userList.Clear();

            foreach (var modelId in klemon.ErkannteModels)
            {
                User detectedUser = new User();
                detectedUser.ModelId = modelId;
                detectedUser = mySqlController.findUserWithGenreByModelId(detectedUser);

                userList.Add(detectedUser);
                ListBox1.Items.Add(detectedUser.Vorname + " " + detectedUser.Nachname);



            }
        }
        #endregion


        #region timer event click handler
        /// <summary>
        /// Click Handler that is activated by clicking the scannen button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void startTimer_Click(Object sender, RoutedEventArgs args)
        {
            this.starttimer = true;
        }
        #endregion

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

        }
    }

}
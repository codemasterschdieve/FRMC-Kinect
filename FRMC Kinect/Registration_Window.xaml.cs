using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Navigation;

using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;
using System.Threading;

///@author Tobias Moser, Jan Plank, Stefan Sonntag

namespace FRMC_Kinect
{
    /// <summary>
    /// Interaction logic for Registration_Window.xaml
    /// </summary>
    public partial class Registration_Window : Window,INotifyPropertyChanged
    {

        #region members

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
        private Byte[] imagedata =null;


        /// <summary>
        /// Initialize MySqlController Instance to get access to mysql functions 
        /// </summary>
        MySqlController mySqlController = new MySqlController();


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


        /// <summary>
        /// Initialize KeyLemonController Instance to use keylemon api
        /// </summary>
        KeyLemonController klemon = new KeyLemonController();


        /// <summary>
        /// Initialize FTPController Instance to use ftp functions for uploading images 
        /// </summary>
        FTPController ftpup = new FTPController();

        
        /// <summary>
        /// filename
        /// </summary>
        string filename;


        /// <summary>
        /// filename
        /// </summary>
        string UserIdGlobal;


        /// <summary>
        /// Initialize Constants Instance to get access to constants
        /// </summary>
        Constants constants = new Constants();




        #endregion


        #region constructor
        public Registration_Window()
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
            this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_FrameArrived;


            // open the sensor
            this.kinectSensor.Open();

            // use the window object as the view model in this simple example
            this.DataContext = this;


            InitializeComponent();
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


        #region MultiSourceFrameArrived Handler to get coordinates for the face and display faceBitmap
        /// <summary>
        /// ToDo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reader_FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {



            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            BodyFrame bodyFrame = null;
            FaceFrame faceFrame = null;

            try
            {

                MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

                // If the Frame has expired by the time we process this event, return.
                if (multiSourceFrame == null)
                {
                    return;
                }

                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame();



                // If any frame has expired by the time we process this event, return.
                // The "finally" statement will Dispose any that are not null.
                if ((depthFrame == null) || (colorFrame == null) || (bodyIndexFrame == null) || (bodyFrame == null))
                {
                    return;
                }




                if (depthData == null)
                {
                    uint depthSize = this.kinectSensor.DepthFrameSource.FrameDescription.LengthInPixels;
                    depthData = new ushort[depthSize];
                }


                // load depth frame into ushort[]
                depthFrame.CopyFrameDataToArray(depthData);


                var dataReceived = false;

                if (bodyFrame != null)
                {
                    if (bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }

                if (dataReceived)
                {
                    foreach (var body in bodies)
                    {
                        if (!body.IsTracked)
                            continue;

                        if (this.body == null)
                        {



                            // specify the required face frame results
                            FaceFrameFeatures faceFrameFeatures =
                                FaceFrameFeatures.BoundingBoxInColorSpace
                                | FaceFrameFeatures.PointsInColorSpace
                                | FaceFrameFeatures.RotationOrientation
                                | FaceFrameFeatures.FaceEngagement
                                | FaceFrameFeatures.Glasses
                                | FaceFrameFeatures.Happy
                                | FaceFrameFeatures.LeftEyeClosed
                                | FaceFrameFeatures.RightEyeClosed
                                | FaceFrameFeatures.LookingAway
                                | FaceFrameFeatures.MouthMoved
                                | FaceFrameFeatures.MouthOpen;


                            Joint headJoint = body.Joints[JointType.Head];
                            ColorSpacePoint colorSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(headJoint.Position);

                            System.Diagnostics.Debug.WriteLine("Gesichtspositionen");
                            System.Diagnostics.Debug.WriteLine("Gesichtsposition in x-Richtung: " + headJoint.Position.X);
                            System.Diagnostics.Debug.WriteLine("Gesichtsposition in y-Richtung: " + headJoint.Position.Y);
                            System.Diagnostics.Debug.WriteLine("Gesichtsposition in z-Richtung: " + headJoint.Position.Z);

                           
                                // get the depth image coordinates for the head
                                DepthSpacePoint depthPoint = this.coordinateMapper.MapCameraPointToDepthSpace(headJoint.Position);
                                System.Diagnostics.Debug.WriteLine("Gesichtsentfernung von der kinect");
                                System.Diagnostics.Debug.WriteLine("Gesichtsentfernung in x-Richtung: " + depthPoint.X);
                                System.Diagnostics.Debug.WriteLine("Gesichtsentfernung in y-Richtung: " + depthPoint.Y);
                                int depthX = (int)Math.Floor(depthPoint.X + 0.5);
                                int depthY = (int)Math.Floor(depthPoint.Y + 0.5);
                                int depthIndex = (depthY * this.kinectSensor.DepthFrameSource.FrameDescription.Width) + depthX;
                                //  int depthIndex = (depthY + this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra).Width) + depthX;

                                ushort depth = depthData[depthIndex];
                           

                            // measure the size of the pixel
                            float hFov = this.kinectSensor.DepthFrameSource.FrameDescription.HorizontalFieldOfView / 2;
                            float numPixels = this.kinectSensor.DepthFrameSource.FrameDescription.Width / 2;
                            double T = Math.Tan((Math.PI * 180) / hFov);
                            double pixelWidth = T * depth;

                            double bitmapsize = 1000 / (pixelWidth / numPixels);

                            int x = (int)(Math.Floor(colorSpacePoint.X + 0.5) - (bitmapsize / 2));
                            int y = (int)(Math.Floor(colorSpacePoint.Y + 0.5) - (bitmapsize / 2));


                            System.Diagnostics.Debug.WriteLine("X: "+x);
                            System.Diagnostics.Debug.WriteLine("Y: " + y);
                            System.Diagnostics.Debug.WriteLine("bitmapsize: " + bitmapsize);
                            System.Diagnostics.Debug.WriteLine("bitmapsize int: " + (int)bitmapsize);

                         
                           
                                this.faceBitmap = bodyBitmap.Crop(new Rect(x, y, bitmapsize, bitmapsize));
                                this.bodyBitmap.DrawRectangle(x, y, (int)bitmapsize + x, (int)bitmapsize + y, Colors.Red);
                           










                        }
                    }

                }
            }
            catch(Exception excep)
            {
                System.Diagnostics.Debug.WriteLine("Bei der Aufnahme sind Probleme aufgetaucht");
                System.Diagnostics.Debug.WriteLine(excep.Message);
            }
            finally
            {
                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                }

                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                }

                if (bodyIndexFrame != null)
                {
                    bodyIndexFrame.Dispose();
                }

                if (bodyFrame != null)
                {
                    bodyFrame.Dispose();
                }
            }
        }
        #endregion


        #region clickhandler to display faceimage
        private void faceImage_Click(object sender, RoutedEventArgs e)
        {

            try { 
            
            faceImage =ConvertWriteableBitmapToBitmapImage(faceBitmap);
            System.Diagnostics.Debug.WriteLine("Der Datentyp des faceImages ist:  "+faceImage.GetType());
            var binding = new Binding { Source=faceImage};
            FaceImageXAML.SetBinding(Image.SourceProperty, binding);


            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(faceBitmap));
            using (MemoryStream stream = new MemoryStream())
            {
                
                encoder.Save(stream);
                imagedata= stream.ToArray();
            }
            }
            
            catch(Exception ex) {
             MessageBox.Show(ex.StackTrace);
            }

        }
        #endregion


        #region convert writablebitmap to png bitmap image
        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage faceImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                faceImage.BeginInit();
                faceImage.CacheOption = BitmapCacheOption.OnLoad;
                faceImage.StreamSource = stream;
                faceImage.EndInit();
                faceImage.Freeze();
            }
            return faceImage;
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
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                    stream.Close();
                }
            }
        }
        #endregion


        #region  create mysql connection
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


        #region register user
        /// <summary>
        /// Clickhandler for registration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveUser_Click(object sender, RoutedEventArgs e)
        {
            // Check if all required textfields have valid values
            if (Firstnametextbox.Text.Length > 0 && Firstnametextbox.Text.Length <= 30 && Lastnametextbox.Text.Length > 0 && Lastnametextbox.Text.Length <= 30 && imagedata != null && Passworttextbox.Password.Length >0 && Passworttextbox.Password.Length <=30 && Passworttextbox.Password == Passwortwdhtextbox.Password && Emailtextbox.Text.Length >0 && Emailtextbox.Text.Length <= 30 && Regex.IsMatch(Emailtextbox.Text,@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {

                try
                {


                    // Initialize user instance
                    User user = new User();


                    // Allocate textfield value to instance members
                    user.Vorname = Firstnametextbox.Text;
                    user.Nachname = Lastnametextbox.Text;
                    user.Passwort = Passworttextbox.Password;
                    user.Email = Emailtextbox.Text;

                    // Check if email is unique in db
                    mySqlController.findEmailByEmail(user);
                    var foundemail = mySqlController.findEmailByEmail(user);
                    if (foundemail != null)
                    {

                        MessageBox.Show("Die Emailadresse wird bereits verwendet. Bitte verwenden Sie eine andere");
                        return;
                    }



                    if (checkboxreggae.IsChecked.Value)
                    {

                        user.MusicGenres.Add(Constants.reggae);
                  
                        }

                    if (checkboxklassik.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.klassik);

                    }

                    if (checkboxpop.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.pop);

                    }

                    if (checkboxpunk.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.punk);

                    }

                    if (checkboxhiphop.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.hiphop);

                    }

                    if (checkboxhouse.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.house);

                    }

                    if (checkboxrock.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.rock);

                    }

                    if (checkboxmetall.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.metall);

                    }

                    if (checkboxjazz.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.jazz);

                    }

                    if (checkboxelectro.IsChecked.Value)
                    {
                        user.MusicGenres.Add(Constants.electro);

                    }



                    // Save user in mysqldb
                    mySqlController.createUser(user);

                    // Get userId from mysqldb
                    var userId = mySqlController.findUserByEmail(user);


                    int userId2 = userId;
                    UserIdGlobal = userId.ToString();

                    user.UserId = userId;

                    // Create Kinect directory if it doesn't exist
                    try
                    {
                        if (!Directory.Exists("C:\\Kinect\\"))
                        {
                            Directory.CreateDirectory("C:\\Kinect\\");
                        }
                    }
                    catch (Exception except)
                    {
                        System.Diagnostics.Debug.WriteLine("CreateDirectoryException: " + except.Message);
                        MessageBox.Show("CreateDirectoryException: " + except.Message);
                    }

                    // Define filepath for saving faceImage local 
                    filename = "C:\\Kinect\\" + userId.ToString() + "_gesicht.jpg";
                    // Save faceImage local
                    CreateThumbnail(filename, faceImage);

                    // Save all checked music genres in mysqldb for a certain user
                    mySqlController.createGenreForUser(user);

                    // Display which user is saved in mysqldb 
                    string name = user.Vorname + " " + user.Nachname;
                    MessageBox.Show("Daten wurden erfolgreich für den User: " + name + " gespeichert." );


                    // Uploads the faceImage as model to ftp server
                    ftpup.modelupload(UserIdGlobal, filename);
                    // Calls the keylemon function to create a model with the previous uploaded faceImage
                    klemon.keyLemonModelCreation(UserIdGlobal, Firstnametextbox.Text, Lastnametextbox.Text, userId2);

                    // Wait 2 sec before close current window and open frmc_window
                    Thread.Sleep(2000);

                    // Open new frmc_window
                    var newwindow = new FRMC_Window();
                    newwindow.Show();

                    // Close current window
                    this.Close();


                    }
                  


                
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error " + ex.Number + " has occurred: ");

                 

                }

            }
            else
            {
                if (Firstnametextbox.Text.Length == 0)
                {
                 
                    MessageBox.Show("Bitte tragen Sie etwas in das Feld Vorname ein");
                }
                else
                    if (Firstnametextbox.Text.Length > 30)
                    {
                    
                        MessageBox.Show("In dem Feld Vorname dürfen nicht mehr als 30 Zeichen sein");
                    }
                    else
                        if (Lastnametextbox.Text.Length == 0)
                        {
                    
                            MessageBox.Show("Bitte tragen Sie etwas in das Feld Nachname ein");
                        }
                        else
                            if (Lastnametextbox.Text.Length > 30)
                            {
                      
                                MessageBox.Show("In dem Feld Nachname dürfen nicht mehr als 30 Zeichen sein");
                            }
                            else
                                if (Emailtextbox.Text.Length == 0)
                                 {

                                    MessageBox.Show("Bitte tragen Sie etwas in das Feld Email ein");
                                 }
                                 else
                                    if (Emailtextbox.Text.Length > 30)
                                    {
                            
                                     MessageBox.Show("In dem Feld Email dürfen nicht mehr als 30 Zeichen sein");
                                    }
                                        else
                                        if(!Regex.IsMatch(Emailtextbox.Text,@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
                                        {
                                            MessageBox.Show("Bitte geben Sie eine gültige Emailadresse an");
                                        }
                                    else
                                        if (Passworttextbox.Password.Length == 0)
                                        {
                              
                                            MessageBox.Show("Bitte tragen Sie etwas in das Feld Passwort ein");
                                        }
                                        else
                                            if (Passworttextbox.Password.Length > 30)
                                             {
                                  
                                                 MessageBox.Show("In dem Feld Passwort dürfen nicht mehr als 30 Zeichen sein");
                                             }
                                        else
                                            if (Passworttextbox.Password != Passwortwdhtextbox.Password)
                                            {
                                      
                                                MessageBox.Show("Das Passwörter müssen identisch sein");
                                            }
                                            else
                                                if (imagedata == null)
                                                {
                                       
                                                    MessageBox.Show("Es ist kein Bild vorhanden");
                                                }

                


            }
        }

        #endregion


        #region Window Closing
        /// <summary>
        /// Handle Window Closing Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegistrationDataWindow_Closing(object sender, CancelEventArgs e)
        {

        }
        #endregion
        
      

    }
}

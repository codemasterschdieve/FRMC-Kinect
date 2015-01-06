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
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace FRMC_Kinect
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window,INotifyPropertyChanged
    {

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
        private Byte[] imagedata =null;


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
        /// KeyLemon Instance
        /// </summary>

        KeyLemon klemon = new KeyLemon();

        /// <summary>
        /// KeyLemon Instance
        /// </summary>

        ftp ftp = new ftp();

        /// <summary>
        /// filename
        /// </summary>

        string filename;

        #endregion



        public Register()
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


        #region Window Closing
        /// <summary>
        /// Handle Window Closing Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            // Close kinectSensor
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }


            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.depthFrameReader != null)
            {
                // depthFrameReder is IDisposable
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }

            if (this.bodyIndexFrameReader != null)
            {
                // bodyIndexFrameReder is IDisposable
                this.bodyIndexFrameReader.Dispose();
                this.bodyIndexFrameReader = null;
            }


            if (this.faceFrameReader != null)
            {
                // faceFrameReder is IDisposable
                this.faceFrameReader.Dispose();
                this.faceFrameReader = null;
            }

            if (this.multiFrameSourceReader != null)
            {
                // MultiSourceFrameReder is IDisposable
                this.multiFrameSourceReader.Dispose();
                this.multiFrameSourceReader = null;
            }

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

                            double bitmapsize = 500 / (pixelWidth / numPixels);

                            int x = (int)(Math.Floor(colorSpacePoint.X + 0.5) - (bitmapsize / 2));
                            int y = (int)(Math.Floor(colorSpacePoint.Y + 0.5) - (bitmapsize / 2));


                            System.Diagnostics.Debug.WriteLine("X: "+x);
                            System.Diagnostics.Debug.WriteLine("Y: " + y);
                            System.Diagnostics.Debug.WriteLine("bitmapsize: " + bitmapsize);
                            System.Diagnostics.Debug.WriteLine("bitmapsize int: " + (int)bitmapsize);


                           this.faceBitmap = bodyBitmap.Crop(new Rect(x, y, bitmapsize, bitmapsize));
                           this.bodyBitmap.DrawRectangle(x,y,(int)bitmapsize+x,(int)bitmapsize+y,Colors.Red);











                        }
                    }

                }
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



        # region popup window definition
        /// <summary>
        /// Define popup window
        /// </summary>
        Window popup = new Window();

        public void createPopup(string title, int height, int width, string text)
        {

            // Window popup = new Window();
            popup.Owner = this;
            popup.Title = title;
            popup.Height = height;
            popup.Width = width;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());


            TextBlock meldungtext = new TextBlock();
            meldungtext.Text = text;
            meldungtext.FontSize = 20;
          

            Button ok = new Button();
            ok.Content = "ok";
            ok.Height = 50;
            ok.Width = 300;
            ok.Click += closeWindow;


            Grid.SetRow(meldungtext, 0);
            Grid.SetRow(ok, 2);
            grid.Children.Add(meldungtext);
            grid.Children.Add(ok);

            popup.Content = grid;
            popup.Show();

        }

        private void closeWindow(object sender, RoutedEventArgs e)
        {
            //popup.Close();
            popup.Visibility = Visibility.Hidden;
        }

       #endregion



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

        //





        //

        private void saveUser_Click(object sender, RoutedEventArgs e)
        {
            if (Firstnametextbox.Text.Length > 0 && Firstnametextbox.Text.Length <= 30 && Lastnametextbox.Text.Length > 0 && Lastnametextbox.Text.Length <= 30 && imagedata != null && Passworttextbox.Password.Length >0 && Passworttextbox.Password.Length <=30 && Passworttextbox.Password == Passwortwdhtextbox.Password && Emailtextbox.Text.Length >0 && Emailtextbox.Text.Length <= 30 && Regex.IsMatch(Emailtextbox.Text,@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {

                try
                {
                    // Connect to MySQL Database

                    //generate the connection string
                    string connStr = CreateConnStr("www.wi-stuttgart.de", "d01c6657", "d01c6657", "hdm123!");

                    //create a MySQL connection with a query string
                    MySqlConnection connection = new MySqlConnection(connStr);

                    //open the connection
                    connection.Open();

                    MySqlCommand cmd = connection.CreateCommand();



                    cmd.CommandText = "SELECT Email FROM User WHERE Email='" + Emailtextbox.Text + "'";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    var foundemail = cmd.ExecuteScalar();
                    if (foundemail != null)
                    {

                        MessageBox.Show("Die Emailadresse wird bereits verwendet. Bitte verwenden Sie eine andere");
                        return;
                    }




                    cmd.CommandText = "INSERT INTO User(Firstname,Lastname,Picture,Picturetype,Passwort,Email) VALUES('" + Firstnametextbox.Text + "','" + Lastnametextbox.Text + "','"+imagedata+"','png','" +Passworttextbox.Password+"','"+Emailtextbox.Text+"') ";
                    // MySqlCommand cmd = new MySqlCommand("INSERT INTO User(Firstname,Lastname) VALUES(" + Firstnametextbox.Text + "," + Lastnametextbox.Text + ") ");
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                  

                    cmd.CommandText = "SELECT UserId FROM User WHERE Email='" + Emailtextbox.Text + "'";

                    cmd.Prepare();
                    var userId = cmd.ExecuteScalar();

                    filename = "C:\\Users\\Stefan\\HdM\\WS_2014_15\\" + userId.ToString() + "_gesicht.png";
                    CreateThumbnail(filename, faceBitmap);

                    string name = klemon.useLocalpictureformodelcreation(filename, userId.ToString());
                    MessageBox.Show("Daten wurden erfolgreich gespeichert für: " + name);
                    

                    var reggae = 1;
                    var klassik = 2;
                    var pop = 3;
                    var punk = 4;
                    var hiphop = 5;
                    var house = 6;
                    var rock = 7;
                    var metall = 8;
                    var jazz = 9;
                    var electro = 10;

                    

                    cmd.CommandText ="SELECT UserId FROM User WHERE Email='"+Emailtextbox.Text+"'";
                    cmd.Prepare();
                    var userid= cmd.ExecuteScalar();


                    if (checkboxreggae.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + reggae + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxklassik.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + klassik + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxpop.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + pop + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxpunk.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + punk + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxhiphop.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + hiphop + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxhouse.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + house + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxrock.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + rock + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxmetall.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + metall + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxjazz.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + jazz + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (checkboxelectro.IsChecked.Value)
                    {
                        cmd.CommandText = "INSERT INTO mn_AllocationTable_User_MusicGenre(UserId,MusicGenreId) VALUES('" + userid + "','" + electro + "') ";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }


                 //close the connection
                 connection.Close();


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
                  //  createPopup("Meldung", 200, 500, "Bitte tragen Sie etwas in das Feld Vorname ein");
                    MessageBox.Show("Bitte tragen Sie etwas in das Feld Vorname ein");
                }
                else
                    if (Firstnametextbox.Text.Length > 30)
                    {
                    //    createPopup("Meldung", 200, 500, "In dem Feld Vorname dürfen nicht mehr als 30 Zeichen sein");
                        MessageBox.Show("In dem Feld Vorname dürfen nicht mehr als 30 Zeichen sein");
                    }
                    else
                        if (Lastnametextbox.Text.Length == 0)
                        {
                     //       createPopup("Meldung", 200, 500, "Bitte tragen Sie etwas in das Feld Nachname ein");
                            MessageBox.Show("Bitte tragen Sie etwas in das Feld Nachname ein");
                        }
                        else
                            if (Lastnametextbox.Text.Length > 30)
                            {
                       //         createPopup("Meldung", 200, 500, "In dem Feld Nachname dürfen nicht mehr als 30 Zeichen sein");
                                MessageBox.Show("In dem Feld Nachname dürfen nicht mehr als 30 Zeichen sein");
                            }
                            else
                                if (Emailtextbox.Text.Length == 0)
                                 {
                           //         createPopup("Meldung", 200, 500, "Bitte tragen Sie etwas in das Feld Email ein");
                                    MessageBox.Show("Bitte tragen Sie etwas in das Feld Email ein");
                                 }
                                 else
                                    if (Emailtextbox.Text.Length > 30)
                                    {
                             //        createPopup("Meldung", 200, 500, "In dem Feld Email dürfen nicht mehr als 30 Zeichen sein");
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
                               //             createPopup("Meldung", 200, 500, "Bitte tragen Sie etwas in das Feld Passwort ein");
                                            MessageBox.Show("Bitte tragen Sie etwas in das Feld Passwort ein");
                                        }
                                        else
                                            if (Passworttextbox.Password.Length > 30)
                                             {
                                   //              createPopup("Meldung", 200, 500, "In dem Feld Passwort dürfen nicht mehr als 30 Zeichen sein");
                                                 MessageBox.Show("In dem Feld Passwort dürfen nicht mehr als 30 Zeichen sein");
                                             }
                                        else
                                            if (Passworttextbox.Password != Passwortwdhtextbox.Password)
                                            {
                                      //          createPopup("Meldung", 200, 500, "Das Passwörter müssen identisch sein");
                                                MessageBox.Show("Das Passwörter müssen identisch sein");
                                            }
                                            else
                                                if (imagedata == null)
                                                {
                                         //           createPopup("Meldung", 200, 500, "Es ist kein Bild vorhanden");
                                                    MessageBox.Show("Es ist kein Bild vorhanden");
                                                }

            }
        }
        #endregion

        public void Testkeylemon_Click(Object sender, RoutedEventArgs args)
        {

            //try
            //{
            ftp.bildupload();
                //  klemon.testKeylemonconnection();
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show(ex.StackTrace);
            //    Console.WriteLine(ex.StackTrace);
            //}
        }


    }
}

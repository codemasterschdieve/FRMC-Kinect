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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;


///@author Tobias Moser, Jan Plank, Stefan Sonntag

namespace FRMC_Kinect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion


        #region hyperlink navigation
        /// <summary>
        /// Use to navigate with hyperlinks
        /// not in use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        #endregion


        #region navigate handlers
        /// <summary>
        /// Navigate to registration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void navigateBtn_Click(object sender, RoutedEventArgs e)
        {
            var newwindow = new Registration_Window(); //create your new form.
            newwindow.Show(); //show the new form.
            this.Close(); //only if you want to close the current form.
        }


        /// <summary>
        /// Navigate to face recognition and media control
        /// </summary>
        /// <param name="sender"></param>
        private void navigateBtnMC_Click(object sender, RoutedEventArgs e)
        {
            var newwindow = new FRMC_Window();
            newwindow.Show();
            this.Close();
        }
        #endregion

    }
}

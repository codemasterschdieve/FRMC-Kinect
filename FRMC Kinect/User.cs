using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRMC_Kinect
{
    public class User
    {
        private int userId;
        private string vorname;
        private string nachname;
        private string email;
        private string modelId;
        private string passwort;
        private List<int> musicGenres = new List<int>();
        private List<string> musicGenreNames = new List<string>();
      


        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }
   

        public string Vorname
        {
            get { return vorname; }
            set { vorname = value; }
        }
       

        public string Nachname
        {
            get { return nachname; }
            set { nachname = value; }
        }
        

        public string Email
        {
            get { return email; }
            set { email = value; }
        }
       

        public string ModelId
        {
            get { return modelId; }
            set { modelId = value; }
        }
       

        public string Passwort
        {
            get { return passwort; }
            set { passwort = value; }
        }
       

        public List<int> MusicGenres
        {
            get { return musicGenres; }
            set { musicGenres = value; }
        }

        public List<string> MusicGenreNames
        {
            get { return musicGenreNames; }
            set { musicGenreNames = value; }
        }

       

        
    }
}

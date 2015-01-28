using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyLemon;
using System.IO;
using System.Windows;

using MySql.Data.MySqlClient;



namespace FRMC_Kinect
{
    class KeyLemon
    {
        MySqlController mySqlController = new MySqlController();
        KLAPI api = new KLAPI("Tobi0604", "qUDuBnzvYCJsxcDD4nnyHWCtiHbwU7rmGxHdh8RXbcOjN24m2TJcDI", "https://api.keylemon.com");
        User user = new User();

        private List<string> allemodelids = new List<string>();

        
        private List<int> score = new List<int>();

        List<string> erkannteModels = new List<string>();



        public List<string> Allemodelids
        {
            get { return allemodelids; }
            set { allemodelids = value; }
        }


        public List<string> ErkannteModels
        

        {
            get { return erkannteModels; }
            set { erkannteModels = value; }
        }


        public List<int> Score
        {
            get { return score; }
            set { score = value; }
        }


        dynamic response;
        dynamic response2;
        dynamic responesModelId;

        public void keyLemonModelCreation(string userId, string vorname, string nachname, int userId2)
        {
            // To read back the model at a later time:
            user.UserId = userId2;

            // We can train using URLs of images
            String[] penelope_urls = new String[1]{
            "http://www.frmc.wi-stuttgart.de/scan/"  + userId + "model.jpg" };

            // Create a model using the URLs, the data and the face_list
            //response = api.CreateFaceModel(null, byteImage[1][0] , null, User_Id);
            try
            {
                response = api.CreateFaceModel(penelope_urls, null, null, userId + "_" + vorname + nachname);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            
            try
            {
                if (response == null)
                {
                    mySqlController.DeleteUserByUserId(user);
                    MessageBox.Show("Gesichtserkennungsoftware Keylemon funktioniert nicht. User nicht registriert");
                }
                else
                {
                    responesModelId = api.GetModel(response["model_id"]);

                    user.ModelId = responesModelId["model_id"];
                    mySqlController.updateUser(user);
                }                             
                
            }
            catch (Exception exc)
            {
                mySqlController.DeleteUserByUserId(user);
                MessageBox.Show("Gesichtserkennungsfehler. User nicht registriert: ");
                Console.WriteLine(exc.StackTrace);
            }
            
        }


        public void RecognizeUserFace()
        {
            try
            {
                //Alte Models Löschen
                ErkannteModels.Clear();
                allemodelids.Clear();
                score.Clear();




                var model_ids = mySqlController.findAllModelIdFromDb(user);

                foreach (var modelid in model_ids)
                {
                    // Then to do a recognition test against this model:
                    // here, we're doing 1 image against 1 model but it's
                    // possible to do n images against n models.
               
                    string[] my_image_to_test = new string[] { "http://www.frmc.wi-stuttgart.de/scan/scan.jpg" };


                    response2 = api.RecognizeFace(modelid, my_image_to_test, null, null);

                    dynamic result_for_face_1 = response2["faces"][0]["results"][0];

                    Console.WriteLine("Test of face {0} against model {1} has a score of {2}",
                                       response2["faces"][0]["face_id"],
                                       result_for_face_1["model_id"],
                                        result_for_face_1["score"]);

                   allemodelids.Add(result_for_face_1["model_id"]);



                   score.Add(result_for_face_1["score"]);
                    
                  
                    

                    if ((result_for_face_1["score"]) >= 25)
                    {
                        
                        erkannteModels.Add(result_for_face_1["model_id"]);
                    }

                  
                }
               

            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler bei Gesichterkennung mit Keylemon: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }


            

        }

        //public async Task RunRecognizeUserFaceAsAsync()
        //{
        //    await Task.Run(() => RecognizeUserFace());
            
        //}

        private void ReadAllBytes(string p)
        {
            throw new NotImplementedException();
        }




    }
}

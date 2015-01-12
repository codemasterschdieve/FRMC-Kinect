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
        List<string> erkannteModels = new List<string>();

        public List<int> ErkannteModels
        {
            get { return erkannteModels; }
            set { erkannteModels = value; }
        }

        dynamic response;
        dynamic response2;
        dynamic responesModelId;

        public void keyLemonModelCreation(string userId, string vorname, string nachname, int userId2)
        {

            


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
                MessageBox.Show(ex.StackTrace);
            }


            // To read back the model at a later time:
            user.UserId = userId2;

          responesModelId = api.GetModel(response["model_id"]);
            
          user.ModelId = responesModelId["model_id"];
           mySqlController.updateUser(user);
            

            //      MessageBox.Show(response["model_id"]);



            //     And delete the model:
            //   response = api.DeleteModel(response ["model_id"]);







        //    dynamic result_for_face_1 = response["faces"][0]["results"][0];

       //     MessageBox.Show(result_for_face_1["name"]);

         


        }


       // public string useLocalpictureformodelcreation(string filename, string User_Id)
       // {


       //     // Or data of images from a local storage
       //     byte[] imageData = File.ReadAllBytes(filename);
 


       //     //String[] face_list = new String[response["faces"].Length];
       //     //// Maybe detect found more than on face in the image, group them
       //     //for (int i =0; i < response ["faces"].Length; i++){
       //     //    face_list[i] = response ["faces"] [i] ["face_id"];
       //     //}
       //     byte[][] byteImage = new byte[1][] { imageData };
 
       //     // Create a model using the URLs, the data and the face_list
       //     //response = api.CreateFaceModel(null, byteImage[1][0] , null, User_Id);
       //     try
       //     {
       //     dynamic response = api.CreateFaceModel(null, byteImage, null, User_Id); 
       //     }
       //     catch (Exception ex){
       //         MessageBox.Show(ex.StackTrace);
       //     }
            
 
       //     // To read back the model at a later time:
       //     dynamic response2 = api.GetModel(response["model_id"]);
 
       //     // And delete the model:
       ////     response = api.DeleteModel(response ["model_id"]);

       //     dynamic result_for_face_1 = response2["faces"][0]["results"][0];

       //     string name = result_for_face_1["name"];


       //     return name;

       // }



        public void RecognizeUserFace()
        {



            //// First, let's train a model
            //// See the Model creation section for more details
       //   String[] penelope_urls = new String[2]{
      //    "http://www.frmc.wi-stuttgart.de/scan/40model.jpg",
      //   "http://www.frmc.wi-stuttgart.de/scan/39model.jpg" };

        //   dynamic response2 = api.CreateFaceModel(penelope_urls, null, null, "test");


         

           var model_ids = mySqlController.findAllModelIdFromDb(user);
            

            foreach(var modelid in model_ids)
            {


             
                // Then to do a recognition test against this model:
                // here, we're doing 1 image against 1 model but it's
                // possible to do n images against n models.

                string[] my_image_to_test = new string[] { "http://www.frmc.wi-stuttgart.de/scan/uploadscan.jpg" };


                response2 = api.RecognizeFace(modelid, my_image_to_test, null, null);

                dynamic result_for_face_1 = response2["faces"][0]["results"][0];

                Console.WriteLine("Test of face {0} against model {1} has a score of {2}",
                                   response2["faces"][0]["face_id"],
                                   result_for_face_1["model_id"],
                                    result_for_face_1["score"]);


                if ((result_for_face_1["score"]) >= 5){
                erkannteModels.Add(result_for_face_1["model_id"]);

                }


            }

           

          

          

        }

        private void ReadAllBytes(string p)
        {
            throw new NotImplementedException();
        }
       
 


    }
}

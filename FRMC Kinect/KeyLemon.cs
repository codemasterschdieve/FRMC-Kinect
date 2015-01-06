using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyLemon;
using System.IO;
using System.Windows;

namespace FRMC_Kinect
{
    class KeyLemon
    {

        KLAPI api = new KLAPI("Tobi0604", "qUDuBnzvYCJsxcDD4nnyHWCtiHbwU7rmGxHdh8RXbcOjN24m2TJcDI", "https://api.keylemon.com");
        dynamic response;



        public void testKeylemonconnection()
        {

            // Images can be give as public URLs. When there's more than
            // one URL: concatenate as a comma separated string into a
            // single parameter.
            String[] image_url = new String[1] { "http://www.keylemon.com/images/saas/group.jpg" };

            dynamic response = api.DetectFaces(image_url, null);

            Console.WriteLine("Face id  : {0}", response["faces"][0]["face_id"]);
            Console.WriteLine("Position : {0} {1}", response["faces"][0]["x"], response["faces"][0]["y"]);
            Console.WriteLine("Size     : {0} {1}", response["faces"][0]["h"], response["faces"][0]["w"]);
        }


        public string useLocalpictureformodelcreation(string filename, string User_Id)
        {


            // Or data of images from a local storage
            byte[] imageData = File.ReadAllBytes(filename);
 


            //String[] face_list = new String[response["faces"].Length];
            //// Maybe detect found more than on face in the image, group them
            //for (int i =0; i < response ["faces"].Length; i++){
            //    face_list[i] = response ["faces"] [i] ["face_id"];
            //}
            byte[][] byteImage = new byte[1][] { imageData };
 
            // Create a model using the URLs, the data and the face_list
            //response = api.CreateFaceModel(null, byteImage[1][0] , null, User_Id);
            try
            {
                response = api.CreateFaceModel(null, byteImage, null, User_Id); 
            }
            catch (Exception ex){
                MessageBox.Show(ex.StackTrace);
            }
            
 
            // To read back the model at a later time:
            response = api.GetModel(response["model_id"]);
 
            // And delete the model:
       //     response = api.DeleteModel(response ["model_id"]);

            dynamic result_for_face_1 = response["faces"][0]["results"][0];

            string name = result_for_face_1["name"];


            return name;

        }



        public string RecognizeUserFace(string LiveScanPicturePath, int User_Id)
        {



            //// First, let's train a model
            //// See the Model creation section for more details
            //String[] penelope_urls = new String[2]{
            //"http://www.keylemon.com/images/saas/penelope/Penelope_Cruz_1.jpg",
            //"http://www.keylemon.com/images/saas/penelope/Penelope_Cruz_2.jpg" };

            //dynamic response = api.CreateFaceModel(penelope_urls, null, null, null);

            string model_id = "User_Id";

            // Then to do a recognition test against this model:
            // here, we're doing 1 image against 1 model but it's
            // possible to do n images against n models.

            string[] my_image_to_test = new string[] {LiveScanPicturePath};


           response = api.RecognizeFace(model_id, my_image_to_test, null, null);

            dynamic result_for_face_1 = response["faces"][0]["results"][0];

            Console.WriteLine("Test of face {0} against model {1} has a score of {2}",
                                 response["faces"][0]["face_id"],
                                 result_for_face_1["model_id"],
                                 result_for_face_1["score"]);

            return null;

        }

        private void ReadAllBytes(string p)
        {
            throw new NotImplementedException();
        }
       
 


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlantLifeAnimationForm
{
    public partial class PlantLifeForm : Form
    {
        public int mainpicturecounter = 0;
        public PlantLifeImagesService plantlifeImages;
        FaceCounter faceCounter;
        bool ShowPeoplePicture = true; 

        private bool captureInProgress;
        private FaceCapture faceCapture;

        VideoMethod CurrentState = VideoMethod.Viewing; //default state
        public enum VideoMethod
        {
            Viewing,
            Recording
        };

        #region faceparams
        string faceTrainingFile, eyeTrainingFile;
        double FaceScale = 1.1;
        int FaceNieghbors = 5;
        int FaceMinSize = 20;
        #endregion

        public PlantLifeForm()
        {
            Console.WriteLine("PlantLifeForm main window starting con!!!");
            InitializeComponent();
            var uri = new System.Uri("ms-appx:///images/logo.png");
            //var file = Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            //ImageMain.Source= ImageSource
            plantlifeImages = new PlantLifeImagesService();
            plantlifeImages.initializeJSONFile();

            ConfigLoad();

            faceCounter = new FaceCounter();

            faceCapture = new FaceCapture(faceTrainingFile, eyeTrainingFile);
            faceCapture.Scale = FaceScale;
            faceCapture.Neighbors = FaceNieghbors;
            faceCapture.FaceMinSize = FaceMinSize;
            faceCapture.FaceCaptured += new FaceCapturedEventHandler(FaceCaptured);
            faceCapture.ImageCaptured += faceCapture_ImageCaptured;

            Console.WriteLine("PlantLifeForm completed!!!");

        }

        void faceCapture_ImageCaptured(object sender)
        {
            peoplePicture.Image = faceCapture.ImageFrameLast.ToBitmap(); 
        }

        /// <summary>
        /// event update() method for processing face captured. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="face"></param>
        private void FaceCaptured(object sender, Face face)
        {
            // deal with face being captured here. (update)
            SetFace(face);
        }

        private void SetFace(Face face){

            // do some logic to set face on screen. 
            

        }
        private void PlantLifePicture_Click(object sender, EventArgs e)
        {

        }

        private void ConfigLoad()
        {
            Console.WriteLine(" plant life ConfigLoad");
            try
            {
                faceTrainingFile = ConfigurationManager.AppSettings["faceTrainingFile"].ToString();
                eyeTrainingFile = ConfigurationManager.AppSettings["eyeTrainingFile"].ToString(); 
                FaceScale = double.Parse(ConfigurationManager.AppSettings["FaceScale"].ToString());
                FaceNieghbors = int.Parse(ConfigurationManager.AppSettings["FaceNieghbors"].ToString());
                FaceMinSize = int.Parse(ConfigurationManager.AppSettings["FaceMinSize"].ToString());

            }
            catch (Exception parseERR)
            {
                Console.WriteLine("ERROR -PlantLifeForm ConfigLoad" + parseERR); 
            }
        }

        private void stopStartProgress()
        {
            if (captureInProgress)
            {
                faceCapture.StopCapture();
                Face bface = faceCapture.GetBestCapturedFace();
                SetFace(bface);
            }
            else
            {
                faceCapture.Scale = FaceScale; 
                faceCapture.Neighbors = FaceNieghbors;
                faceCapture.FaceMinSize = FaceMinSize;
                faceCapture.StartCapture();
            }

            captureInProgress = !captureInProgress;
        }
        private void PlantLifeForm_Load(object sender, EventArgs e)
        {
            faceCapture.StartCapture(); 
            Console.WriteLine("PlantLifeForm_Load ... ");
        }

        private void PlantLifeForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if ( < PlantLifePicture.ZOrder)
            if (ShowPeoplePicture)
            {
                peoplePicture.SendToBack();
                ShowPeoplePicture = false; 
            }
            else
            {
                peoplePicture.BringToFront();
                ShowPeoplePicture = true; 
            }

        }





    }
}

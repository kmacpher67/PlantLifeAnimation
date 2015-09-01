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

        private bool captureInProgress;
        private FaceCapture faceCapture;

        VideoMethod CurrentState = VideoMethod.Viewing; //default state
        public enum VideoMethod
        {
            Viewing,
            Recording
        };

        #region faceparams
        double FaceScale = 1.1;
        int FaceNieghbors = 5;
        int FaceMinSize = 20;
        #region

        public PlantLifeForm()
        {
            Console.WriteLine("main window starting con!!!");
            InitializeComponent();
            var uri = new System.Uri("ms-appx:///images/logo.png");
            //var file = Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            //ImageMain.Source= ImageSource
            plantlifeImages = new PlantLifeImagesService();
            plantlifeImages.initializeJSONFile();

            ConfigLoad();

            faceCounter = new FaceCounter();

            faceCapture = new FaceCapture("haarcascade_frontalface_default.xml", "haarcascade_eye.xml");
            faceCapture.FaceCaptured += new FaceCapturedEventHandler(FaceCaptured);


            Console.WriteLine("PlantLifeForm completed!!!");

        }

        /// <summary>
        /// event update() method for processing face captured. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="face"></param>
        private void FaceCaptured(object sender, Face face)
        {
            // deal with face being captured here. (update)
        }

        private void SetFace(Face face){

            // do some logic to set face on screen. 

        }
        private void PlantLifePicture_Click(object sender, EventArgs e)
        {

        }

        private void ConfigLoad()
        {
            try
            {
                FaceScale = double.Parse(ConfigurationManager.AppSettings["FaceScale"].ToString());
                FaceNieghbors = int.Parse(ConfigurationManager.AppSettings["FaceNieghbors"].ToString());
                FaceMinSize = int.Parse(ConfigurationManager.AppSettings["FaceMinSize"].ToString());

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
                faceCapture.Neighbors = int.Parse(cboNeighbors.Text);
                faceCapture.MinSize = int.Parse(cboNeighbors.Text);
                faceCapture.StartCapture();
            }

            captureInProgress = !captureInProgress;
        }
        private void PlantLifeForm_Load(object sender, EventArgs e)
        {
            Console.WriteLine("PlantLifeForm_Load ... ");
        }


    }
}

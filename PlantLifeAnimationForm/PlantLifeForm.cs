using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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
        bool ShowPeoplePicture = true;
        Bgr drawBoxColor = new Bgr(0, double.MaxValue, 50);

        private bool captureInProgress;
        private FaceCapture faceCapture;
        private bool HasCuda = true; 

        VideoMethod CurrentState = VideoMethod.Viewing; //default state
        public enum VideoMethod
        {
            Viewing,
            Recording
        };

        #region faceparams
        string faceTrainingFile = "haar\\haarcascade_frontalface_default.xml";
        string eyeTrainingFile = "haar\\haarcascade_eye.xml";
        string faceTrainingFileCuda = "haar_cuda\\haarcascade_frontalface_default.xml";
        string eyeTrainingFileCuda = "haar_cuda\\haarcascade_eye.xml";
        double FaceScale = 1.15;
        int FaceNieghbors = 3;
        int FaceMinSize = 10;
        int FaceMaxSize = 200;
        double EyeScale = 1.15;
        int EyeNieghbors = 1;
        int EyeMinSize = 4;
        int EyeMaxSize = 48;
        #endregion

        public int lastFaceIndex = 0;
        public int lastFaceCount = 0;

        public int reloadDirIndex = 0;
        public string[] reloaddir = {"images/zone1","images/zone2","images/main", "images/Matrix", "images/complex", "images/Pond", "images/Fireworks"};

        public int reloadDirIndexOver = 0;
        public string[] reloaddirOver = { "images/butterfly", "images/spiders" , "images/butterfly4", "images/butterfly2", "images/butterfly3", "images/butterfly7" };

        public PlantLifeForm()
        {
            Console.WriteLine("PlantLifeForm main window starting constructor!!!");
            InitializeComponent();
            this.PlantLifePicture.Size=this.DesktopBounds.Size;
            var uri = new System.Uri("ms-appx:///images/logo.png");
            //var file = Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            //ImageMain.Source= ImageSource
            plantlifeImages = new PlantLifeImagesService();
            plantlifeImages.frameSize = this.PlantLifePicture.Size;
            plantlifeImages.initializeJSONFile();

            ConfigLoad();

            FaceCapture.HasCuda = HasCuda;

            var oclptr = OclInvoke.GetPlatformInfo();
            if (oclptr.Size < 1)
            {
                Console.WriteLine("NO OCL GPUs FOUND!!!");
            }
            else
            {
                var myb = oclptr[0];
                var myb1 = oclptr[oclptr.Size - 1];
                //OclPlatformInfo opi = (OclPlatformInfo)listinfo;
                Console.WriteLine("GPU CUDA OCL checker size=" + oclptr.Size + " GPU0= " + myb + " GPU1=" + myb1);
                // size=2 GPU0= Intel(R) OpenCL GPU1=NVIDIA CUDA
            }


            if (HasCuda)
            {
                faceTrainingFile = faceTrainingFileCuda;
                eyeTrainingFile = eyeTrainingFileCuda;
                Console.WriteLine("HasCuda overwriten haar training files =" + HasCuda);
            }

            this.faceCapture = new FaceCapture(faceTrainingFile, eyeTrainingFile,FaceScale, FaceNieghbors, FaceMinSize);

            this.faceCapture.Scale = FaceScale;
            this.faceCapture.Neighbors = FaceNieghbors;
            this.faceCapture.FaceMinSize = FaceMinSize;
            this.faceCapture.FaceMaxSize = FaceMaxSize;
            this.faceCapture.FaceCaptured += new FaceCapturedEventHandler(FaceCaptured);
            this.faceCapture.ImageCaptured += faceCapture_ImageCaptured;


            // start off with default plant image: 
               updatePlantImage();

            Console.WriteLine("PlantLifeForm constructor completed!!!");
        }

        void updatePlantImage()
        {
            PlantLifePicture.Image = plantlifeImages.handleFacedScoredInput(faceCapture.Faces);
        }

        void faceCapture_ImageCaptured(object sender)
        {
            // only update plant image if the face is any good. 
            updatePlantImage();

            if (faceCapture.ImageFrameLast != null && faceCapture.ImageFrameLast.Width != 0 && faceCapture.Faces != null && lastFaceCount!= faceCapture.Faces.Count)
            {                
            using (Image<Bgr, byte> currentFrame = faceCapture.ImageFrameLast.Resize(faceCapture.reductionRatio,Inter.Cubic) )
            {
                double rs = (1.0 * peoplePicture.Size.Width / currentFrame.Size.Width);

                if (faceCapture.Faces != null && faceCapture.Faces.Count > lastFaceIndex)
                {
                    lastFaceIndex = faceCapture.Faces.Count - 1;
                    Face currentFace = faceCapture.Faces[lastFaceIndex];

                    /// depreciated v2 - MCvFont f = new MCvFont(CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);
                    Rectangle rectFace = currentFace.FaceRect;
                    currentFrame.Draw(rectFace, drawBoxColor, 3);

                        String displayFaceData = " W=" + currentFace.Width + " Mo=" + currentFace.MotionPixels;
                        //"F#=" + faceCapture.Faces.Count + " Rx,y=" + rectFace.X + "," + rectFace.Y ;
                        //displayFaceData = displayFaceData + " P=" + currentFace.FramePosX + " W=" + currentFace.Width + " Mo=" + currentFace.MotionPixels;
                        //Version 3.0 does it differently than version 2.0
                        //Draw "Hello, world." on the image using the specific font
                        CvInvoke.PutText(
                       currentFrame,
                       displayFaceData,
                       new System.Drawing.Point(1, 60),
                       FontFace.HersheyComplex,
                       rs * 4,
                       new Bgr(0, 0, 255).MCvScalar,6);
                    peoplePicture.Image = currentFrame.Resize(rs, Inter.Cubic).ToBitmap();

                }
                else if (lastFaceIndex > 1)
                {
                    lastFaceIndex = faceCapture.Faces.Count - 2;
                }
            }
            }

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
                faceTrainingFileCuda = ConfigurationManager.AppSettings["faceTrainingFileCuda"].ToString();
                eyeTrainingFileCuda = ConfigurationManager.AppSettings["eyeTrainingFileCuda"].ToString();

                FaceScale = double.Parse(ConfigurationManager.AppSettings["FaceScale"].ToString());
                FaceNieghbors = int.Parse(ConfigurationManager.AppSettings["FaceNieghbors"].ToString());
                FaceMinSize = int.Parse(ConfigurationManager.AppSettings["FaceMinSize"].ToString());
                HasCuda = Convert.ToBoolean(ConfigurationManager.AppSettings["HasCuda"] ?? "false");
                FaceMaxSize = int.Parse(ConfigurationManager.AppSettings["FaceMaxSize"].ToString());

                EyeScale = double.Parse(ConfigurationManager.AppSettings["EyeScale"].ToString());
                EyeNieghbors = int.Parse(ConfigurationManager.AppSettings["EyeNieghbors"].ToString());
                EyeMinSize = int.Parse(ConfigurationManager.AppSettings["EyeMinSize"].ToString());
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

        private void PlantLifeForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9)
            {
                Console.WriteLine("F9 key pressed change baseimage bg index = " + reloadDirIndex);
                plantlifeImages.reloadImages(reloaddir[this.reloadDirIndex++]);

                if (reloadDirIndex >= reloaddir.Length)
                    reloadDirIndex = 0;
            }


            if (e.KeyCode == Keys.F8)
            {
                Console.WriteLine("PRESSED F8 key change load overlay index ="+ reloadDirIndexOver);
                plantlifeImages.plantLifeImagesOver.Clear();
                plantlifeImages.currentOverlayFrame = 0;
                plantlifeImages.loadOverlayImages(reloaddirOver[this.reloadDirIndexOver++]);
                if (reloadDirIndexOver >= reloaddirOver.Length)
                    reloadDirIndexOver = 0;
            }

                if (e.KeyCode == Keys.F11)
            {
                Console.WriteLine("F11 KEY PRESSED");
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.MaximizeBox = true;
                    this.PlantLifePicture.Size = this.Size;
                    plantlifeImages.frameSize = PlantLifePicture.Size;
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.MaximizeBox = true;
                    this.PlantLifePicture.Size = this.Size;
                    plantlifeImages.frameSize = PlantLifePicture.Size;

                }
            }
        }

        private void PlantLifeForm_MaximizedBoundsChanged(object sender, EventArgs e)
        {
            Console.WriteLine("PlantLifeForm_MaximizedBoundsChanged e=" + e.ToString());
            this.PlantLifePicture.Size = this.DesktopBounds.Size;
            this.PlantLifePicture.Dock = DockStyle.Fill;
            plantlifeImages.frameSize = PlantLifePicture.Size;
        }

        private void PlantLifeForm_ResizeEnd(object sender, EventArgs e)
        {
            Console.WriteLine("PlantLifeForm_ResizeEnd e=" + e.ToString());
            this.PlantLifePicture.Size = this.Size;
            this.PlantLifePicture.Dock = DockStyle.Fill;
            plantlifeImages.frameSize = PlantLifePicture.Size;
        }


    }
}

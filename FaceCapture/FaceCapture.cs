using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.Util;
using Emgu.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace PlantLifeAnimationForm
{
    public delegate void FaceCapturedEventHandler(object sender, Face face);
    public delegate void ImageCapturedEventHandler(object sender);

    public class FaceCapture
    {
        private Capture capture;
        private MotionHistory _motionHistory;
        private BackgroundSubtractor foregroundDetector;
        public string FaceTrainingFile { get; set; }
        public string EyeTrainingFile { get; set; }
        public double Scale { get; set; }
        public int Neighbors { get; set; }
        public int FaceMinSize { get; set; }
        public int FaceMaxSize { get; set; }
        public List<Face> Faces { get; set; }

        public double motionHistoryDuration = 1.0;
        public double maxDelta = 0.05;
        public double minDelta = 0.5;

        private IFindFaces FaceDetector;

        public Image<Bgr, Byte> ImageFrameLast;
        public Image<Bgr, Byte> ImageMotionLast;
        public Image<Bgr, byte> ImageForeGroundMaskLast; 

        public event FaceCapturedEventHandler FaceCaptured;
        public event ImageCapturedEventHandler ImageCaptured;

        public FaceCapture(string faceTrainingFile, string eyeTrainingFile)
            : this(faceTrainingFile, eyeTrainingFile, 1.1, 10, 100)
        {

        }

        public FaceCapture(string faceTrainingFile, string eyeTrainingFile, double scale, int neighbors, int minSize)
        {
            FaceTrainingFile = faceTrainingFile;
            EyeTrainingFile = eyeTrainingFile;
            Scale = scale;
            Neighbors = neighbors;
            FaceMinSize = minSize;
            FaceMaxSize = 200;

            if (CudaInvoke.HasCuda)
            {
                FaceDetector = new FaceDetectCuda();
            }
            else
            {
                FaceDetector = new FaceDetect();
            }

            _motionHistory = new MotionHistory(
                motionHistoryDuration, //in second, the duration of motion history you wants to keep
                maxDelta, //in second, maxDelta for cvCalcMotionGradient
                minDelta); //in second, minDelta for cvCalcMotionGradient
            //capture = new Capture();
        }

        public void StartCapture()
        {
            Console.WriteLine("StartCapture");
            Faces = new List<Face>();
            capture = new Capture();
            _motionHistory = new MotionHistory(1.0, 0.05, 0.5);
            Application.Idle += ProcessFrame;
        }

        public void StopCapture()
        {
            Console.WriteLine("StopCapture");
            Application.Idle -= ProcessFrame;
            capture.Dispose();
            capture = null;

            if (this.Faces.Count > 4)
            {
                this.Faces.RemoveAt(0);
                this.Faces.RemoveAt(0);
            }
        }

        public List<Face> GetFaces(int numFaces)
        {
            return GetFaces(numFaces, 75);
        }

        public List<Face> GetFaces(int numFaces, int minScore)
        {
            int frameCount = 0;
            capture = new Capture();
            _motionHistory = new MotionHistory(1.0, 0.05, 0.5);
            List<Face> foundfaces = new List<Face>();

            while (foundfaces.Count() < numFaces)
            {
                Mat mat = capture.QueryFrame();
                Image<Bgr, Byte> ImageFrame = mat.ToImage<Bgr, Byte>();

                frameCount = frameCount + 1;
                MotionInfo motion = this.GetMotionInfo(mat);
                List<Face> detectedFaces = FaceDetector.FindFaces(ImageFrame, this.FaceTrainingFile, this.EyeTrainingFile, this.Scale, this.Neighbors, this.FaceMinSize);

                if (frameCount > 2)
                {
                    foreach (Face face in detectedFaces)
                    {
                        face.MotionObjects = motion.MotionObjects;
                        face.MotionPixels = motion.MotionPixels;

                        if (face.FaceScore > minScore)
                        {
                            foundfaces.Add(face);
                        }
                    }
                }
            }

            capture.Dispose();
            capture = null;
            return foundfaces;
        }

        //public async Task<List<Face>> GetFacesAsync(int numFaces)
        //{
        //    return await Task.Run(() => this.GetFaces(numFaces));
        //}

        //public async Task<List<Face>> GetFacesAsync(int numFaces, int minScore)
        //{
        //    return await Task.Run(() => this.GetFaces(numFaces, minScore));
        //}

        public Face GetFace()
        {
            return this.GetFace(75);
        }

        public Face GetFace(int minScore)
        {
            List<Face> foundfaces = new List<Face>();
            foundfaces = this.GetFaces(1, minScore);
            return foundfaces.FirstOrDefault();
        }

        //public async Task<Face> GetFaceAsync()
        //{
        //    return await Task.Run(() => this.GetFace());
        //}

        //public async Task<Face> GetFaceAsync(int minScore)
        //{
        //    return await Task.Run(() => this.GetFace(minScore));
        //}

        public Face GetBestCapturedFace()
        {
            return GetBestCapturedFaces(1).FirstOrDefault();
        }

        public List<Face> GetBestCapturedFaces(int facecount)
        {
            return Faces.OrderByDescending(f => f.FaceScore).Take(facecount).ToList();
        }


        /// <summary>
        /// main element of capturing the camera and playing back. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void ProcessFrame(object sender, EventArgs arg)
        {

            Mat mat = capture.QueryFrame();
            Image<Bgr, Byte> ImageFrame = mat.ToImage<Bgr, Byte>();
            Console.WriteLine("FaceCapture ProcessFrame start" + mat);

            if (ImageCaptured != null)
            {
                ImageCaptured(this);
            }

            MotionInfo motion = this.GetMotionInfo(mat);

            List<Face> FoundFaces = FaceDetector.FindFaces(ImageFrame, this.FaceTrainingFile, this.EyeTrainingFile, this.Scale, this.Neighbors, this.FaceMinSize);

            foreach (Face face in FoundFaces)
            {
                face.MotionObjects = motion.MotionObjects;
                face.MotionPixels = motion.MotionPixels;

                if (FaceCaptured != null)
                {
                    FaceCaptured(this, face);
                }
                Faces.Add(face);
            }
        }

        private MotionInfo GetMotionInfo(Mat image)
        {

            Mat _forgroundMask = new Mat();
            Mat _segMask = new Mat();
            MotionInfo motionInfoObj = new MotionInfo();
            double minArea, angle, objectCount, totalPixelCount;
            double overallangle = 0;
            double  motionPixelCount =0;
            int motionArea =0; 
            totalPixelCount = 0;
            objectCount = 0;
            minArea = 800;

            if (foregroundDetector == null)
            {
                foregroundDetector = new BackgroundSubtractorMOG2();
            }

            foregroundDetector.Apply(image, _forgroundMask);

            _motionHistory.Update(_forgroundMask);

            ImageForeGroundMaskLast = _forgroundMask.ToImage<Bgr, byte>();

            #region get a copy of the motion mask and enhance its color
            double[] minValues, maxValues;
            Point[] minLoc, maxLoc;
            _motionHistory.Mask.MinMax(out minValues, out maxValues, out minLoc, out maxLoc);
            Mat motionMask = new Mat();
            using (ScalarArray sa = new ScalarArray(255.0 / maxValues[0]))
                CvInvoke.Multiply(_motionHistory.Mask, sa, motionMask, 1, DepthType.Cv8U);
            //Image<Gray, Byte> motionMask = _motionHistory.Mask.Mul(255.0 / maxValues[0]);
            #endregion

            //create the motion image 
            Image<Bgr, Byte> motionImage = new Image<Bgr, byte>(motionMask.Size);
            //display the motion pixels in blue (first channel)
            //motionImage[0] = motionMask;
            CvInvoke.InsertChannel(motionMask, motionImage, 0);

            //Threshold to define a motion area, reduce the value to detect smaller motion
            minArea = 100;
         //storage.Clear(); //clear the storage
         Rectangle[] rects;

         using (VectorOfRect boundingRect = new VectorOfRect())
         {
             _motionHistory.GetMotionComponents(_segMask, boundingRect);
             rects = boundingRect.ToArray();
         }

         //iterate through each of the motion component
         foreach (Rectangle comp in rects)
         {
            int area = comp.Width * comp.Height;
            //reject the components that have small area;
            _motionHistory.MotionInfo(_forgroundMask, comp, out angle, out motionPixelCount);
            if (area < minArea) continue;
            else
            {
                overallangle = overallangle + angle; 
                totalPixelCount = totalPixelCount + motionPixelCount;
                objectCount = objectCount + 1;
                motionArea = motionArea + area; 
            }

            // find the angle and motion pixel count of the specific area

            ////Draw each individual motion in red
            //DrawMotion(motionImage, comp, angle, new Bgr(Color.Red));
         }
            motionInfoObj.MotionArea = motionArea; 
            motionInfoObj.OverallAngle = overallangle;
            motionInfoObj.BoundingRect = rects;
            motionInfoObj.TotalMotions = rects.Length;
            motionInfoObj.MotionObjects = objectCount;
            motionInfoObj.MotionPixels = totalPixelCount;
            Console.WriteLine(" GetMotionInfo - Total Motions found: " + rects.Length + "; Motion Pixel count: " + totalPixelCount);
         return motionInfoObj;
        }
    }
}

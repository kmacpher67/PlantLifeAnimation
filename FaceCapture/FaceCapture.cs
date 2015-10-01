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
using System.Diagnostics;
using System.Threading;

namespace PlantLifeAnimationForm
{
    public delegate void FaceCapturedEventHandler(object sender, Face face);
    public delegate void ImageCapturedEventHandler(object sender);

    public class FaceCapture
    {
        private Capture capture;
        private MotionHistory _motionHistory;
        public List<MotionInfo> motions = new List<MotionInfo>();
        private BackgroundSubtractor foregroundDetector;
        public string FaceTrainingFile { get; set; }
        public string EyeTrainingFile { get; set; }
        public double Scale { get; set; }
        public int Neighbors { get; set; }
        public int FaceMinSize { get; set; }
        public int FaceMaxSize { get; set; }
        public List<Face> Faces { get; set; }

        public double EyeScale { get; set; }
        public int EyeNeighbors { get; set; }
        public int EyeFaceMinSize { get; set; }
        public int EyeFaceMaxSize { get; set; }

        public int reductionWidth = 640;
        public double reductionRatio = 1; // assumes input source is 320x200;  1/2 640/400

        public double motionHistoryDuration = 1.1;
        public double maxDelta = 0.05;
        public double minDelta = 0.5;

        public int frameCount = 0;
        public Size captureScreenSize = new Size();

        /// <summary>
        /// average movement of pixels weighted smoothed .75 / .25 new
        /// </summary>
        public double averagetotalPixelCount =1;
        public double motionPixelsAvg = 0;
        public double motionPixelsSmooth = 0.8; 

        private IFindFaces FaceDetector;
        public static bool HasCuda = false;

        public Image<Bgr, Byte> ImageFrameLast;
        public Image<Bgr, Byte> ImageMotionLast;
        public Image<Bgr, byte> ImageForeGroundMaskLast; 

        public event FaceCapturedEventHandler FaceCaptured;
        public event ImageCapturedEventHandler ImageCaptured;

        public Stopwatch timermotion = new Stopwatch();
        public Stopwatch timerfaces = new Stopwatch();
        public Stopwatch timerlastfacecapture = new Stopwatch();

        public bool faceProcessing = false;

        /// <summary>
        /// Motion change threshold, indicator to check for a new face 
        /// </summary>
        public double MotionThreshold = 0.9
            ; 


        public FaceCapture(string faceTrainingFile, string eyeTrainingFile)
            : this(faceTrainingFile, eyeTrainingFile, 1.12, 8, 12)
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

            try
            {
                if (HasCuda && CudaInvoke.HasCuda)
                {
                    FaceDetector = new FaceDetectCuda();
                }
                else
                {
                    FaceDetector = new FaceDetect();
                }
            }
            catch (Exception errCuda)
            {
                Console.WriteLine("ERROR - FaceCapture HasCuda="+HasCuda+" err=" + errCuda);
                FaceDetector = new FaceDetect();
            }

            _motionHistory = new MotionHistory(
                motionHistoryDuration, //in second, the duration of motion history you wants to keep
                maxDelta, //in second, maxDelta for cvCalcMotionGradient
                minDelta); //in second, minDelta for cvCalcMotionGradient
            //capture = new Capture();
        }

        /// <summary>
        /// start and initial motion capture
        /// </summary>
        public void StartCapture()
        {
            Console.WriteLine("StartCapture");
            Faces = new List<Face>();
            capture = new Capture();
            _motionHistory = new MotionHistory(1.0, 0.05, 0.5);
            Application.Idle += ProcessFrame;
        }

        /// <summary>
        /// stop and displose of capture device
        /// </summary>
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


        /// <summary>
        /// first best face score
        /// </summary>
        /// <returns></returns>
        public Face GetBestCapturedFace()
        {
            return GetBestCapturedFaces(1).FirstOrDefault();
        }

        /// <summary>
        /// gets list of faces based scores
        /// </summary>
        /// <param name="facecount"></param>
        /// <returns></returns>
        public List<Face> GetBestCapturedFaces(int facecount)
        {
            return Faces.OrderByDescending(f => f.FaceScore).Take(facecount).ToList();
        }

        /// <summary>
        /// main element of capturing the camera and playing back. 
        /// Camera source input for capture.QueryFrame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void ProcessFrame(object sender, EventArgs arg)
        {
            Mat mat = capture.QueryFrame();
            if(mat==null)
            {
                Console.WriteLine("CAMERA CONNECTED????   MAT NULL");
                return;
            }
            Image<Bgr, Byte> ImageFrame = mat.ToImage<Bgr, Byte>();
            if ((frameCount++)%10==0)
                Console.WriteLine("FC ProcessFrame fc=" + frameCount + " dt=" + DateTime.Now + " tmo="+timermotion.Elapsed + " tface=" + timerfaces.Elapsed);

            this.processNewFaceImages(mat, ImageFrame);
            if (ImageCaptured != null)
            {
                ImageFrameLast = ImageFrame;
                ImageCaptured(this); //FIRE event for registered handlers. 
            }

            if (motions.Count > 100)
                motions.RemoveRange(0, 10);
        }
        /// <summary>
        /// async await face checking in background thread 
        /// </summary>
        /// <returns></returns>
        public async void checkFace(Image<Bgr, Byte> ImageFrame)
        {
            faceProcessing = true;
            timerfaces.Restart();

            //        var t = Task.Factory.StartNew(async delegate {
            //            await Task.Delay(1000);
            //            return FaceDetector.FindFaces(ImageFrame.Resize(reductionRatio, Inter.Cubic), FaceTrainingFile, EyeTrainingFile, Scale, Neighbors, FaceMinSize); },
            //CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            //Task< List < Face >> FoundFacesTask = await Task.Factory.StartNew(() =>
            //{
            //    var x = FaceDetector.FindFacesAsync(ImageFrame.Resize(reductionRatio, Inter.Cubic), FaceTrainingFile, EyeTrainingFile, Scale, Neighbors, FaceMinSize);
            //});
            //Task taskslow = new Task<List<Face>>(() => FaceDetector.FindFaces(ImageFrame.Resize(reductionRatio, Inter.Cubic), FaceTrainingFile, EyeTrainingFile, Scale, Neighbors, FaceMinSize));
            //taskslow.Start();

            List<Face> FoundFaces = await Task.Run(() => FaceDetector.FindFaces(ImageFrame.Resize(reductionRatio, Inter.Cubic), FaceTrainingFile, EyeTrainingFile, Scale, Neighbors, FaceMinSize));

            timerfaces.Stop();
            Debug.WriteLine(timerfaces.Elapsed);
            foreach (Face face in FoundFaces)
            {
                face.MotionObjects = motions.Last().MotionObjects;
                face.MotionPixels = motions.Last().MotionPixels;
                motionPixelsAvg = motionPixelsAvg * motionPixelsSmooth + motions.Last().MotionPixels * (1 - motionPixelsSmooth);
                face.MotionPixelsAvg = motionPixelsAvg;

                if (FaceCaptured != null)
                {
                    FaceCaptured(this, face);
                }
                Faces.Add(face);
            }
            timerlastfacecapture.Restart();
            faceProcessing = false;
        }

        void processNewFaceImages(Mat mat, Image<Bgr, Byte> ImageFrame)
        {
            timermotion.Restart();
            //  array of motion info MotionInfo motion =
            motions.Add(this.GetMotionInfo(mat));
            timermotion.Stop();

            bool changed = calculateMotion();
            // only calc face stuff if changes are big. 
            if (Faces.Count < 1 || changed || timerlastfacecapture.ElapsedMilliseconds>6000)
            {
                reductionRatio = (double)reductionWidth / (double)ImageFrame.Width;
                if (!faceProcessing)
                {
                    faceProcessing = true;
                    checkFace(ImageFrame);
                }
            }

        }

        private bool calculateMotion(MotionInfo curMotion = null)
        {
            bool isKewl = true;

            if (motions.Count < 2)
                return false;

            if (curMotion==null)
                 curMotion = motions.Last();

            if (motions.Count < 2)
                return false; 

            MotionInfo motionCur = motions.Last();

            if (motionCur.BoundingRect.Count() < 1)
                return false;

            double normalizedMo = Math.Abs(curMotion.SmoothedAvg - curMotion.MotionPixels) / curMotion.MotionPixels;
            if (normalizedMo > MotionThreshold)
                isKewl = true;

                return isKewl;
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
            //Image<Bgr, Byte> motionImage = new Image<Bgr, byte>(motionMask.Size);
            ////display the motion pixels in blue (first channel)
            ////motionImage[0] = motionMask;
            //CvInvoke.InsertChannel(motionMask, motionImage, 0);

            //Threshold to define a motion area, reduce the value to detect smaller motion
            minArea = 100;
         //storage.Clear(); //clear the storage
         Rectangle[] rects;

         using (VectorOfRect boundingRect = new VectorOfRect())
         {
             _motionHistory.GetMotionComponents(_segMask, boundingRect);
             rects = boundingRect.ToArray();
         }

            // REMOVED THIS COSTS TOO MUCH TO ITERATE

            ////iterate through each of the motion component
            //foreach (Rectangle comp in rects)
            //{
            //   int area = comp.Width * comp.Height;
            //   //reject the components that have small area;
            //   _motionHistory.MotionInfo(_forgroundMask, comp, out angle, out motionPixelCount);
            //   if (area < minArea) continue;
            //   else
            //   {
            //       overallangle = overallangle + angle; 
            //       totalPixelCount = totalPixelCount + motionPixelCount;
            //       objectCount = objectCount + 1;
            //       motionArea = motionArea + area; 
            //   }

            // find the angle and motion pixel count of the specific area

            ////Draw each individual motion in red
            //DrawMotion(motionImage, comp, angle, new Bgr(Color.Red));
            //}

            // find and draw the overall motion angle
            double overallAngle, overallMotionPixelCount;
            _motionHistory.MotionInfo(_forgroundMask, new Rectangle(Point.Empty, motionMask.Size), out overallAngle, out overallMotionPixelCount);

            motionInfoObj.MotionArea = motionArea; 
            motionInfoObj.OverallAngle = overallAngle;
            motionInfoObj.BoundingRect = rects;
            motionInfoObj.TotalMotions = rects.Length;
            motionInfoObj.MotionObjects = objectCount;
            motionInfoObj.MotionPixels = overallMotionPixelCount;
            averagetotalPixelCount = 0.75 * averagetotalPixelCount + 0.25 * overallMotionPixelCount;
            motionInfoObj.SmoothedAvg = averagetotalPixelCount;

            if (this.calculateMotion(motionInfoObj))
                Console.WriteLine(" GetMotionInfo Total Motions=" + rects.Length + "; Pixel count=" + overallMotionPixelCount+"; ang=" + overallAngle);

            return motionInfoObj;
        }
    }
}

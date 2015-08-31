using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace PlantLifeAnimationForm
{
    class FaceCounter
    {
        public Capture cap;
        public CascadeClassifier face_cascade;
        public int NumberOfFacesCounted;
        public double NumberOfFacesCounting;
        public double scaleFactor = 1.1;
        public Size minSize = new Size(20, 20); //By default, it is set to the size of samples the classifier has been trained on ( \sim 20\times 20 for face detection)
        public Size maxSize = new Size(300, 300);
        public int minNeighbors = 3;
        public Image<Gray, byte> grayframe;
        public Image<Bgr, byte> lastImageCaptured;

        public FaceCounter()
        {
            Debug.WriteLine("FaceCounter started again" );
            //Configure();
            Debug.WriteLine("FaceCounter Configured");
        }

        /**
         * captures camera, not used if currently having image and converting to grey scale. 
         * 
         */
        public Image<Gray, byte> captureImageCamera()
        {
            // there's only one channel (greyscale), hence the zero index
            //var faces = nextFrame.DetectHaarCascade(haar)[0];
            using (Image<Bgr, byte> nextFrame = cap.QueryFrame())
            {
                lastImageCaptured = nextFrame;
                grayframe = nextFrame.Convert<Gray, byte>();
            }
            return grayframe;
        }

        public Image<Gray, byte> convertColorImageSetGrey(Image<Bgr, byte> nextFrame)
        {
            // if there is no writeline, then input image is null.
            if(nextFrame !=null)
            {
                Debug.WriteLine("Does this image=lastimg?=" + nextFrame);
                lastImageCaptured = nextFrame;
                grayframe = nextFrame.Convert<Gray, byte>();
            }
            return grayframe;
        }

        public int faceCounter()
        {
            Debug.WriteLine("FaceCounter setting counter");

                Debug.WriteLine("faceCounter query frame" + grayframe);
                if (grayframe != null)
                {

                    //-- Detect faces
                    //face_cascade.DetectMultiScale()
                    Rectangle[] faces = face_cascade.DetectMultiScale(grayframe, scaleFactor, minNeighbors, minSize, maxSize);

                    NumberOfFacesCounted = 0;
                    foreach (var face in faces)
                    {
                        NumberOfFacesCounted++;
                    }
                    NumberOfFacesCounting = NumberOfFacesCounting * .75 + NumberOfFacesCounted * .25; 
                }
            
            return NumberOfFacesCounted;
        }

        public void ConfigureCapture()
        {
            Debug.WriteLine("FaceCounter Configur ing");
            try { 
            // passing 0 gets zeroth webcam
            cap = new Capture(0);
            }
            catch (Exception exp)
            {
                Debug.WriteLine("error null? =" + exp);
            }
        }

        public void ConfigureCascade()
        {
            Debug.WriteLine("FaceCounter Configur ing");
            try
            {
                // adjust path to find your xml
                //haar = new HaarCascade("..\\..\\..\\..\\lib\\haarcascade_frontalface_alt2.xml");
                //haar = new HaarCascade("haar\\haarcascade_frontalface_default.xml");
                // HaarCascade ?? OLD because I used old dll so it happened.
                //Debug.WriteLine("has Cude Cuda really? = " + CudaInvoke.HasCuda);
                // adjust path to find your xml
                //face_cascade = new CascadeClassifier("haar\\haarcascade_frontalface_alt2.xml");
                //haarcascade_frontalface_default.xml
                face_cascade = new CascadeClassifier("haar\\haarcascade_frontalface_default.xml");
                //haar = new HaarCascade("haar\\haarcascade_frontalface_default.xml");
                //haar = new HaarCascade( "haar\\haarcascade_frontalface_alt2.xml");
            }
            catch (Exception exp)
            {
                Debug.WriteLine("error null? =" + exp);
            }
        }
    }
}

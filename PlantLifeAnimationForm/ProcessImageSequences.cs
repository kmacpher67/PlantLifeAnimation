using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Windows.Forms;
using System.Threading;

namespace PlantLifeAnimationForm
{

    class ProcessImageSequences
    {
        private Capture capture;

        public string imageContentDir { get; set; }
        public Stopwatch timeInterval = new Stopwatch();
        public List<Image<Bgr, byte>> paintingJPEGMov = new List<Image<Bgr, byte>>();
        public int paintingJPEGMovIndex = 0;

        public ProcessImageSequences()
        {
            imageContentDir = "images\\zone2\\0247 Painting photo JPEG.mov";
            capture = new Capture(imageContentDir);
            Debug.WriteLine("starting ProcessImageSequences capture =  " + capture.ToString());
        }

        public void StartCapture()
        {
            Console.WriteLine("StartCapture for =" + imageContentDir);
            capture = new Capture(imageContentDir);
            Application.Idle += ProcessFrame;
        }

        public void StopCapture()
        {
            Console.WriteLine("StopCapture");
            Application.Idle -= ProcessFrame;
            capture.Dispose();
            capture = null;
        }

        /// <summary>
        /// main element of capturing the camera and playing back. 
        /// Camera source input for capture.QueryFrame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void ProcessFrame(object sender, EventArgs arg)
        {
            Debug.WriteLine(" ProcessFrame=" + paintingJPEGMovIndex);
            processMovIntoImageArray();

        }

        public void processMovIntoImageArray()
        {
            Debug.WriteLine(" processMovIntoImageArray=" + paintingJPEGMovIndex);
            try
            {
                while (true)
                {
                    Mat mat = capture.QueryFrame();
                    Image<Bgr, Byte> ImageFrame = mat.ToImage<Bgr, Byte>();
                    paintingJPEGMov.Add(ImageFrame);
                    paintingJPEGMovIndex++;
                    Thread.Sleep(15);
                }

            }
            catch (Exception exp)
            {
                StopCapture();
            }
        }

    }
}

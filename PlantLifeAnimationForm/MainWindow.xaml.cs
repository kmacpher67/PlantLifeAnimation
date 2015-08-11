using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PlantLifeAnimationForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int mainpicturecounter =0;
        ImageSourceConverter c;
        public PlantLifeImagesService plantlifeImages;
        Capture capWebcam = null;
        bool blnCaptureingInProcess = false;
        Image<Bgr, Byte> imgOriginal;
        Image<Bgr, Byte> imgVanGoh;
        Image<Bgr, Byte> imgProcessed;
        List<Image<Gray, Byte>> faces;
        CascadeClassifier active_cascade = null;
        CascadeClassifier face_cascade;
        CascadeClassifier face2_cascade;
        CascadeClassifier facea_cascade;
        CascadeClassifier facep_cascade;
        CascadeClassifier bodyu_cascade;
        CascadeClassifier bodyf_cascade;
        CascadeClassifier bodyl_cascade; //haarcascade_lowerbody.xml
        OpenFileDialog OF = new OpenFileDialog();
        SaveFileDialog SF = new SaveFileDialog();
        //current video mode and state
        bool playstate = false;
        bool recordstate = false;

        VideoMethod CurrentState = VideoMethod.Viewing; //default state
        public enum VideoMethod
        {
            Viewing,
            Recording
        };

        double scaleFactor = 1.07;
        int minNeighbors = 2; //Minimum number (minus 1) of neighbor rectangles that makes up an object. All the groups of a smaller number of rectangles than min_neighbors-1 are rejected. If min_neighbors is 0, the function does not any grouping at all and returns all the detected candidate rectangles, which may be useful if the user wants to apply a customized grouping procedure
        Size minSize = new Size(10, 10); //By default, it is set to the size of samples the classifier has been trained on ( \sim 20\times 20 for face detection)
        Size maxSize = new Size(200, 200);
        int frameCount = 0;
        int trainingCount = 0;
        Stopwatch watch = new Stopwatch();
        Rectangle[] lastRectangle = null;
        Bgr drawBoxColor = new Bgr(0, double.MaxValue, 50);

        FaceCounter fc = new FaceCounter();
        int NumberOfFaces = 0;
        double NumberOfFacesSmooth = 0;
        double resizeImage = 1;
        double maxWidth = 360;

        DispatcherTimer dispatcherTimer;
        DateTimeOffset startTime;
        DateTimeOffset lastTime;
        DateTimeOffset stopTime;
        int timesTicked = 1;
        int timesToTick = 1000;
        FaceCounter faceCounter; 

        public MainWindow()
        {
            InitializeComponent();
            c = new ImageSourceConverter();
            var uri = new System.Uri("ms-appx:///images/logo.png");
            //var file = Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            //ImageMain.Source= ImageSource
            plantlifeImages = new PlantLifeImagesService();
            plantlifeImages.initializeJSONFile();
            faceCounter = new FaceCounter();


            Console.WriteLine("main window completed!!!");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Window_Loaded");
            try
            {
                //c = new ImageSourceConverter();
                //BitmapImage bitmapimage = plantlifeImages.plantLifeImages[0].PlantImage;
                ////object mybutt = c.ConvertFrom(bitmapimage.GetHbitmap());
                ImageMain.Source = (ImageSource)plantlifeImages.plantLifeImages[0].PlantImage;

                capWebcam = new Capture(); // assocate capture object to default web cam
                capturePicture();

                this.dispatcherTimerSetup();
                
                blnCaptureingInProcess = true;
            }
            catch (Exception loadedERR)
            {
                Console.WriteLine("ERROR - Window_Loaded" + loadedERR);
            }
        }

        public void dispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            //Application.Idle += processFrameAndUpdateGUI;
            dispatcherTimer.Tick += processFrameAndUpdateGUI;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            startTime = DateTimeOffset.Now;
            lastTime = startTime;
            dispatcherTimer.Start();
        }

        void processFrameAndUpdateGUI(object sender, EventArgs e)
        {
            frameCount++;
            if (frameCount > 30)
            {
                watch.Stop();
                var timeSpan = watch.ElapsedMilliseconds;
                frameCount = 0;
                Debug.WriteLine("frame count 30= " + timeSpan + " ms");
                watch.Restart();
            }
            capturePicture();
            Console.WriteLine(" imgProcessed =" + imgProcessed + " imgOriginal= " + imgOriginal);

        }


        private void configHaar()
        {
            try
            {
                watch.Start();
                //Debug.WriteLine("has Cude GPU really? = " + GpuInvoke.HasCuda);
                // adjust path to find your xml
                //face_cascade = new CascadeClassifier("haar\\haarcascade_frontalface_alt2.xml");
                //haarcascade_frontalface_default.xml
                face_cascade = new CascadeClassifier("haar\\haarcascade_frontalface_default.xml");
                face2_cascade = new CascadeClassifier("haar\\haarcascade_frontalface_alt2.xml");
                facea_cascade = new CascadeClassifier("haar\\haarcascade_frontalface_alt.xml");
                facep_cascade = new CascadeClassifier("haar\\haarcascade_profileface.xml");
                bodyl_cascade = new CascadeClassifier("haar\\haarcascade_lowerbody.xml");
                bodyu_cascade = new CascadeClassifier("haar\\haarcascade_upperbody.xml");
                bodyf_cascade = new CascadeClassifier("haar\\haarcascade_fullbody.xml");
                active_cascade = face_cascade;

            }
            catch (Exception exp)
            {
                Debug.WriteLine("error null? =" + exp);
            }
        }

        void capturePicture()
        {
            //Debug.WriteLine("processFrameAndUpdateGUI");
            imgOriginal = capWebcam.QueryFrame(); // gets next frame
            if (imgOriginal == null) return;
            // (double)imageBoxOrig.Size.Width
            resizeImage = maxWidth / (double)imgOriginal.Size.Width;
            imgProcessed = imgOriginal.Resize(resizeImage, INTER.CV_INTER_CUBIC);

        }


        private void ImageMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("ImageMain_MouseUp=" + mainpicturecounter);
            if ( ++mainpicturecounter >= plantlifeImages.plantLifeImages.Count)
                mainpicturecounter = 0;

                ImageMain.Source = (ImageSource)plantlifeImages.plantLifeImages[mainpicturecounter].PlantImage;
        }

        public BitmapImage mergeTwoImages(BitmapImage innerImage, BitmapImage backimage)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                //Inner Image
                WriteableBitmap wb = new WriteableBitmap(innerImage);

                //Frame Images
                WriteableBitmap wbFinal = new WriteableBitmap(backimage);

                Image image = new Image();
                image.Height = innerImage.PixelHeight;
                image.Width = innerImage.PixelWidth;
                image.Source = innerImage;

                // TranslateTransform                      
                TranslateTransform tf = new TranslateTransform();
                tf.X = 52;
                tf.Y = 60;
                //wbFinal.

                //wbFinal.Invalidate();
                //wbFinal.SaveJpeg(mem, wb.PixelWidth, wb.PixelHeight, 0, 100);
                //mem.Seek(0, System.IO.SeekOrigin.Begin);

                // Show image.               
                return backimage;
            }
        }

    }
}

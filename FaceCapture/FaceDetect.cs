using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantLifeAnimationForm
{
    class FaceDetect : IFindFaces
    {
        public double eyescale = 1.04;
        public int eyeneighbors = 0;
        public int eyeminsize = 2;
        public int eyemaxsize = 100;
        public bool useOcl = true;

        public Task<List<Face>> FindFacesAsync(Image<Bgr, Byte> image, String faceFileName, string eyeFileName, double scale, int neighbors, int minSize)
        {
            return Task.Factory.StartNew(() =>
                {
                    return FindFaces(image,faceFileName, eyeFileName, scale, neighbors, minSize) ;
                });
        }


        public List<Face> FindFaces(Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> image, string faceFileName, string eyeFileName, double scale, int neighbors, int minSize)
        {
           
            List<Face> faces = new List<Face>();
            List<Rectangle> facesRect = new List<Rectangle>();
            List<Rectangle> eyesRect = new List<Rectangle>();
            try
            {
                using (CascadeClassifier face = createClassifier(faceFileName))
                {
                    using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>())
                    {
                        gray._EqualizeHist();
                        Rectangle[] facesDetected = face.DetectMultiScale(gray, scale, neighbors, new Size(minSize, minSize), Size.Empty);

                        foreach (Rectangle f in facesDetected)
                        {
                            using (Image<Gray, Byte> faceImg = gray.GetSubRect(f))
                            {
                                using (Image<Gray, Byte> clone = faceImg.Clone())
                                {
                                    Face facemodel = new Face();
                                    eyesRect = new List<Rectangle>(FindEyes(eyeFileName, clone));
                                    if (eyesRect != null && eyesRect.Count>0)
                                    {
                                        facemodel.EyesRects = eyesRect;
                                        facemodel.EyesCount = eyesRect.Count;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    facemodel.FaceImage = clone.Bitmap;
                                    facemodel.Height = facemodel.FaceImage.Height;
                                    facemodel.Width = facemodel.FaceImage.Width;
                                    facemodel.FaceRect = f;
                                    facemodel.FramePosX = f.X;
                                    facemodel.FramePosY = f.Y;
                                    facemodel.ImageFrameSize = image.Size;

                                    Gray avgf = new Gray();
                                    MCvScalar avstd = new MCvScalar();
                                    clone.AvgSdv(out avgf, out avstd);
                                    facemodel.StdDev = avstd.V0;
                                    faces.Add(facemodel);
                                    if (faces.Count%5==0)
                                        Console.WriteLine("FaceDetect OpenCL every5 Add faceModel" + facemodel.Width);

                                    break;
                                }
                            }
                            gray.ROI = Rectangle.Empty;
                        }
                    }
                }
            }
            catch (Exception errFaceDet)
            {
                Console.WriteLine("ERROR - faceDetect OpenCL =" + errFaceDet);
            }
            return faces;
        }

        private CascadeClassifier createClassifier(String ccfilename)
        {

            return new CascadeClassifier(ccfilename);
        }

        public Rectangle[] FindEyes(string eyeFileName, Image<Gray, Byte> imageFace)
        {
            using (CascadeClassifier eye = createClassifier(eyeFileName))
            using (Mat eyeRegionMat = new Mat())
            {
                Rectangle[] eyeRegion = eye.DetectMultiScale(imageFace, eyescale, eyeneighbors, new Size(eyeminsize, eyeminsize), new Size(eyemaxsize, eyemaxsize));
                return eyeRegion;
            }
        }

    }
}

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
        public double eyescale = 1.1;
        public int eyeneighbors = 8;
        public int eyeminsize = 8; 
        public List<Face> FindFaces(Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> image, string faceFileName, string eyeFileName, double scale, int neighbors, int minSize)
        {
           
            List<Face> faces = new List<Face>();
            List<Rectangle> facesRect = new List<Rectangle>();
            List<Rectangle> eyesRect = new List<Rectangle>();
            using (CascadeClassifier face = new CascadeClassifier(faceFileName))
            using (CascadeClassifier eye = new CascadeClassifier(eyeFileName))
            {
                using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>())
                {
                    gray._EqualizeHist();
                    Rectangle[] facesDetected = face.DetectMultiScale(gray, scale, neighbors, new Size(minSize, minSize), Size.Empty);

                    foreach (Rectangle f in facesDetected)
                    {
                        if (f.Width<80)
                            break;
                        gray.ROI = f;

                            using (Image<Gray, Byte> faceImg = gray.GetSubRect(f))
                            {
                                using (Image<Gray, Byte> clone = faceImg.Clone())
                                {
                                    Rectangle[] eyesDetected = FindEyes(eyeFileName, clone);
                                    if (eyesDetected.Count() > 0)
                                    {
                                        Face facemodel = new Face();
                                        facemodel.FaceImage = clone.Bitmap;
                                        facemodel.Height = facemodel.FaceImage.Height;
                                        facemodel.Width = facemodel.FaceImage.Width;
                                        facemodel.FaceRect = f;
                                        eyesRect.AddRange(eyesDetected);
                                        facemodel.EyesRects.AddRange(eyesRect);
                                        facemodel.EyesCount = eyesRect.Count;
                                        Gray avgf = new Gray();
                                        MCvScalar avstd = new MCvScalar();
                                        clone.AvgSdv(out avgf, out avstd);
                                        facemodel.StdDev = avstd.V0;
                                        faces.Add(facemodel);

                                        Console.WriteLine("FaceDetect no gpuCUDA Add faceModel" + facemodel.FaceScore);
                                        break;
                                    }
                                }
                            }
                        
                        gray.ROI = Rectangle.Empty;
                    }
                }
            }
            return faces;
        }

        public Rectangle[] FindEyes(string eyeFileName, Image<Gray, Byte> image)
        {
            using (CascadeClassifier eye = new CascadeClassifier(eyeFileName))
            using (Mat eyeRegionMat = new Mat())
            {
                Rectangle[] eyeRegion =  eye.DetectMultiScale(image, eyescale, eyeneighbors, new Size(eyeminsize, eyeminsize), Size.Empty);
                return eyeRegion;
            }
        }

    }
}

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
    class FaceDetection 
    {

        public double scaleFace = 1.08;
        public int neighborsFace = 3; 
        public int minSizeFace = 24; 
        public int maxSizeFace = 220;

        public double scaleEye = 1.02;
        public int neighborsEye = 5; 
        public int minSizeEye = 4; 
        public int maxSizeEye = 80; 


        public List<FaceScored> FindFaces(Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> image, CascadeClassifier cascadeClassifierFace, CascadeClassifier cascadeClassifierEye)
        {

        List<FaceScored> currentFaces = new List<FaceScored>();
        using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>())
            {
            gray._EqualizeHist();
            Size minFaceSize = new Size(minSizeFace , minSizeFace );
            Size maxFaceSize =  new Size(maxSizeFace , maxSizeFace );
            Size minEyeSize = new Size(minSizeEye , minSizeEye );
            Size maxEyeSize =  new Size(maxSizeEye , maxSizeEye );
            Rectangle[] facesDetected = cascadeClassifierFace.DetectMultiScale(gray, scaleFace , neighborsFace , minFaceSize,maxFaceSize);

            foreach (Rectangle f in facesDetected)
            {
                if (f.Width<80)
                    break;
                gray.ROI = f;

                Rectangle[] eyesDetected = cascadeClassifierEye.DetectMultiScale(gray, scaleEye, neighborsEye, minEyeSize, maxEyeSize);
                if (eyesDetected.Count() >0){
                    FaceScored faceModel = new FaceScored();
                    faceModel.FaceImage = gray.Bitmap;
                    faceModel.FaceImageFullColr = image.GetSubRect(f).Bitmap;
                    faceModel.Height = faceModel.FaceImage.Height;
                    faceModel.Width = faceModel.FaceImage.Width;
                    faceModel.EyesCount = eyesDetected.Count();

                    Gray avgf = new Gray();
                    MCvScalar avstd = new MCvScalar();
                    gray.AvgSdv(out avgf, out avstd);
                    faceModel.StdDev = avstd.V0;

                    currentFaces.Add(faceModel);
                    Console.WriteLine("FaceDetect Add faceModel" + faceModel.FaceScore);
                    break;
                }
                gray.ROI = Rectangle.Empty;
                }
            }
        return currentFaces;
        }
    }
}

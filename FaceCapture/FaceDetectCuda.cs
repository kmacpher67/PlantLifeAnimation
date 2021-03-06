﻿using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantLifeAnimationForm
{
    class FaceDetectCuda : IFindFaces
    {
        public Task<List<Face>> FindFacesAsync(Image<Bgr, Byte> image, String faceFileName, string eyeFileName, double scale, int neighbors, int minSize)
        {
            return Task.Factory.StartNew(() =>
            {
                return FindFaces(image, faceFileName, eyeFileName, scale, neighbors, minSize);
            });
        }
        public List<Face> FindFaces(Image<Bgr, byte> image, string faceFileName, string eyeFileName, double scale, int neighbors, int minSize)
        {
            List<Face> faces = new List<Face>();
            List<Rectangle> facesRect = new List<Rectangle>();
            List<Rectangle> eyesRect = new List<Rectangle>();
            try
            {
                //Console.WriteLine(" FaceDetectGPU FindFaces faceFileName=" + faceFileName + " cuda = " + CudaInvoke.HasCuda);
                using (CudaCascadeClassifier face = new CudaCascadeClassifier(faceFileName))
                {
                    using (CudaImage<Bgr, Byte> CudaImage = new CudaImage<Bgr, byte>(image))
                    using (CudaImage<Gray, Byte> CudaGray = CudaImage.Convert<Gray, Byte>())
                    using (GpuMat region = new GpuMat())
                    {

                        face.DetectMultiScale(CudaGray, region);
                        Rectangle[] faceRegion = face.Convert(region);
                        facesRect.AddRange(faceRegion);
                        foreach (Rectangle f in faceRegion)
                        {
                            using (CudaImage<Gray, Byte> faceImg = CudaGray.GetSubRect(f))
                            {
                                using (CudaImage<Gray, Byte> clone = faceImg.Clone(null))
                                {
                                    Face facemodel = new Face();
                                    eyesRect = new List<Rectangle>(FindEyes(eyeFileName, clone));
                                    if (eyesRect != null)
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
                                    clone.ToImage().AvgSdv(out avgf, out avstd);
                                    facemodel.StdDev = avstd.V0;
                                    faces.Add(facemodel);
                                    if (facemodel.FaceScore > 39)
                                        Console.WriteLine("FaceDetect USING gpuCUDA Add faceModel" + facemodel.FaceScore);

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception cudaerrJones)
            {
                Console.WriteLine("cudaerrJones = " + cudaerrJones);
            }

            return faces;
        }

        private Rectangle[] FindEyes(string eyeFileName, CudaImage<Gray, Byte> image)
        {
            using (CudaCascadeClassifier eye = new CudaCascadeClassifier(eyeFileName))
            using (GpuMat eyeRegionMat = new GpuMat())
            {
                eye.DetectMultiScale(image, eyeRegionMat);
                Rectangle[] eyeRegion = eye.Convert(eyeRegionMat);
                return eyeRegion;
            }
        }
    }
}

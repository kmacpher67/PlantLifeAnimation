using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PlantLifeAnimationForm
{
    public class FaceScoring
    {
        public List<FaceScored> lastFaces { get; set; }


        /// <summary>
        /// static variable used for scaling image array based on face size
        /// </summary>
        public static int FaceSizeMax = 120;

        public FaceScoring()
        {

        }

        public List<FaceScored> GetFacesScored(List<FaceScored> newFacesToScore)
        {
            List<FaceScored> foundfaces = new List<FaceScored>();

            if (newFacesToScore != null && lastFaces!=null && newFacesToScore.Equals(lastFaces))
            {
                Console.WriteLine("GetFacesScored already evaluated this one");
                return newFacesToScore;
            }
            else if (newFacesToScore == null)
            {
                Console.WriteLine("GetFacesScored newFacesToScore is NULL");
                return null;
            }

            foreach (FaceScored fc in newFacesToScore)
            {

            }


            lastFaces= foundfaces; 
            return foundfaces;
        }


        public FaceScored getClosest(List<FaceScored> faces)
        {
            return orderClosetoFar(faces).First();
        }

        public FaceScored getFurthest(List<FaceScored> faces)
        {
            return orderClosetoFar(faces).Last();
        }

        public List<FaceScored> orderClosetoFar(List<FaceScored> fc)
        {
           return fc.OrderByDescending(f => f.Width).ToList();
        }
        public List<FaceScored> GetBestCapturedFaces(List<FaceScored> newFacesToScore, int facecount)
        {
            return newFacesToScore.OrderByDescending(f => f.FaceScore).Take(facecount).ToList();
        }


    }
}

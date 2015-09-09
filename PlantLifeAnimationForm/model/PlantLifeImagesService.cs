using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PlantLifeAnimationForm
{
    public class PlantLifeImagesService
    {
        public List<PlantLifeImage> plantLifeImages = new List<PlantLifeImage>();

        public PlantLifeImagesService()
        {
            loadImages();

        }

        public Bitmap handleFacedScoredInput(List<FaceScored> faces)
        {

            Bitmap bm = plantLifeImages.FirstOrDefault<PlantLifeImage>().PlantImage;
            if (faces != null)
            {

                FaceScoring fsc = new FaceScoring();
                FaceScored faceclosest = fsc.getClosest(faces);
                FaceScored facefurthest = fsc.getFurthest(faces);
                if (faces.Count==1)
                {
                    int faceWidth = faces[0].Width; 
                    int plantlifeindex = (int)(faceWidth/320)*plantLifeImages.Count;
                    plantlifeindex = (plantlifeindex >= plantLifeImages.Count) ? plantLifeImages.Count - 1 : plantlifeindex;
                    bm = plantLifeImages[plantlifeindex].PlantImage;
                }
                else
                {
                    bm = plantLifeImages.Find(x => x.numberOfPeople == faces.Count).PlantImage;
                }

            }

           return bm;        
        }

        /// <summary>
        /// targetDir is used for ImageType put types into custom directories and load for automatic categorization. 
        /// </summary>
        /// <param name="targetDir"></param>
        public void loadImages(string targetDir = "images")
        {
            Console.WriteLine("plant life loadImages");
            string[] files = Directory.GetFiles(targetDir, "*");
            foreach (var f in files)
            {
                PlantLifeImage plantLifeImage = new PlantLifeImage();
                plantLifeImage.PlantImage = getImageFromFile(f);
                plantLifeImage.ImageFileName = f;
                plantLifeImage.ImageName = f;
                plantLifeImage.ImageType = targetDir;
                plantLifeImages.Add(plantLifeImage);
            }

        }
        public Bitmap getImageFromFile(String filePath)
        {
            Console.WriteLine("getImageFromFile=" + filePath);

            Bitmap bitmapSource = null;
            try
            {
                bitmapSource = (Bitmap)System.Drawing.Image.FromFile(filePath);
               
                //Uri fileUri = new Uri(filePath, UriKind.Relative);

                //bitmapSource.BeginInit();
                //bitmapSource.CacheOption = BitmapCacheOption.None;
                //bitmapSource.UriSource = fileUri;
                //bitmapSource.EndInit();
            }
            catch (Exception eek)
            {
                Console.WriteLine("ERROR: getImageFromFile" + eek);
            }
            return bitmapSource;
        }

        public void initializeJSONFile()
        {
            PlantLifeImage pli = new PlantLifeImage();
            pli.DistanceAway = 10;
            pli.ImageFileName = "images/closer.jpg";
            pli.ImageName = "closer.jpg";
            pli.ImageType = "outdoor";
            pli.PlantLifeAnimationId = 1;
            pli.TimeOfDay = "any";

            List<PlantLifeImage> jPlantlifeimages = new List<PlantLifeImage>();
            JsonSerializer serializer = new JsonSerializer();

            try
            {
                jPlantlifeimages.Add(pli);
                using (StreamWriter sw = new StreamWriter(@"model/images.txt"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, jPlantlifeimages);
                }
            }
            catch(Exception errInit)
            {
                Console.WriteLine("initializeJSONFile ERROR = " + errInit);
            }

        }

    }
}

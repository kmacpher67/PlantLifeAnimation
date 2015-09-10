using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PlantLifeAnimationForm
{
    public class PlantLifeImagesService
    {
        public List<PlantLifeImage> plantLifeImages = new List<PlantLifeImage>();
        public List<PlantLifeImage> plantLifeImagesOver = new List<PlantLifeImage>();
        public int currentOverlayIndex = 0; 

        public PlantLifeImagesService()
        {
            loadImages("images/complex");
            loadOverlayImages(); // use default butterfly
        }

        public Bitmap handleFacedScoredInput(List<Face> faces)
        {

            Bitmap bm = plantLifeImages.FirstOrDefault<PlantLifeImage>().PlantImage;
            if (faces != null)
            {

                FaceScoring fsc = new FaceScoring();
                //FaceScored faceclosest = fsc.getClosest(faces);
                //FaceScored facefurthest = fsc.getFurthest(faces);
                if (faces.Count>=1)
                {
                    
                    //  bm = plantLifeImages.Find(x => x.numberOfPeople == faces.Count).PlantImage;
                    try {
                        int faceWidth = faces[faces.Count-1].Width;
                        int plantlifeindex = (int)(1.0 * faceWidth / 320 * plantLifeImages.Count);
                        plantlifeindex = (plantlifeindex >= plantLifeImages.Count) ? plantLifeImages.Count - 1 : plantlifeindex;
                        bm = plantLifeImages[plantlifeindex].PlantImage;
                        if (faces[faces.Count - 1].framePosX > 99)
                        {
                            Console.WriteLine("HOLY COW LESS TAN 100 frameposx=" + faces[faces.Count - 1].framePosX);
                            // TODO stubbed out overlay image onto another image trickery
                            bm = appplyOverlayImage(bm, faces[faces.Count - 1].framePosX);
                        }
                    }
                    catch (Exception errint)
                    {
                        Console.WriteLine("ERROR - handleFacedScoredInput =" + errint);
                    }
                }
            }

           return bm;        
        }

        public Bitmap appplyOverlayImage(Bitmap bm, int p = 0)
        {
            //TODO overlay image2 onto image1
            asdf


            return bm;
        }

        public void loadOverlayImages(string targetDir = "images/butterfly")
        {
            Console.WriteLine("loadOverlayImages plant life loadImages");
            string[] files = Directory.GetFiles(targetDir, "*");
            foreach (var f in files)
            {
                PlantLifeImage plantLifeImage = new PlantLifeImage();
                plantLifeImage.PlantImage = getImageFromFile(f);
                plantLifeImage.ImageFileName = f;
                plantLifeImage.ImageName = f;
                plantLifeImage.ImageType = targetDir;
                plantLifeImagesOver.Add(plantLifeImage);

                FrameDimension dimension = new FrameDimension(plantLifeImage.PlantImage.FrameDimensionsList[0]);
                // Number of frames
                int frameCount = plantLifeImage.PlantImage.GetFrameCount(dimension);
                Console.WriteLine(" PlantImage.GetFrameCount=" + frameCount + " for file:" + f);
            }
        }

        public void reloadImages(string targetDir = "images")
        {
            List<PlantLifeImage> plantLifeImagesOld = plantLifeImages;
            plantLifeImages.Clear();
            loadImages(targetDir);
            if (plantLifeImages == null || plantLifeImages.Count == 0)
            {
                plantLifeImages = plantLifeImagesOld;
            }
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

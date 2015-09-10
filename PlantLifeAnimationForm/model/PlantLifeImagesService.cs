﻿using Newtonsoft.Json;
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
        public int currentOverlayFrame = 0;
        public Size frameSize = new Size(640, 480);

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
            try{
                Bitmap finalImage = new Bitmap(frameSize.Width,frameSize.Height);
                FrameDimension dimension = new FrameDimension(plantLifeImagesOver[0].PlantImage.FrameDimensionsList[0]);
                // Number of frames
                int frameCount = plantLifeImagesOver[0].PlantImage.GetFrameCount(dimension);
                Bitmap[] overlayBMframes = ParseFrames( plantLifeImagesOver[0].PlantImage);
                Bitmap img2 = overlayBMframes[currentOverlayFrame++];
                if(currentOverlayFrame>=frameCount)
                    currentOverlayFrame=0;

                using (Graphics g = Graphics.FromImage(finalImage))
                {
                //go through each image and draw it on the final image (Notice the offset; since I want to overlay the images i won't have any offset between the images in the finalImage)
                    int offset = 0;
                    // only using hte main image adn overlay (initially it's a butterfly on crazy green background
                    //foreach (Bitmap image in images)
                    //{
                        g.DrawImage(bm, new Rectangle(offset, 0, bm.Width, bm.Height));
                        g.DrawImage(bm, new Rectangle(offset, 0, img2.Width, img2.Height));
                    //} 
                }
                bm=finalImage;
            }
            catch(Exception errbm)
            {
                Console.WriteLine("ERROR -  appplyOverlayImage = " + errbm);
            }
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


        /// <summary>
        /// Parses individual Bitmap frames from a multi-frame Bitmap into an array of Bitmaps
        /// Get the number of animation frames to copy into a Bitmap array
        /// 
        /// </summary>
        /// <param name="Animation"></param>
        /// <returns>Bitmap[] Copy the animation frame</returns>

        public Bitmap[] ParseFrames(Bitmap Animation)
        {
            // Get the number of animation frames to copy into a Bitmap array

            int Length = Animation.GetFrameCount(FrameDimension.Time);

            // Allocate a Bitmap array to hold individual frames from the animation

            Bitmap[] Frames = new Bitmap[Length];

            // Copy the animation Bitmap frames into the Bitmap array

            for (int Index = 0; Index < Length; Index++)
            {
                // Set the current frame within the animation to be copied into the Bitmap array element

                Animation.SelectActiveFrame(FrameDimension.Time, Index);

                // Create a new Bitmap element within the Bitmap array in which to copy the next frame

                Frames[Index] = new Bitmap(Animation.Size.Width, Animation.Size.Height);

                // Copy the current animation frame into the new Bitmap array element

                Graphics.FromImage(Frames[Index]).DrawImage(Animation, new Point(0, 0));
            }

            // Return the array of Bitmap frames

            return Frames;
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

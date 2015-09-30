using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PlantLifeAnimationForm
{
    public class PlantLifeImagesService
    {
        public List<PlantLifeImage> plantLifeImages = new List<PlantLifeImage>();
        public List<PlantLifeImage> plantLifeImagesOver = new List<PlantLifeImage>();
        public int currentOverlayIndex = 0;
        public int currentOverlayFrame = 0;
        public List<Int16> currentOverlayFrames = new List<Int16>();
        public Size frameSize = new Size(1062, 573);

        public double thresholdMotionValue = 9999;
        public string rapidMotionOverlay = "images/butterfly\\animated-butterfly-image-0005.gif"; 

        public PlantLifeImagesService()
        {
            Bitmap bmONE =  getImageFromFile("images/butterfly/animated-butterfly-image-0005.gif");

            loadMovie(); // load background complex as .mov frame by frame. 
            //loadImages("images/complex");
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
                        int plantlifeindex = (int)(1.0 * faceWidth / FaceScoring.FaceSizeMax * plantLifeImages.Count);
                        plantlifeindex = (plantlifeindex >= plantLifeImages.Count) ? plantLifeImages.Count - 1 : plantlifeindex;
                        bm = plantLifeImages[plantlifeindex].PlantImage;
                        if (faces[faces.Count - 1].FramePosX > 99)
                        {
                            if (faces.Count%5==0)
                                Console.WriteLine(" -- plantlifeindex=" + plantlifeindex + " framePosX=" + faces[faces.Count - 1].FramePosX);
                            // TODO stubbed out overlay image onto another image trickery
                            bm = appplyOverlayImage(bm, findOverlayIndexByFaceData(faces[faces.Count - 1]));
                            if(faces[faces.Count - 1].MotionPixelsAvg>thresholdMotionValue)
                            {
                                int oindex = findOverlayIndexByName(rapidMotionOverlay);
                                bm = appplyOverlayImage(bm, oindex);
                            }
                            double deviation = (faces[faces.Count - 1].MotionPixelsAvg / thresholdMotionValue);
                            if (deviation > 5 || deviation<0.2)
                            {
                                thresholdMotionValue = faces[faces.Count - 1].MotionPixelsAvg;
                            }
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

        public int findOverlayIndexByFaceData(Face faceTarget)
        {
            int olay = 0;

            int totalNumber = plantLifeImagesOver.Count();

            double scaleIndex = (double)faceTarget.FramePosX / 1299.0;
            if (scaleIndex > 0.99)
                scaleIndex = 0.99;

            olay = Convert.ToInt32(scaleIndex * totalNumber);

            return olay; 
        }

        public int findOverlayIndexByName(string filename)
        {
            int pos=0;
            try{
                pos = plantLifeImagesOver.FindLastIndex(x => x.ImageFileName == filename);
                }
            catch(Exception errname){
                Console.WriteLine("findOverlayIndexByName=" + errname);
            }
            
            return pos;
        }
        public Bitmap getPlantLifeImageByKey(int p = 0)
        {
            Bitmap overlayImage = (Bitmap)plantLifeImagesOver[0].PlantImage.Clone();

            try
            {
                overlayImage = (Bitmap)plantLifeImagesOver[p].PlantImage.Clone();
            }
            catch (Exception errOverlay)
            {
                Console.WriteLine("getPlantLifeImageByKey=" + errOverlay);
            }
            return overlayImage;
        }

        /// <summary>
        /// bm is the main background image and p is the overlay image index from the plant life overlay for this scene
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public Bitmap appplyOverlayImage(Bitmap bm, int p = 0)
        {
            //TODO overlay image2 onto image1
            try{
                Bitmap finalImage = (Bitmap)bm.Clone();
                FrameDimension dimension = new FrameDimension(getPlantLifeImageByKey(p).FrameDimensionsList[0]);
                // Number of frames
                int frameCount = plantLifeImagesOver[0].PlantImage.GetFrameCount(dimension);
                Bitmap[] overlayBMframes = ParseFrames( plantLifeImagesOver[0].PlantImage);
                Bitmap img2 = overlayBMframes[currentOverlayFrame++];
                if(currentOverlayFrame>=frameCount)
                    currentOverlayFrame=0;

                using (Graphics g = Graphics.FromImage(finalImage))
                {
                //go through each image and draw it on the final image (Notice the offset; since I want to overlay the images i won't have any offset between the images in the finalImage)
                    int offsetX = 0;
                    int offsetY = 0;
                    // only using hte main image adn overlay (initially it's a butterfly on crazy green background
                    //foreach (Bitmap image in images)
                    //{
                        g.DrawImage(bm, new Rectangle(offsetX, offsetY, bm.Width, bm.Height));
                        // TODO set the overlay position based on face head position
                        offsetX = bm.Width / 4;
                        offsetY = bm.Height / 4;
                        g.DrawImage(img2, new Rectangle(offsetX, offsetY, img2.Width, img2.Height));
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
                Console.Write("fn=" + f + " ");
                plantLifeImage.ImageName = f;
                plantLifeImage.ImageType = targetDir;
                Console.Write(" targetDir=" + targetDir + " ");
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

        public void loadMovie(string movelocation = "images\\zone2\\0247 Painting photo JPEG.mov")
        {
            ProcessImageSequences processImageSeq = new ProcessImageSequences();
            processImageSeq.StartCapture();
            Thread.Sleep(50);

            for (int x=0; x<=processImageSeq.paintingJPEGMovIndex ; x++)
            {
                PlantLifeImage plantLifeImage = new PlantLifeImage();
                plantLifeImage.PlantImage = processImageSeq.paintingJPEGMov[x].ToBitmap();
                plantLifeImages.Add(plantLifeImage);
            }
        }

        /// <summary>
        /// targetDir is used for ImageType put types into custom directories and load for automatic categorization. 
        /// </summary>
        /// <param name="targetDir"></param>
        public void loadImages(string targetDir = "images")
        {
            Console.WriteLine("plant life loadImages targetDir="+targetDir);
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
                using (Bitmap temp = (Bitmap)System.Drawing.Image.FromFile(filePath))
                {
                    bitmapSource = (Bitmap)temp.Clone();
                }
                
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

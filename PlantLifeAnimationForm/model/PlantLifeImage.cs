using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PlantLifeAnimationForm
{
    /// <summary>
    /// model class that represents a display image of plants to interact with
    /// </summary>
    public class PlantLifeImage
    {
        public int PlantLifeAnimationId;

        /// <summary>
        /// unique name of the plant 
        /// </summary>
        public string ImageName;

        /// <summary>
        /// image type "love" "big" "small" for alternative lookup
        /// </summary>
        public string ImageType;

        /// <summary>
        /// attribute of image for people a certain distance or scaler number relative to distance away (perhaps size of face) 
        /// </summary>
        public int DistanceAway;

        /// <summary>
        /// number of people
        /// </summary>
        public int numberOfPeople;

        /// <summary>
        /// image appropriate for the time of day "morning" "afternoon" "evening" "night" "any" 
        /// </summary>
        public string TimeOfDay;

        /// <summary>
        /// actual bitmap of the image from the directory
        /// </summary>
        public BitmapImage PlantImage;

        /// <summary>
        /// image fileName of original file 
        /// </summary>
        public string ImageFileName;

        
    }
}

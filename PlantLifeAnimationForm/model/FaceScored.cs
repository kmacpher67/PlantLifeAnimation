using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantLifeAnimationForm
{
    public class FaceScored
    {
        public Bitmap FaceImage { get; set; }
        public Bitmap FaceImageFullColr { get; set; }
        public int movementSpeed { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int EyesCount { get; set; }
        public double StdDev { get; set; }
        public double MotionObjects { get; set; }
        public double MotionPixels { get; set; }
        public int FaceScore
        {
            get
            {
                double score = Math.Min(300, Convert.ToDouble(((this.Height + this.Width) / 2)));
                score = score + Convert.ToDouble((this.StdDev * 4));

                //int score = Convert.ToInt32(this.Height + (this.StdDev * 2.45));

                if (this.EyesCount >= 2)
                {
                    score = score + 300;
                }
                else
                {
                    score = score + (this.EyesCount * 150);
                }

                if (this.MotionPixels > 0)
                {
                    score = score - Math.Max(-150, Convert.ToDouble((this.MotionPixels / 100000)));
                }

                if (this.MotionObjects > 0)
                {
                    score = score - Math.Max(-150, Convert.ToDouble((this.MotionObjects * 10)));
                }

                score = ((score / 800) * 100);

                return Convert.ToInt32(score);
            }
        }
    }
}

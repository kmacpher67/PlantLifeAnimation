using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantLifeAnimationForm
{
    public class MotionInfo
    {
        public double MotionObjects { get; set; }
        public double MotionPixels { get; set; }

        public double OverallAngle { get; set; }

        public Mat ForgroundMask { get; set; }

        public int TotalMotions { get; set; }

        public int MotionArea { get; set; }

        public Image<Bgr, Byte> MotionImage { get; set; }

        public Rectangle[] BoundingRect { get; set; }

        public double SmoothedAvg { get; set; }
    }
}

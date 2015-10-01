using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace plantlifeanim4.model
{
    public class ZoneDef
    {
        public int Id { get; set; }
        public string ZoneName { get; set; }
        public Size FrameSize { get; set; } = new Size(320, 210);
        public int DistanceAway { get; set; } = 70;

        public string ImageDir { get; set; }

        #region faceDetectionparams
        public string FaceTrainingFile { get; set; } = "haar\\haarcascade_frontalface_default.xml";
        public string EyeTrainingFile { get; set; } = "haar\\haarcascade_eye.xml";
        public string FaceTrainingFileCuda { get; set; } = "haar_cuda\\haarcascade_frontalface_default.xml";
        public string EyeTrainingFileCuda { get; set; } = "haar_cuda\\haarcascade_eye.xml";
        public double FaceScale { get; set; } = 1.15;
        public int FaceNieghbors { get; set; } = 4;
        public int FaceMinSize { get; set; } = 12;
        public int FaceMaxSize { get; set; } = 200;
        public double EyeScale { get; set; } = 1.15;
        public int EyeNieghbors { get; set; } = 1;
        public int EyeMinSize { get; set; } = 4;
        public int EyeMaxSize { get; set; } = 48;
        #endregion
    }
}

using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantLifeAnimationForm
{
    interface IFindFaces
    {
        List<Face> FindFaces(Image<Bgr, Byte> image, String faceFileName, string eyeFileName, double scale, int neighbors, int minSize);
    }
}

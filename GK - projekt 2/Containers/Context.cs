using GK___projekt_2.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GK___projekt_2.Containers
{
    public static class Context
    {
        public static float psi = 3.0f, fi = 3.0f;
        public static bool isFog = false;
        public static int spotlightScope=1, paintingMode=2;
        public static DirectBitmap dbm;
        public static Matrix4x4 lookAtMatrix, perspectiveProjectionMatrix, translationMatrix;
        public static List<Model> models;
        public static bool isLight1On = false;
        public static bool isLight2On = false;


    }
}

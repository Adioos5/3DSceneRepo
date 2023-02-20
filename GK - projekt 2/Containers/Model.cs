using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;   
using GK___projekt_2.Engine;    
using GK___projekt_2.Geometry;  

namespace GK___projekt_2.Containers
{
    public class Model
    {
        public List<Vector3> normalVectors = new List<Vector3>();
        public List<int> ind = new List<int>();
        public List<int> indT = new List<int>();
        public List<Triangle> triangles = new List<Triangle>();
        public List<Edge> ET = new List<Edge>();
        public List<Edge> AET = new List<Edge>();
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> verticesLocalCoordinates = new List<Vector3>();
        public List<List<int>> verticesTrianglesIndexes = new List<List<int>>();
        public List<List<Triangle>> trianglesBasedOnDistanceFromCenter = new List<List<Triangle>>();
        public List<Vector4> v4List;
        public List<(int, int)> edgesL = new List<(int, int)>();
        public int centerX, centerY;

        public float[,] Z_Buffer { get; set; }

        public Vector3 pointInTriangle = new Vector3(0, 0, 0);
        public Matrix4x4 xRotationMatrix;

        public Vector3 surfaceNormalVector, objectColor, lightColor, lightVector1, lightVector2, lightVector3, eyeVector, normalMapNormalVector;

        public double dist0, dist1, dist2;
        public double side0, side1, side2;
        public double pw, p0, p1, p2;
        public double wholeTriangleArea, triangle0Area, triangle1Area, triangle2Area;
        public double alfa0, alfa1, alfa2;
        
        public double kd, ks, ka;
        public int mirroring;

        public bool drawFromImage = false;

        public LightRoute lightRoute;

        public DirectBitmap dbm;
        public Bitmap imageBitmap;
        public Pen pen = new Pen(Brushes.Black, Constants.THICKNESS);
        public Brush brush = Brushes.White;

        public PictureBox canvas;

        public Model(PictureBox _canvas, DirectBitmap dbm)
        {
            canvas = _canvas;

            for (int l = 0; l < 5; l++)
                trianglesBasedOnDistanceFromCenter.Add(new List<Triangle>());

            PictureBox PictureBox1 = new PictureBox();

            PictureBox1.Image = Logic.ResizeImage(new Bitmap("..\\..\\..\\Images\\ball.jpg"), new Size(canvas.Size.Width, canvas.Size.Height));

            imageBitmap = (Bitmap)PictureBox1.Image;

            kd = 1;
            ks = 0;
            ka = 0;
            mirroring = 1;

            objectColor = new Vector3(1, 1, 1);
            lightColor = new Vector3(1,1,1);
            //lightColor = new Vector3(0.5f,0.5f,0.5f);

            vertices.Add(new Vector3(0, 0, 0));
            verticesLocalCoordinates.Add(new Vector3(0, 0, 0));
            verticesTrianglesIndexes.Add(new List<int>());

            normalVectors.Add(new Vector3(0, 0, 0));

            canvas.Image = dbm.Bitmap;

            lightRoute = new LightRoute(0.7);
            lightRoute.CalculateNewCoordinates(0);

            Z_Buffer = new float[canvas.Width, canvas.Height];
        }
    }
}

using GK___projekt_2.Containers;
using GK___projekt_2.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GK___projekt_2
{
    public class Triangle
    {
        public List<Vector3> Vertices { get; set; }
        public List<Edge> Edges { get; set; }
        public Vector3 NormalVector { get; set; }
        public List<Vector3> NormalVectors { get; set; }
        public bool Proceeded { get; set; }

        public List<(Vector3, double, double)> Pixels { get; set; }

        private List<Edge> initialStateE;
        private List<Vector3> initialStateV;
        private int initialStateYmax;
        private int initialStateYmin;
        private int initialStateXmax;
        private int initialStateXmin;

        public int ymax, xmax;
        public int ymin, xmin; 

        public Triangle(List<Vector3> vertices, List<Vector3> normalVectors)
        {
            NormalVectors = normalVectors;
            Proceeded = false;
            Vertices = vertices;
            Edges = new List<Edge>();
            Pixels = new List<(Vector3, double, double)>();
            
            ymin = (int)vertices[0].Y;
            ymax = (int)vertices[0].Y;
            xmin = (int)vertices[0].X;
            xmax = (int)vertices[0].X;

            foreach (var v in vertices)
            {
                ymin = Math.Min(ymin, (int)v.Y);
                ymax = Math.Max(ymax, (int)v.Y);
            }

            foreach (var v in vertices)
            {
                xmin = Math.Min(xmin, (int)v.X);
                xmax = Math.Max(xmax, (int)v.X);
            }

            NormalVector = new Vector3((float)((normalVectors[0].X + normalVectors[1].X + normalVectors[2].X) / 3.0),
                            (float)((normalVectors[0].Y + normalVectors[1].Y + normalVectors[2].Y) / 3.0),
                            (float)((normalVectors[0].Z + normalVectors[1].Z + normalVectors[2].Z) / 3.0));

            double scalarProduct = Logic.NormalVectorScalarProduct(NormalVector);

            NormalVector = new Vector3((float)((double)NormalVector.X / (double)scalarProduct), (float)((double)NormalVector.Y / (double)scalarProduct), (float)((double)NormalVector.Z / (double)scalarProduct));

            initialStateE = Edges.ToList();
            initialStateV = Vertices.ToList();
            initialStateYmin = ymin;
            initialStateYmax = ymax;
            initialStateXmin = xmin;
            initialStateXmax = xmax;

        }

        public void GeneratePixels()
        {
            Pixels.Clear();

            List<Vector3> vertices = Vertices;
            List<Edge> AET = new List<Edge>();
            vertices = vertices.OrderBy(vertex => vertex.Y).ToList();

            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < vertices.Count-1; j++)
                {
                    if ((int)vertices[j].Y == (int)vertices[j + 1].Y)
                    {
                        if (vertices[j].X > vertices[j + 1].X)
                        {
                            Vector3 temp = vertices[j];
                            vertices[j] = vertices[j + 1];
                            vertices[j + 1] = temp;
                        }
                    }
                }
            }

            int ymin = (int)vertices[0].Y, ymax = (int)vertices[2].Y;
            int y = ymin;

            AET.Add(Edges.Where(e => e.Contains(vertices[0]) && e.Contains(vertices[1])).First());
            AET.Add(Edges.Where(e => e.Contains(vertices[0]) && e.Contains(vertices[2])).First());
            AET = AET.OrderBy(e => e.ScanLineX(y)).ToList();

            double x_CURRENT;
            double x_next;
            double m1 = (float)AET[0].m, m2 = (float)AET[1].m;

            x_CURRENT = vertices[0].X;

            if ((int)vertices[0].Y != (int)vertices[1].Y) x_next = vertices[0].X;
            else x_next = vertices[1].X;

            while (y != ymax)
            {

                if (y == (int)vertices[1].Y)
                {
                    AET.Remove(Edges.Where(e => e.Contains(vertices[0]) && e.Contains(vertices[1])).First());
                    AET.Add(Edges.Where(e => e.Contains(vertices[1]) && e.Contains(vertices[2])).First());
                    m1 = (double)AET[0].m;
                    m2 = (double)AET[1].m;
                }

                AET = AET.OrderBy(e => e.ScanLineX(y)).ToList();
                x_CURRENT = (double)AET[0].ScanLineX(y);
                x_next = (double)AET[1].ScanLineX(y);

                for (int x = (int)x_CURRENT; x <= (int)x_next+1; x++)
                {
                    Pixels.Add(((Vector3, double, double))(new Vector3(x, y, Z_Value(x, y)), x_CURRENT, x_next));
                }

                y++;
            }
        }

        public (double, double, double) CalculateBarycentricCoordinates(float x, float y)
        {
            double dist0, dist1, dist2;
            double side0, side1, side2;
            double pw, p0, p1, p2;
            double wholeTriangleArea, triangle0Area, triangle1Area, triangle2Area;
            double alfa0, alfa1, alfa2;

            dist0 = Math.Sqrt(Math.Pow(x - Vertices[0].X, 2) + Math.Pow(y - Vertices[0].Y, 2));
            dist1 = Math.Sqrt(Math.Pow(x - Vertices[1].X, 2) + Math.Pow(y - Vertices[1].Y, 2));
            dist2 = Math.Sqrt(Math.Pow(x - Vertices[2].X, 2) + Math.Pow(y - Vertices[2].Y, 2));

            side0 = Math.Sqrt(Math.Pow(Vertices[0].X - Vertices[1].X, 2) + Math.Pow(Vertices[0].Y - Vertices[1].Y, 2));
            side1 = Math.Sqrt(Math.Pow(Vertices[1].X - Vertices[2].X, 2) + Math.Pow(Vertices[1].Y - Vertices[2].Y, 2));
            side2 = Math.Sqrt(Math.Pow(Vertices[2].X - Vertices[0].X, 2) + Math.Pow(Vertices[2].Y - Vertices[0].Y, 2));

            pw = (side0 + side1 + side2) / 2;
            p0 = (side0 + dist0 + dist1) / 2;
            p1 = (side1 + dist1 + dist2) / 2;
            p2 = (side2 + dist2 + dist0) / 2;

            wholeTriangleArea = Math.Sqrt(pw * (pw - side0) * (pw - side1) * (pw - side2));
            triangle0Area = Math.Sqrt(Math.Abs(p0 * (p0 - side0) * (p0 - dist0) * (p0 - dist1)));
            triangle1Area = Math.Sqrt(Math.Abs(p1 * (p1 - side1) * (p1 - dist1) * (p1 - dist2)));
            triangle2Area = Math.Sqrt(Math.Abs(p2 * (p2 - side2) * (p2 - dist2) * (p2 - dist0)));

            alfa0 = triangle0Area / wholeTriangleArea;
            alfa1 = triangle1Area / wholeTriangleArea;
            alfa2 = triangle2Area / wholeTriangleArea;

            return (alfa0, alfa1, alfa2);
        }

        public float Z_Value(float x, float y)
        {
            (double alfa0, double alfa1, double alfa2) = CalculateBarycentricCoordinates(x, y);
            return (float)(alfa0 * Vertices[2].Z + alfa1 * Vertices[0].Z + alfa2 * Vertices[1].Z);
        }

        public bool Contains(Vector3 p)
        {
            return Vertices.Contains(p);
        }

        public void RefreshToInitialState()
        {
            Vertices = initialStateV.ToList();
            Edges = initialStateE.ToList();
            Proceeded = false;


            xmin = initialStateXmin;
            xmax = initialStateXmax;
            ymin = initialStateYmin;
            ymax = initialStateYmax;
        }
        public void RefreshEdges()
        {
            Edges.Clear();
            Edges.Add(new Edge(Vertices[0], Vertices[1]));
            Edges.Add(new Edge(Vertices[1], Vertices[2]));
            Edges.Add(new Edge(Vertices[2], Vertices[0]));

            ymin = (int)Vertices[0].Y;
            ymax = (int)Vertices[0].Y;
            xmin = (int)Vertices[0].X;
            xmax = (int)Vertices[0].X;

            foreach (var v in Vertices)
            {
                ymin = Math.Min(ymin, (int)v.Y);
                ymax = Math.Max(ymax, (int)v.Y);
            }

            foreach (var v in Vertices)
            {
                xmin = Math.Min(xmin, (int)v.X);
                xmax = Math.Max(xmax, (int)v.X);
            }
        }

        public void ConfigureEdges(List<Edge> ET)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                foreach (var e in ET)
                {
                    if (e.Contains(Vertices[i]) && e.Contains(Vertices[(i+1)%3]) && !Edges.Contains(e))
                    {
                        Edges.Add(e);
                        break;
                    }
                }
            }
        }

        public (Edge eL, Edge eR) EdgesOnScanline(int y)
        {
            List<Edge> edges = new List<Edge>();

            foreach(Edge e in Edges)
            {
                if (e.ymin <= y && e.ymax >= y) edges.Add(e);
            }

            edges = edges.OrderBy(e => e.ScanLineX(y)).ToList();

            return (edges[0], edges[1]);
        }

        public (Vector3 v1, Vector3 v2) Neighbours(Vector3 v)
        {
            return (Vertices[(Vertices.IndexOf(v)+1)%3], Vertices[(Vertices.IndexOf(v) + 2) % 3]);
        }

    }
}

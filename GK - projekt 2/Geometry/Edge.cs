using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GK___projekt_2.Geometry
{
    public class Edge
    {
        public int ymax, ymin;
        public int xmax, xmin;
        public List<Vector3> vertices;
        public double m, b, edge_x;

        public Edge(Vector3 v1, Vector3 v2)
        {
            vertices = new List<Vector3>();

            if (v1.Y < v2.Y)
            {
                Vector3 temp = v1;
                v1 = v2;
                v2 = temp;
            }

            vertices.Add(v1);
            vertices.Add(v2);

            ymax = Math.Max((int)vertices[0].Y, (int)vertices[1].Y);
            ymin = Math.Min((int)vertices[0].Y, (int)vertices[1].Y);
            xmax = Math.Max((int)vertices[0].X, (int)vertices[1].X);
            xmin = Math.Min((int)vertices[0].X, (int)vertices[1].X);

            m = (v2.Y - v1.Y) / (double)(v2.X - v1.X);

            b = v1.Y - m * v1.X;

            edge_x = xmin;
        }

        public bool Contains(Vector3 v)
        {
            return vertices.Contains(v);
        }

        private Point Intersect(Point a1, Point a2, Point b1, Point b2)
        {
            double A1 = a2.Y - a1.Y;
            double B1 = a1.X - a2.X;
            double C1 = A1 * a1.X + B1 * a1.Y;

            double A2 = b2.Y - b1.Y;
            double B2 = b1.X - b2.X;
            double C2 = A2 * b1.X + B2 * b1.Y;

            double numitor = A1 * B2 - A2 * B1;
            if (numitor == 0) return new Point(0, 0);
            else
            {
                double x = (B2 * C1 - B1 * C2) / numitor;
                double y = (A1 * C2 - A2 * C1) / numitor;
                return new Point(Convert.ToInt32(x), Convert.ToInt32(y));
            }
        }

        public double ScanLineX(double y)
        {
            Point intersectionPoint = Intersect(new Point(0, (int)y), new Point(1, (int)y),
                new Point((int)vertices[0].X, (int)vertices[0].Y), new Point((int)vertices[1].X, (int)vertices[1].Y));

            return intersectionPoint.X;

            //double result = (double)((double)(y - b) / m);
            //if (double.IsInfinity(m)) return xmin;
            //else return result;
        }

    }
}

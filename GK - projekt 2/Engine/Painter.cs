using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GK___projekt_2.Containers;
using GK___projekt_2.Engine;
using GK___projekt_2.Geometry;

namespace GK___projekt_2
{
    
    public static class Painter
    {
        public static float[,] Z_Buffer_Tab;

        public static void PrepareCanvas(Model context, DirectBitmap dbm, Color color)
        {
            for (int y = 0; y < context.canvas.Height; y++)
            {
                for (int x = 0; x < context.canvas.Width; x++)
                {
                    dbm.SetPixel(x, y, color);
                    Painter.Z_Buffer_Tab[x, y] = int.MaxValue;
                }
            }
        }

        public static void DrawSphereNet(Model context)
        {
            using (Graphics g = Graphics.FromImage(context.canvas.Image))
            {
                foreach (var triangle in context.triangles)
                {
                    foreach (var e in triangle.Edges)
                    {
                        Point p1 = new Point((int)e.vertices[0].X, (int)e.vertices[0].Y);
                        Point p2 = new Point((int)e.vertices[1].X, (int)e.vertices[1].Y);
                        g.DrawLine(Pens.White, p1, p2);
                    }
                }
            }
        }

        public static void Z_Buffer(Model context, int fogLevel = 0)
        {

            foreach (Triangle t in context.triangles)
            {
                if ((context.eyeVector.X * t.NormalVector.X +
                     context.eyeVector.Y * t.NormalVector.Y +
                     context.eyeVector.Z * t.NormalVector.Z) > 0) continue;

                t.GeneratePixels();

                foreach ((Vector3, double, double) pxP in t.Pixels)
                {
                    Vector3 px = pxP.Item1;

                    if (!(px.X >= t.xmin && px.X > 0 && px.X <= t.xmax && px.X < context.canvas.Width &&
                        px.Y >= t.ymin && px.Y > 0 && px.Y <= t.ymax && px.Y < context.canvas.Height))
                        continue;

                    float z = t.Z_Value(px.X, px.Y);
                    if (z <= Z_Buffer_Tab[(int)px.X, (int)px.Y])
                    {
                        Color clr = FinalColor((int)px.X, (int)px.Y, z, t, context, pxP.Item2, pxP.Item3, fogLevel);
                        Context.dbm.SetPixel((int)px.X, (int)px.Y, clr);
                        Z_Buffer_Tab[(int)px.X, (int)px.Y] = z;
                    }
                }
            }
        }

        public static Color FinalColor(int x, int y, float z, Triangle? t, Model context, double x_a, double x_b, int fogLevel = 0)
        {
            if (t == null) return Color.White;

            context.lightVector1 = Vector3.Normalize(new Vector3((float)-context.lightRoute.X, (float)context.lightRoute.Y, -1));
            context.lightVector2 = Vector3.Normalize(new Vector3(0, -1000, 0));
            context.lightVector3 = Vector3.Normalize(new Vector3(-1000, 1000, 0));

            (double r, double g, double b) finalColor = (0, 0, 0);

            if (Context.paintingMode == 0)
            {

                Logic.CalculateBarycentricCoordinates(ref context, x, y, t);

                context.surfaceNormalVector = new Vector3((float)(context.alfa0 * t.NormalVectors[2].X + context.alfa1 * t.NormalVectors[0].X + context.alfa2 * t.NormalVectors[1].X),
                                    (float)(context.alfa0 * t.NormalVectors[2].Y + context.alfa1 * t.NormalVectors[0].Y + context.alfa2 * t.NormalVectors[1].Y),
                                    (float)(context.alfa0 * t.NormalVectors[2].Z + context.alfa1 * t.NormalVectors[0].Z + context.alfa2 * t.NormalVectors[1].Z));

                context.surfaceNormalVector = Vector3.TransformNormal(context.surfaceNormalVector, context.xRotationMatrix);

                double cosAngleNL1 = 0.0, cosAngleVR1 = 0.0, cosAngleNL2 = 0.0, cosAngleVR2 = 0.0, cosAngleNL3 = 0.0, cosAngleVR3 = 0.0;
                Logic.CalculateCosinusesUsingByPointInterpolation(context, x, y, context.lightVector1, ref cosAngleNL1, ref cosAngleVR1);
                Logic.CalculateCosinusesUsingByPointInterpolation(context, x, y, context.lightVector2, ref cosAngleNL2, ref cosAngleVR2);
                Logic.CalculateCosinusesUsingByPointInterpolation(context, x, y, context.lightVector3, ref cosAngleNL3, ref cosAngleVR3);

                // MGŁA
                if (Context.isFog && fogLevel == 0) fogLevel = 10;
                float w;
                if (Context.isFog) w = z.Map(0, 100000, 0, fogLevel);
                else w = 0;

                float coeff = 1.0f;
                // SPOTLIGHT
                if (!Context.isFog)
                {
                    Vector3 D = Vector3.Normalize(new Vector3((float)(Math.Cos(Context.fi) * Math.Sin(Context.psi)), (float)(Math.Sin(Context.fi) * Math.Sin(Context.psi)), (float)(Math.Cos(Context.psi))));
                    coeff = (float)Math.Pow(-D.X * context.surfaceNormalVector.X - D.Y * context.surfaceNormalVector.Y - D.Z * context.surfaceNormalVector.Z, Context.spotlightScope);
                    coeff = Math.Max(coeff, 0);
                }

                (double R, double G, double B) I1 = (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL2 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0),
                               Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL2 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0),
                               Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL2 + context.ks * context.lightColor.Z * context.objectColor.Z * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0));

                (double R, double G, double B) I2 = (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL3 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0),
                                               Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL3 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0),
                                               Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL3 + context.ks * context.lightColor.Z * context.objectColor.Z * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0));

                if (!Context.isLight1On) I1 = (0, 0, 0);
                if (!Context.isLight2On) I2 = (0, 0, 0);

                finalColor = (
                    I1.R + I2.R + w + coeff * (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL1 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)),
                    I1.G + I2.G + w + coeff * (Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL1 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)),
                    I1.B + I2.B + w + coeff * (Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL1 + context.ks * context.lightColor.Z * context.objectColor.Y * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)));

                finalColor.r = Math.Max(finalColor.r, 0);
                finalColor.g = Math.Max(finalColor.g, 0);
                finalColor.b = Math.Max(finalColor.b, 0);

                try
                {
                    return Color.FromArgb(255, Math.Min((int)(255 * finalColor.r), 255), Math.Min((int)(255 * finalColor.g), 255), Math.Min((int)(255 * finalColor.b), 255));
                } catch(Exception e)
                {
                    return Color.Black;
                }
            }
            else if (Context.paintingMode == 1)
            {

                List<(double r, double g, double b)> finalColors = new List<(double, double, double)>();
                finalColors.Add((0, 0, 0));
                finalColors.Add((0, 0, 0));
                finalColors.Add((0, 0, 0));

                // MGŁA
                if (Context.isFog && fogLevel == 0) fogLevel = 10;
                float w;
                if (Context.isFog) w = z.Map(0, 100000, 0, fogLevel);
                else w = 0;

                float coeff = 1.0f;
                // SPOTLIGHT
                if (!Context.isFog)
                {
                    Vector3 D = Vector3.Normalize(new Vector3((float)(Math.Cos(Context.fi) * Math.Sin(Context.psi)), (float)(Math.Sin(Context.fi) * Math.Sin(Context.psi)), (float)(Math.Cos(Context.psi))));
                    coeff = (float)Math.Pow(-D.X * context.surfaceNormalVector.X - D.Y * context.surfaceNormalVector.Y - D.Z * context.surfaceNormalVector.Z, Context.spotlightScope);
                    coeff = Math.Max(coeff, 0);
                }

                for (int i = 0; i < finalColors.Count; i++)
                {
                    double cosAngleNL1 = 0.0, cosAngleVR1 = 0.0, cosAngleNL2 = 0.0, cosAngleVR2 = 0.0, cosAngleNL3 = 0.0, cosAngleVR3 = 0.0;
                    Logic.CalculateCosinusesUsingByColorInterpolation(context, t, i, context.lightVector1, ref cosAngleNL1, ref cosAngleVR1);
                    Logic.CalculateCosinusesUsingByColorInterpolation(context, t, i, context.lightVector2, ref cosAngleNL2, ref cosAngleVR2);
                    Logic.CalculateCosinusesUsingByColorInterpolation(context, t, i, context.lightVector3, ref cosAngleNL3, ref cosAngleVR3);

                    (double R, double G, double B) I1 = (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL2 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0),
                                               Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL2 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0),
                                               Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL2 + context.ks * context.lightColor.Z * context.objectColor.Z * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0));

                    (double R, double G, double B) I2 = (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL3 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0),
                                                   Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL3 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0),
                                                   Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL3 + context.ks * context.lightColor.Z * context.objectColor.Z * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0));

                    if (!Context.isLight1On) I1 = (0, 0, 0);
                    if (!Context.isLight2On) I2 = (0, 0, 0);

                    finalColors[i] = (
                        I1.R + I2.R + w + (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL1 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)),
                        I1.G + I2.G + w + (Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL1 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)),
                        I1.B + I2.B + w + (Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL1 + context.ks * context.lightColor.Z * context.objectColor.Y * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)));
                }

                Edge e1 = t.Edges[0], e2 = t.Edges[0];

                List<int> sortedIndexes = new List<int>() { 0, 1, 2 };

                sortedIndexes = sortedIndexes.OrderBy(i => t.Vertices[i].Y).ToList();

                if (y >= t.Vertices[sortedIndexes[0]].Y && y <= t.Vertices[sortedIndexes[1]].Y && y >= t.Vertices[sortedIndexes[0]].Y && y <= t.Vertices[sortedIndexes[2]].Y)
                {
                    e1 = t.Edges.Where(e => e.Contains(t.Vertices[sortedIndexes[0]]) && e.Contains(t.Vertices[sortedIndexes[1]])).First();
                    e2 = t.Edges.Where(e => e.Contains(t.Vertices[sortedIndexes[0]]) && e.Contains(t.Vertices[sortedIndexes[2]])).First();
                }
                else if (y >= t.Vertices[sortedIndexes[0]].Y && y <= t.Vertices[sortedIndexes[1]].Y && y >= t.Vertices[sortedIndexes[1]].Y && y <= t.Vertices[sortedIndexes[2]].Y)
                {
                    e1 = t.Edges.Where(e => e.Contains(t.Vertices[sortedIndexes[0]]) && e.Contains(t.Vertices[sortedIndexes[1]])).First();
                    e2 = t.Edges.Where(e => e.Contains(t.Vertices[1]) && e.Contains(t.Vertices[sortedIndexes[2]])).First();
                }
                else if (y >= t.Vertices[sortedIndexes[0]].Y && y <= t.Vertices[sortedIndexes[2]].Y && y >= t.Vertices[sortedIndexes[1]].Y && y <= t.Vertices[sortedIndexes[2]].Y)
                {
                    e1 = t.Edges.Where(e => e.Contains(t.Vertices[sortedIndexes[0]]) && e.Contains(t.Vertices[sortedIndexes[2]])).First();
                    e2 = t.Edges.Where(e => e.Contains(t.Vertices[sortedIndexes[1]]) && e.Contains(t.Vertices[sortedIndexes[2]])).First();
                }

                List<Edge> edges = new List<Edge>() { e1, e2 };
                edges = edges.OrderBy(e => e.ScanLineX(y)).ToList();

                int i1 = t.Vertices.IndexOf(edges[0].vertices[0]);
                int i2 = t.Vertices.IndexOf(edges[0].vertices[1]);
                int i3 = t.Vertices.IndexOf(edges[1].vertices[0]);
                int i4 = t.Vertices.IndexOf(edges[1].vertices[1]);

                (double R, double G, double B) I_L = (-(255 * finalColors[i1].r * (edges[0].vertices[0].Y - y) / (edges[0].vertices[1].Y - edges[0].vertices[0].Y) + 255 * finalColors[i2].r * (y - edges[0].vertices[1].Y) / (edges[0].vertices[1].Y - edges[0].vertices[0].Y)),
                                                      -(255 * finalColors[i1].g * (edges[0].vertices[0].Y - y) / (edges[0].vertices[1].Y - edges[0].vertices[0].Y) + 255 * finalColors[i2].g * (y - edges[0].vertices[1].Y) / (edges[0].vertices[1].Y - edges[0].vertices[0].Y)),
                                                      -(255 * finalColors[i1].b * (edges[0].vertices[0].Y - y) / (edges[0].vertices[1].Y - edges[0].vertices[0].Y) + 255 * finalColors[i2].b * (y - edges[0].vertices[1].Y) / (edges[0].vertices[1].Y - edges[0].vertices[0].Y)));

                (double R, double G, double B) I_R = (-(255 * finalColors[i3].r * (edges[1].vertices[0].Y - y) / (edges[1].vertices[1].Y - edges[1].vertices[0].Y) + 255 * finalColors[i4].r * (y - edges[1].vertices[1].Y) / (edges[1].vertices[1].Y - edges[1].vertices[0].Y)),
                                                      -(255 * finalColors[i3].g * (edges[1].vertices[0].Y - y) / (edges[1].vertices[1].Y - edges[1].vertices[0].Y) + 255 * finalColors[i4].g * (y - edges[1].vertices[1].Y) / (edges[1].vertices[1].Y - edges[1].vertices[0].Y)),
                                                      -(255 * finalColors[i3].b * (edges[1].vertices[0].Y - y) / (edges[1].vertices[1].Y - edges[1].vertices[0].Y) + 255 * finalColors[i4].b * (y - edges[1].vertices[1].Y) / (edges[1].vertices[1].Y - edges[1].vertices[0].Y)));

                double x_L = edges[0].ScanLineX(y);
                double x_R = edges[1].ScanLineX(y);

                if (x_L == x_R) return Color.Black;

                Color I_P = Color.FromArgb(255, (int)Math.Min(255,Math.Max(0, ((x_R - x) / (x_R - x_L) * I_L.R + (x - x_L) / (x_R - x_L) * I_R.R))),
                                           (int)Math.Min(255, Math.Max(0, ((x_R - x) / (x_R - x_L) * I_L.G + (x - x_L) / (x_R - x_L) * I_R.G))),
                                           (int)Math.Min(255, Math.Max(0, ((x_R - x) / (x_R - x_L) * I_L.B + (x - x_L) / (x_R - x_L) * I_R.B))));

                return I_P;

            }
            else
            {
                context.alfa0 = 1.0 / 3.0;
                context.alfa1 = 1.0 / 3.0;
                context.alfa2 = 1.0 / 3.0;

                context.surfaceNormalVector = new Vector3((float)(context.alfa0 * t.NormalVectors[2].X + context.alfa1 * t.NormalVectors[0].X + context.alfa2 * t.NormalVectors[1].X),
                                    (float)(context.alfa0 * t.NormalVectors[2].Y + context.alfa1 * t.NormalVectors[0].Y + context.alfa2 * t.NormalVectors[1].Y),
                                    (float)(context.alfa0 * t.NormalVectors[2].Z + context.alfa1 * t.NormalVectors[0].Z + context.alfa2 * t.NormalVectors[1].Z));

                context.surfaceNormalVector = Vector3.TransformNormal(context.surfaceNormalVector, context.xRotationMatrix);

                double cosAngleNL1 = 0.0, cosAngleVR1 = 0.0, cosAngleNL2=0.0, cosAngleVR2=0.0, cosAngleNL3 = 0.0, cosAngleVR3 = 0.0;
                Logic.CalculateCosinusesUsingByPointInterpolation(context, x, y, context.lightVector1, ref cosAngleNL1, ref cosAngleVR1);
                Logic.CalculateCosinusesUsingByPointInterpolation(context, x, y, context.lightVector2, ref cosAngleNL2, ref cosAngleVR2);
                Logic.CalculateCosinusesUsingByPointInterpolation(context, x, y, context.lightVector3, ref cosAngleNL3, ref cosAngleVR3);

                // MGŁA
                if (Context.isFog && fogLevel==0) fogLevel = 10;
                float w;
                if (Context.isFog) w = z.Map(0, 100000, 0, fogLevel);
                else w = 0;

                float coeff = 1.0f;
                // SPOTLIGHT
                if (!Context.isFog)
                {
                    Vector3 D = Vector3.Normalize(new Vector3((float)(Math.Cos(Context.fi) * Math.Sin(Context.psi)), (float)(Math.Sin(Context.fi) * Math.Sin(Context.psi)), (float)(Math.Cos(Context.psi))));
                    coeff = (float)Math.Pow(-D.X * context.surfaceNormalVector.X - D.Y * context.surfaceNormalVector.Y - D.Z * context.surfaceNormalVector.Z, Context.spotlightScope);
                    coeff = Math.Max(coeff, 0);
                }

                (double R, double G, double B) I1 = (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL2 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0),
                                               Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL2 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0),
                                               Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL2 + context.ks * context.lightColor.Z * context.objectColor.Z * Logic.MyPow(cosAngleVR2, context.mirroring), 1.0));

                (double R, double G, double B) I2 = (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL3 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0),
                                               Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL3 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0),
                                               Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL3 + context.ks * context.lightColor.Z * context.objectColor.Z * Logic.MyPow(cosAngleVR3, context.mirroring), 1.0));

                if (!Context.isLight1On) I1 = (0, 0, 0);
                if (!Context.isLight2On) I2 = (0, 0, 0);

                finalColor = (
                    I1.R + I2.R + w + coeff * (Math.Min(context.kd * context.lightColor.X * context.objectColor.X * cosAngleNL1 + context.ks * context.lightColor.X * context.objectColor.X * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)),
                    I1.G + I2.G + w + coeff * (Math.Min(context.kd * context.lightColor.Y * context.objectColor.Y * cosAngleNL1 + context.ks * context.lightColor.Y * context.objectColor.Y * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)),
                    I1.B + I2.B + w + coeff * (Math.Min(context.kd * context.lightColor.Z * context.objectColor.Z * cosAngleNL1 + context.ks * context.lightColor.Z * context.objectColor.Y * Logic.MyPow(cosAngleVR1, context.mirroring), 1.0)));


                return Color.FromArgb(255, Math.Min((int)(255 * finalColor.r), 255), Math.Min((int)(255 * finalColor.g), 255), Math.Min((int)(255 * finalColor.b), 255));

            }
        }

        public static void FillTriangle(Model context, Triangle t, Color color)
        {

            t.GeneratePixels();
            foreach(var pxP in t.Pixels)
            {
                Vector3 px = pxP.Item1;
                if (px.X > 0 && px.X < context.canvas.Width && px.Y > 0 && px.Y < context.canvas.Height)
                    ((Bitmap)context.canvas.Image).SetPixel((int)px.X, (int)px.Y, color);
            }
            t.Proceeded = true;
        }
        public static void FillTorus(Model context, Color color)
        {
            List<Triangle> trianglesOnCurrentScanline = new List<Triangle>();

            context.indT.Clear();

            for (int k = 0; k < context.triangles.Count; k++)
                context.indT.Add(k);

            for (int i = 0; i < context.triangles.Count; i++)
                for (int j = 0; j < context.triangles.Count; j++)
                {
                    if (context.triangles[context.indT[i]].ymin < context.triangles[context.indT[j]].ymin)
                    {
                        int temp = context.indT[i];
                        context.indT[i] = context.indT[j];
                        context.indT[j] = temp;
                    }
                }

            int ymin = context.triangles[context.indT[0]].ymin;
            int ymax = context.triangles[context.indT[context.indT.Count-1]].ymax;

            int idxT = 0;

            for (int y = ymin; y < ymax; y++)
            {
                Logic.SortTriangles(context, y, ref idxT, ref trianglesOnCurrentScanline);

                foreach (var tr in trianglesOnCurrentScanline)
                {
                    if (tr.Proceeded) continue;
                    FillTriangle(context, tr, color);
                    tr.Proceeded = true;
                }
            }
        }

    }
}

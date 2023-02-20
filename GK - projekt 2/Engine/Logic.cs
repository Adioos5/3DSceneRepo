using GK___projekt_2.Containers;
using GK___projekt_2.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GK___projekt_2
{
    public static class Logic
    {
        public static float Sign(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public static bool PointInTriangle(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, v1, v2);
            d2 = Sign(pt, v2, v3);
            d3 = Sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        public static double MyPow(double num, int exp)
        {
            double result = 1.0;
            while (exp > 0)
            {
                if (exp % 2 == 1)
                    result *= num;
                exp >>= 1;
                num *= num;
            }

            return result;
        }

        public static void CalculateCosinusesUsingNormalMap(Model context, int k, int y, ref double cosAngleNL, ref double cosAngleVR)
        {
            Color c = context.imageBitmap.GetPixel(k, y);
            (double n1, double n2, double n3) N_texture = (
                                                           Logic.Map(c.R, 0, 255, -1, 1),
                                                           Logic.Map(c.G, 0, 255, -1, 1),
                                                           Logic.Map(c.B, 0, 255, 0, 1)
                                                           );

            (double n1, double n2, double n3) B = context.surfaceNormalVector == new Vector3(0, 0, 1) ? (0, 1, 0) : (context.surfaceNormalVector.Y, -context.surfaceNormalVector.X, 0);
            (double n1, double n2, double n3) T = (B.n2 * context.surfaceNormalVector.Z - B.n3 * context.surfaceNormalVector.Y,
                                                   B.n3 * context.surfaceNormalVector.X - B.n1 * context.surfaceNormalVector.Z,
                                                   B.n1 * context.surfaceNormalVector.Y - B.n2 * context.surfaceNormalVector.X);

            double[,] M = { { T.n1, B.n1, context.surfaceNormalVector.X },
                                    { T.n2, B.n2, context.surfaceNormalVector.Y },
                                    { T.n3, B.n3, context.surfaceNormalVector.Z }};

            context.normalMapNormalVector = new Vector3((float)(M[0, 0] * N_texture.n1 + M[0, 1] * N_texture.n2 + M[0, 2] * N_texture.n3)
                                   , (float)(M[1, 0] * N_texture.n1 + M[1, 1] * N_texture.n2 + M[1, 2] * N_texture.n3)
                                   , (float)(M[2, 0] * N_texture.n1 + M[2, 1] * N_texture.n2 + M[2, 2] * N_texture.n3));

            cosAngleNL = context.normalMapNormalVector.X * context.lightVector1.X + context.normalMapNormalVector.Y * context.lightVector1.Y + context.normalMapNormalVector.Z * context.lightVector1.Z;

            (double n1, double n2, double n3) RVector = (
                2 * (cosAngleNL) * context.normalMapNormalVector.X - context.lightVector1.X,
                2 * (cosAngleNL) * context.normalMapNormalVector.Y - context.lightVector1.Y,
                2 * (cosAngleNL) * context.normalMapNormalVector.Z - context.lightVector1.Z
                );

            cosAngleVR = context.eyeVector.X * RVector.n1 + context.eyeVector.Y * RVector.n2 + context.eyeVector.Z * RVector.n3;

            cosAngleNL = Math.Max(0, cosAngleNL);
            cosAngleVR = Math.Max(0, cosAngleVR);
        }

        public static double NormalVectorScalarProduct(Vector3 normalVector)
        {
            return normalVector.X * normalVector.X +
                normalVector.Y * normalVector.Y +
                normalVector.Z * normalVector.Z;
        }

        public static void CalculateCosinusesUsingByPointInterpolation(Model context, int k, int y, Vector3 lightVector,ref double cosAngleNL, ref double cosAngleVR)
        {
            cosAngleNL = context.surfaceNormalVector.X * lightVector.X + context.surfaceNormalVector.Y * lightVector.Y + context.surfaceNormalVector.Z * lightVector.Z;

            (double n1, double n2, double n3) RVector = (
                2 * (cosAngleNL) * context.surfaceNormalVector.X - lightVector.X,
                2 * (cosAngleNL) * context.surfaceNormalVector.Y - lightVector.Y,
                2 * (cosAngleNL) * context.surfaceNormalVector.Z - lightVector.Z
                );

            cosAngleVR = context.eyeVector.X * RVector.n1 + context.eyeVector.Y * RVector.n2 + context.eyeVector.Z * RVector.n3;

            cosAngleNL = Math.Max(0, cosAngleNL);
            cosAngleVR = Math.Max(0, cosAngleVR);
        }

        public static void CalculateCosinusesUsingByColorInterpolation(Model context, Triangle t, int i, Vector3 lightVector, ref double cosAngleNL, ref double cosAngleVR)
        {
            cosAngleNL = t.NormalVectors[i].X * lightVector.X + t.NormalVectors[i].Y * lightVector.Y + t.NormalVectors[i].Z * lightVector.Z;

            (double n1, double n2, double n3) RVector = (
                2 * (cosAngleNL) * t.NormalVectors[i].X - lightVector.X,
                2 * (cosAngleNL) * t.NormalVectors[i].Y - lightVector.Y,
                2 * (cosAngleNL) * t.NormalVectors[i].Z - lightVector.Z
                );

            cosAngleVR = context.eyeVector.X * RVector.n1 + context.eyeVector.Y * RVector.n2 + context.eyeVector.Z * RVector.n3;

            cosAngleNL = Math.Max(0, cosAngleNL);
            cosAngleVR = Math.Max(0, cosAngleVR);
        }

        public static void CalculateLinearCoordinates(ref Model context, int x, int y, float z, Triangle t)
        {
            Vector3 v = new Vector3(x, y, z);

            float dist1 = Vector3.Distance(t.Vertices[0], v);
            float dist2 = Vector3.Distance(t.Vertices[1], v);
            float dist3 = Vector3.Distance(t.Vertices[2], v);

            float sum = dist1 + dist2 + dist3;
            context.alfa0 = dist1 / sum;
            context.alfa1 = dist2 / sum;
            context.alfa2 = dist3 / sum;
        }

        public static void CalculateBarycentricCoordinates(ref Model context, int x, int y, Triangle t)
        {
            context.dist0 = Math.Sqrt(Math.Pow(x - t.Vertices[0].X, 2) + MyPow(y - t.Vertices[0].Y, 2));
            context.dist1 = Math.Sqrt(Math.Pow(x - t.Vertices[1].X, 2) + MyPow(y - t.Vertices[1].Y, 2));
            context.dist2 = Math.Sqrt(Math.Pow(x - t.Vertices[2].X, 2) + MyPow(y - t.Vertices[2].Y, 2));

            context.side0 = Math.Sqrt(Math.Pow(t.Vertices[0].X - t.Vertices[1].X, 2) + MyPow(t.Vertices[0].Y - t.Vertices[1].Y, 2));
            context.side1 = Math.Sqrt(Math.Pow(t.Vertices[1].X - t.Vertices[2].X, 2) + MyPow(t.Vertices[1].Y - t.Vertices[2].Y, 2));
            context.side2 = Math.Sqrt(Math.Pow(t.Vertices[2].X - t.Vertices[0].X, 2) + MyPow(t.Vertices[2].Y - t.Vertices[0].Y, 2));

            context.pw = (context.side0 + context.side1 + context.side2) / 2;
            context.p0 = (context.side0 + context.dist0 + context.dist1) / 2;
            context.p1 = (context.side1 + context.dist1 + context.dist2) / 2;
            context.p2 = (context.side2 + context.dist2 + context.dist0) / 2;

            context.wholeTriangleArea = Math.Sqrt(context.pw * (context.pw - context.side0) * (context.pw - context.side1) * (context.pw - context.side2));
            context.triangle0Area = Math.Sqrt(Math.Abs(context.p0 * (context.p0 - context.side0) * (context.p0 - context.dist0) * (context.p0 - context.dist1)));
            context.triangle1Area = Math.Sqrt(Math.Abs(context.p1 * (context.p1 - context.side1) * (context.p1 - context.dist1) * (context.p1 - context.dist2)));
            context.triangle2Area = Math.Sqrt(Math.Abs(context.p2 * (context.p2 - context.side2) * (context.p2 - context.dist2) * (context.p2 - context.dist0)));

            context.alfa0 = context.triangle0Area / context.wholeTriangleArea;
            context.alfa1 = context.triangle1Area / context.wholeTriangleArea;
            context.alfa2 = context.triangle2Area / context.wholeTriangleArea;
        }

        public static double Map(double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static Image ResizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        public static void SortVertices(Model context, int y, ref int idx, ref List<Vector3> verticesOnCurrentScanline)
        {
            while (context.vertices[context.ind[idx]].Y < y - 1)
                idx++;
            while (context.vertices[context.ind[idx]].Y == y - 1)
                verticesOnCurrentScanline.Add(context.vertices[context.ind[idx++]]);
        }

        public static void SortTriangles(Model context, int y, ref int idxT, ref List<Triangle> trianglesOnCurrentScanline)
        {
            if (idxT < context.indT.Count && context.triangles[context.indT[idxT]].ymax >= y && context.triangles[context.indT[idxT]].ymin <= y)
            {
                for (int i = 0; i < trianglesOnCurrentScanline.Count; i++)
                {
                    if (trianglesOnCurrentScanline[i].ymax <= y) trianglesOnCurrentScanline.RemoveAt(i);
                }
                do
                {
                    trianglesOnCurrentScanline.Add(context.triangles[context.indT[idxT++]]);
                } while (idxT < context.indT.Count && context.triangles[context.indT[idxT]].ymax >= y && context.triangles[context.indT[idxT]].ymin <= y);
            }
        }

        public static void TransformAllObjects(bool withAnimation = false, int objectId = 2, float angle = 0f)
        {

            for (int i = 0; i < Constants.TORUSES_AMOUNT; i++)
            {
                if (withAnimation && i == objectId) Context.models[i].xRotationMatrix = Matrix4x4.CreateRotationX(angle);

                foreach (var triangle in Context.models[i].triangles)
                {
                    triangle.RefreshToInitialState();

                    for (int j = 0; j < triangle.Vertices.Count; j++)
                    {
                        Vector4 v4 = new Vector4(triangle.Vertices[j].X, triangle.Vertices[j].Y, triangle.Vertices[j].Z, 1);
                        v4 = Vector4.Transform(v4, Context.models[i].xRotationMatrix);
                        if (withAnimation && i == objectId) v4 = Vector4.Transform(v4, Context.translationMatrix);
                        v4 = Vector4.Transform(v4, Context.lookAtMatrix);
                        v4 = Vector4.Transform(v4, Context.perspectiveProjectionMatrix);

                        v4.X /= v4.W;
                        v4.Y /= v4.W;
                        v4.W /= v4.W;

                        v4.X = v4.X * 500 + Context.models[i].canvas.Size.Width / 2;
                        v4.Y = v4.Y * 500 + Context.models[i].canvas.Size.Height / 2;
                        triangle.Vertices[j] = new Vector3(v4.X, v4.Y, v4.Z);
                        triangle.RefreshEdges();
                    }

                }

            }
        }
        public static void ManageAET(Model context, int y, List<Vector3> verticesOnCurrentScanline)
        {
            List<int> trianglesIndexes;
            Triangle triangle;

            foreach (var vertex in verticesOnCurrentScanline)
            {
                trianglesIndexes = context.verticesTrianglesIndexes[context.vertices.IndexOf(vertex)];

                for (int i = 0; i < trianglesIndexes.Count; i++) 
                {
                    triangle = context.triangles[trianglesIndexes[i]];

                    foreach (var edge in triangle.Edges)
                    {
                        if (edge.Contains(vertex) && edge.Contains(triangle.Neighbours(vertex).v2))
                        {
                            if (triangle.Neighbours(vertex).v2.Y >= vertex.Y)
                            {
                                if (!context.AET.Contains(edge)) context.AET.Add(edge);
                            }
                            else
                            {
                                context.AET.Remove(edge);
                            }
                        }
                        else if (edge.Contains(vertex) && edge.Contains(triangle.Neighbours(vertex).v1))
                        {
                            if (triangle.Neighbours(vertex).v1.Y >= vertex.Y)
                            {
                                if (!context.AET.Contains(edge)) context.AET.Add(edge);
                            }
                            else
                            {
                                context.AET.Remove(edge);
                            }
                        }
                    }
                }
            }
            context.AET = context.AET.OrderBy(e => e.ScanLineX(y)).ToList();
        }

        public static void InitializeEngine(Model context, string objPath, int value, int dx, int dy, int dz = 0)
        {
            ReadOBJ(objPath, context, value, dx, dy, dz);

            context.centerX = dx;
            context.centerY = dy;

            foreach (Triangle t in context.triangles)
            {
                t.ConfigureEdges(context.ET);
                t.GeneratePixels();
            }
            context.ind.Clear();
            context.ind.Add(0);

            for (int k = 1; k < context.vertices.Count; k++)
                context.ind.Add(k);

            for (int i = 1; i < context.vertices.Count; i++)
                for (int j = 1; j < context.vertices.Count; j++)
                {
                    if (context.vertices[context.ind[i]].Y < context.vertices[context.ind[j]].Y)
                    {
                        int temp = context.ind[i];
                        context.ind[i] = context.ind[j];
                        context.ind[j] = temp;
                    }
                }

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
        }

        private static void ReadOBJ(string path, Model context, int value, int dx, int dy, int dz)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                while (sr.Peek() >= 0)
                {
                    var line = sr.ReadLine();
                    if (line != null && line[0] == 'v' && line[1] != 'n' && line[1]!='t')
                    {
                        var coordinates = line.Split();

                        context.vertices.Add(new Vector3((int)(double.Parse(coordinates[1], CultureInfo.InvariantCulture) * value + context.canvas.Size.Width / 2) + dx,
                                        (int)(double.Parse(coordinates[2], CultureInfo.InvariantCulture) * value + context.canvas.Size.Height / 2) + dy,
                                        (int)(double.Parse(coordinates[3], CultureInfo.InvariantCulture) * value + context.canvas.Size.Height / 2) + dz));

                        context.verticesTrianglesIndexes.Add(new List<int>());
                    } 
                    else
                    if (line != null && line[0] == 'v' && line[1] == 'n')
                    {
                        var values = line.Split();

                        var n1 = Double.Parse(values[1], CultureInfo.InvariantCulture);
                        var n2 = Double.Parse(values[2], CultureInfo.InvariantCulture);
                        var n3 = Double.Parse(values[3], CultureInfo.InvariantCulture);
                        
                        context.normalVectors.Add(new Vector3((float)n1, (float)n2, (float)n3));
                    }
                    if (line != null && line[0] == 'f')
                    {
                        var indices = line.Split();
                        var nums1 = indices[1].Split('/');
                        var nums2 = indices[2].Split('/');
                        var nums3 = indices[3].Split('/');

                        var num1 = int.Parse(nums1[nums1.Count() - 1]);
                        var num2 = int.Parse(nums2[nums2.Count() - 1]);
                        var num3 = int.Parse(nums3[nums3.Count() - 1]);

                        var v1 = context.vertices[int.Parse(indices[1].ReadUntilCharacter('/'))];
                        var v2 = context.vertices[int.Parse(indices[2].ReadUntilCharacter('/'))];
                        var v3 = context.vertices[int.Parse(indices[3].ReadUntilCharacter('/'))];

                        var v1L = int.Parse(indices[1].ReadUntilCharacter('/'));
                        var v2L = int.Parse(indices[2].ReadUntilCharacter('/'));
                        var v3L = int.Parse(indices[3].ReadUntilCharacter('/'));

                        context.edgesL.Add((v1L, v2L));
                        context.edgesL.Add((v2L, v3L));
                        context.edgesL.Add((v3L, v1L));

                        Vector3 nv1 = context.normalVectors[num1];
                        Vector3 nv2 = context.normalVectors[num2];
                        Vector3 nv3 = context.normalVectors[num3];

                        context.triangles.Add(new Triangle(new List<Vector3> { v1, v2, v3 }, new List<Vector3> { nv1, nv2, nv3 }));

                        context.verticesTrianglesIndexes[context.vertices.IndexOf(v1)].Add(context.triangles.Count - 1);
                        context.verticesTrianglesIndexes[context.vertices.IndexOf(v2)].Add(context.triangles.Count - 1);
                        context.verticesTrianglesIndexes[context.vertices.IndexOf(v3)].Add(context.triangles.Count - 1);

                        context.ET.Add(new Edge(v1, v2));
                        context.ET.Add(new Edge(v2, v3));
                        context.ET.Add(new Edge(v3, v1));
                    }
                }
            }
        }
    }
}

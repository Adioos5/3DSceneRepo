using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic;
using System.Threading;
using System.Windows.Forms.VisualStyles;
using System.IO;
using System.Numerics;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using GK___projekt_2.Containers;
using GK___projekt_2.Geometry;
using GK___projekt_2.Engine;
using Constants = GK___projekt_2.Containers.Constants;

namespace GK___projekt_2
{
    public partial class Form1 : Form
    {
        private List<Camera> cameras;
        private int currentCameraIdx;
        
        private float distanceMultiplier = 0f;

        private static bool isShaking = false;

        private static Color backgroundColor = Color.DarkBlue;

        public static int animationType = 0;

        public Form1()
        {
            InitializeComponent();

            cameras = new List<Camera>();
            
            cameras.Add(new Camera(new Vector3(-10000, 1000, -1000), new Vector3(0, 0, 0), new Vector3(0, 0, 1))); // kamera patrz¹ca z miejsca [0]
            cameras.Add(new Camera(new Vector3(-7000, 5000, -1000), new Vector3(0, 0, 0), new Vector3(0, 0, 1))); // kamera nieruchoma [1]
            cameras.Add(new Camera(new Vector3(-10000, 1000, -1000), new Vector3(0, 0, 0), new Vector3(0, 0, 1))); // kamera 3rd person [2]
            
            currentCameraIdx = 0;

            Context.models = new List<Model>();
            Context.dbm = new DirectBitmap(Canvas.Size.Width, Canvas.Size.Height);

            for (int i = 0; i < Constants.TORUSES_AMOUNT; i++)
            {
                Context.models.Add(new Model(Canvas, Context.dbm));
                Context.models[i].xRotationMatrix = Matrix4x4.CreateRotationX(distanceMultiplier * i);

                Vector3 eyeVector = Vector3.Normalize(cameras[currentCameraIdx].cameraTarget - cameras[currentCameraIdx].cameraPosition);
                Context.models[i].eyeVector = eyeVector;
            }

            Painter.Z_Buffer_Tab = new float[Canvas.Width, Canvas.Height];

            Context.lookAtMatrix = Matrix4x4.CreateLookAt(cameras[currentCameraIdx].cameraPosition, cameras[currentCameraIdx].cameraTarget, cameras[currentCameraIdx].cameraUpVector);
            Context.perspectiveProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)((120.0 / 360.0) * Math.PI), 1f, 50f, 1000f);

            InitializeAllObjects();

            Logic.TransformAllObjects();

            PaintScene();
        }

        private void InitializeAllObjects()
        {
            Logic.InitializeEngine(Context.models[0], "..\\..\\..\\ObjModels\\torus.obj", 500, 0, 0);
            Logic.InitializeEngine(Context.models[1], "..\\..\\..\\ObjModels\\cube.obj", 300, 1000, 1000);
            Logic.InitializeEngine(Context.models[2], "..\\..\\..\\ObjModels\\sphere.obj", 300, -1000, -1000);
            Logic.InitializeEngine(Context.models[3], "..\\..\\..\\ObjModels\\plane.obj", 700, 0, 0);
        }

        public void RefreshToInitialScene()
        {
            Context.lookAtMatrix = Matrix4x4.CreateLookAt(cameras[currentCameraIdx].cameraPosition, 
                cameras[currentCameraIdx].cameraTarget, cameras[currentCameraIdx].cameraUpVector);

            Logic.TransformAllObjects();

            PaintScene();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            double result = (trackBar1.Value / 360.0) * Math.PI;
            Context.perspectiveProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)result, 1f, 50f, 1000f);

            Logic.TransformAllObjects();

            PaintScene();
        }

        private void PaintScene(int fogLevel = 0)
        {
            Painter.PrepareCanvas(Context.models[0], Context.dbm, backgroundColor);

            for (int i = 0; i < Constants.TORUSES_AMOUNT; i++)
            {
                Painter.Z_Buffer(Context.models[i], fogLevel);
            }

            Canvas.Refresh();
        }

        private void CalculateBackgroundColor()
        {
            float RDiff = Color.Cyan.R - Color.DarkBlue.R;
            float GDiff = Color.Cyan.G - Color.DarkBlue.G;
            float BDiff = Color.Cyan.B - Color.DarkBlue.B;

            double MR = RDiff / Math.PI;
            double MG = GDiff / Math.PI;
            double MB = BDiff / Math.PI;

            double k = Context.psi - Math.PI;
            backgroundColor = Context.isFog ? Color.White : Color.FromArgb(Math.Max(0,(int)(MR * k) + Color.DarkBlue.R), Math.Max(0, (int)(MG * k) + Color.DarkBlue.G), Math.Max(0, (int)(MB * k) + Color.DarkBlue.B));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int x = int.Parse(xValue.Text);
            int y = int.Parse(yValue.Text);
            int z = int.Parse(zValue.Text);

            cameras[currentCameraIdx].cameraPosition = new Vector3(x, y, z);
            Vector3 eyeVector = Vector3.Normalize(cameras[currentCameraIdx].cameraTarget - cameras[currentCameraIdx].cameraPosition);
            for (int i = 0; i < Constants.TORUSES_AMOUNT; i++)
            {
                Context.models[i].eyeVector = eyeVector;
            }
            Context.lookAtMatrix = Matrix4x4.CreateLookAt(cameras[currentCameraIdx].cameraPosition, cameras[currentCameraIdx].cameraTarget,
                cameras[currentCameraIdx].cameraUpVector);

            Logic.TransformAllObjects();

            CalculateBackgroundColor();

            PaintScene();
        }

        private void PhongRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PhongRadioButton.Checked) Context.paintingMode = 0;
            PaintScene();
        }

        private void GouraudRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (GouraudRadioButton.Checked) Context.paintingMode = 1;
            PaintScene();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            currentCameraIdx = 0;
            Vector3 eyeVector = Vector3.Normalize(cameras[currentCameraIdx].cameraTarget - cameras[currentCameraIdx].cameraPosition);
            for (int i = 0; i < Constants.TORUSES_AMOUNT; i++)
            {
                Context.models[i].eyeVector = eyeVector;
            }
            RefreshToInitialScene();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            currentCameraIdx = 1;
            Vector3 eyeVector = Vector3.Normalize(cameras[currentCameraIdx].cameraTarget - cameras[currentCameraIdx].cameraPosition);
            for (int i = 0; i < Constants.TORUSES_AMOUNT; i++)
            {
                Context.models[i].eyeVector = eyeVector;
            }
            RefreshToInitialScene();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            currentCameraIdx = 2;
            Vector3 eyeVector = Vector3.Normalize(cameras[currentCameraIdx].cameraTarget - cameras[currentCameraIdx].cameraPosition);
            for(int i = 0; i < Constants.TORUSES_AMOUNT; i++)
            {
                Context.models[i].eyeVector = eyeVector;
            }
            RefreshToInitialScene();
        }

        private void fiAngle_Scroll(object sender, EventArgs e)
        {
            Context.fi = fiAngle.Value / 100.0f;
            RefreshToInitialScene();
        }

        private void psiAngle_Scroll(object sender, EventArgs e)
        {
            Context.psi = psiAngle.Value / 100.0f;

            CalculateBackgroundColor();

            RefreshToInitialScene();
        }

        private void FogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (FogCheckBox.Checked)
            {
                Context.isFog = true;
                backgroundColor = Color.White;
                StartFogAnimation();
            }
            else
            {
                Context.isFog = false;
                CalculateBackgroundColor();
            }
            RefreshToInitialScene();
        }

        private void SpotlightScope_Scroll(object sender, EventArgs e)
        {
            Context.spotlightScope = SpotlightScope.Value;
            RefreshToInitialScene();
        }

        private void ShakingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ShakingCheckBox.Checked) isShaking = true;
            else isShaking = false;

            RefreshToInitialScene();
        }

        private void CPTRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (CPTRadioButton.Checked) Context.paintingMode = 2;
            PaintScene();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            animationType = 0;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            animationType = 1;
        }

        private void StartFogAnimation()
        {
            Context.lookAtMatrix = Matrix4x4.CreateLookAt(cameras[currentCameraIdx].cameraPosition, 
                cameras[currentCameraIdx].cameraTarget, cameras[currentCameraIdx].cameraUpVector);
            Logic.TransformAllObjects();
            
            for(int i = 1; i < 10; i++)
            {
                PaintScene(i);
                Thread.Sleep(1);
            }
        }

        private void StartAnimationButton_Click(object sender, EventArgs e)
        {
            StartAnimationButton.Text = "Animation in progress...";
            StartAnimationButton.Refresh();

            Random r = new Random();
            
            Vector3 deltaPosition = new Vector3(0f, 0f, 0f);

            int shakingVertical = isShaking ? 1000 : 0;
            int shakingHorizontal = 0;
            int temp = Context.models[2].centerY;
            int chosenObject = 2;

            if (animationType == 0)
            {
                float angle = 0f;
                float zIncrease = 100f, angleIncrease = 0.1f;


                for (int q = 0; q < 40; q++)
                {
                    if (q == 20)
                    {
                        zIncrease = -zIncrease;
                        angleIncrease = -angleIncrease;
                    }
                    int rIntX = r.Next(0, shakingVertical);
                    int rIntY = r.Next(0, shakingVertical);
                    int rIntZ = r.Next(0, shakingHorizontal);

                    angle += angleIncrease;
                    deltaPosition = new Vector3(rIntX, rIntY, deltaPosition.Z - zIncrease + rIntZ);

                    Context.models[chosenObject].centerY = (int)(Context.models[chosenObject].centerY + deltaPosition.Y);

                    if (!isShaking)
                    {
                        cameras[0].cameraTarget = new Vector3(cameras[0].cameraTarget.X, Context.models[chosenObject].centerY + 400, deltaPosition.Z - zIncrease);

                        cameras[2].cameraTarget = new Vector3(cameras[2].cameraTarget.X, Context.models[chosenObject].centerY + 400, deltaPosition.Z - 600f);
                        cameras[2].cameraPosition = new Vector3(-7000f, Context.models[chosenObject].centerY + 400, deltaPosition.Z - 600f);
                    }
                    Context.lookAtMatrix = Matrix4x4.CreateLookAt(cameras[currentCameraIdx].cameraPosition,
                        cameras[currentCameraIdx].cameraTarget,
                        cameras[currentCameraIdx].cameraUpVector);
                    Context.translationMatrix = Matrix4x4.CreateTranslation(deltaPosition);

                    Logic.TransformAllObjects(true, chosenObject, angle);

                    PaintScene();

                    Thread.Sleep(1);
                }

                Context.models[2].centerY = temp;
                cameras[2].cameraPosition = new Vector3(-7000f, Context.models[chosenObject].centerY + 400f, -1000f);
            } else
            {
                float xIncrease = 100f;
                
                for (int q = 0; q < 40; q++)
                {
                    if (q == 20)
                    {
                        xIncrease = -xIncrease;
                    }

                    deltaPosition = new Vector3(deltaPosition.X + xIncrease, 0, 0);

                    Context.models[chosenObject].centerX = (int)(Context.models[chosenObject].centerX + xIncrease);

                    cameras[0].cameraTarget = new Vector3(Context.models[chosenObject].centerX-400f, 0f, 0f);

                    cameras[2].cameraTarget = new Vector3(Context.models[chosenObject].centerX, Context.models[chosenObject].centerY + 400, 0f);
                    cameras[2].cameraPosition = new Vector3(Context.models[chosenObject].centerX - 6000f, Context.models[chosenObject].centerY + 400, -600f);

                    Context.lookAtMatrix = Matrix4x4.CreateLookAt(cameras[currentCameraIdx].cameraPosition,
                                                            cameras[currentCameraIdx].cameraTarget,
                                                            cameras[currentCameraIdx].cameraUpVector);
                    Context.translationMatrix = Matrix4x4.CreateTranslation(deltaPosition);

                    Logic.TransformAllObjects(true, chosenObject);

                    PaintScene();

                    Thread.Sleep(1);
                }
            }
            StartAnimationButton.Text = "Start animation";
            StartAnimationButton.Refresh();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Context.isLight1On = true;
            } else
            {
                Context.isLight1On = false;
            }
            RefreshToInitialScene();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Context.isLight2On = true;
            }
            else
            {
                Context.isLight2On = false;
            }
            RefreshToInitialScene();
        }
    }
}
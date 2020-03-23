using CenterSpace.NMath.Core;
using MathNet.Numerics;//要裝，自己去youtube看怎麼裝
using OpenCvSharp;
using OpenCvSharp.Extensions;         
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

//using System.Windows.Forms.DataVisualization.Charting;

namespace SpectroChipApp
{

    public partial class Form1 : Form
    {
        // Create class-level accesible variables  取roi用
        private VideoCapture capture;

        private Mat frame;
        private Bitmap image;
        private Bitmap image_roi;//我哪邊都能用，超爽ㄉ//E04以後不可以再這樣搞
        private Bitmap image_roi_for_gray;
        private Bitmap image_roi_for_cali;
        public int x = 0;
        public int y = 200;
        public int w = 640;
        public int h = 30;
        public int X_Start = 0;
        public int Y_Start = 0;
        public double Exp = 1511;
        public double Ag = 0;
        public double Dg = 32;

        private bool isgoUpMouseDown = false;
        private bool isgoDownMouseDown = false;
        private bool isgoLeftMouseDown = false;
        private bool isgoRightMouseDown = false;

        private double[] parameter_buffer = new double[] { 0, 0, 0, 0, 0 };

        public int btnStart_Visible_flag = 1; //開始時btnStart是可見的

        private Thread camera;
        private bool isCameraRunning = false;

        private bool mouseIsDown = false;
        private Rectangle mouseRect = Rectangle.Empty;

        // SG Fitting
        public int left_length = 10;

        public int right_length = 10;
        public int Polynomial = 3;

        //波長校正用
        // private double[] xPixel_array=new double[10];
        //private double[] yWave_array = new double[10];

        //灰階用

        private void CaptureCamera()
        {
            camera = new Thread(new ThreadStart(CaptureCameraCallback));
            camera.Start();
            Thread.Sleep(100);
            //Exp = capture.Get(15);
            //EXPtextBox.Text = Exp.ToString();
            //btnStart.Visible = true;
        }

        private void CaptureCameraCallback()
        {
            // Thread.Sleep(100);//加了比較順
            frame = new Mat();

            capture = new VideoCapture(0);

            capture.FrameHeight = 1280;
            capture.FrameWidth = 1920;
            
            

            //Console.WriteLine(capture.FrameHeight);
            //Console.WriteLine(capture.FrameWidth);

            capture.Open(0);

            if (capture.IsOpened())
            {
                while (isCameraRunning)
                {
                    try
                    {
                        capture.Read(frame);

                       

                        Rect roi = new Rect(x, y, w, h);//首先要用个rect确定我们的兴趣区域在哪

                        Mat ImageROI = new Mat(frame, roi);//新建一个mat，把roi内的图像加载到里面去。

                        image = BitmapConverter.ToBitmap(frame);
                        image_roi = BitmapConverter.ToBitmap(ImageROI);
                        image_roi_for_gray = BitmapConverter.ToBitmap(ImageROI);
                        image_roi_for_cali = BitmapConverter.ToBitmap(ImageROI);
                        //Cv2.ImShow("兴趣区域", ImageROI);
                        // Cv2.ImShow("滚滚", image);
                    }
                    catch (InvalidCastException e)
                    {
                    }

                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                    }
                    pictureBox1.Image = image;

                    if (pictureBox2.Image != null)
                    {
                        pictureBox2.Image.Dispose();
                    }

                    pictureBox2.Image = image_roi;//最後把roi顯示在 pictureBox2的地方

                    this.Invoke(new Action(() =>
                    {
                        displayRoiSensorView(image_roi_for_gray);
                        //DisplayRoiCalibratedView(image_roi_for_cali);
                    }));

                    Thread.Sleep(300);//加了比較順

                    //下面這樣太吃記憶體
                    /*RectangleF cloneRect = new RectangleF(x, y, w, h);
                   System.Drawing.Imaging.PixelFormat format = image_roi.PixelFormat;
                   Bitmap image_roi_for_gray = image_roi.Clone(cloneRect, format);*/
                }
            }
            btnStart_Visible_flag = 1;
        }

        //---------------------------------宜運函數(首)---------------------

        private void displayRoiSensorView(Bitmap input_image)//育代
        {
            //this.chart2.Series.Clear();

            int W = input_image.Width, H = input_image.Height;
            //Bitmap image_roi_for_gray = new Bitmap(w, h);

            // Bitmap im1 = new Bitmap(w, h);//讀出原圖X軸 pixel
            Bitmap im1 = new Bitmap(W, H);//讀出原圖X軸 pixel
            int Pixel_x = 0;//正在被掃描的點
            int Pixel_y = 0;
            double[] ARed = new double[W];
            double[] AGreen = new double[W];
            double[] ABlue = new double[W];
            double[] AGray = new double[W];
            double[] IntensityRed = new double[W];
            double[] IntensityGreen = new double[W];
            double[] IntensityBlue = new double[W];
            double[] IntensityGray = new double[W];

            var sgf = new SavitzkyGolayFilter(left_length, right_length, Polynomial);

            System.Windows.Forms.DataVisualization.Charting.Series seriesRed = new System.Windows.Forms.DataVisualization.Charting.Series("紅色", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesGreen = new System.Windows.Forms.DataVisualization.Charting.Series("綠色", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesBlue = new System.Windows.Forms.DataVisualization.Charting.Series("藍色", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesGray = new System.Windows.Forms.DataVisualization.Charting.Series("灰階", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesClb = new System.Windows.Forms.DataVisualization.Charting.Series("波長校正", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesSG1 = new System.Windows.Forms.DataVisualization.Charting.Series("SG", 1000);
            //Console.WriteLine("W:"+W);

            /*
               for (k = 0; k < W; k++)
               {
                   //theWL = parameter_buffer[4] * (Math.Pow(k, 4)) + parameter_buffer[3] * (Math.Pow(k, 3)) + parameter_buffer[2] * (Math.Pow(k, 2)) + parameter_buffer[1] * k + parameter_buffer[0];
                   //seriesClb.Points.AddXY(Convert.ToDouble(k), theWL);
               }*/

            for (Pixel_x = 0; Pixel_x < W; Pixel_x++)
            {
                for (Pixel_y = 0; Pixel_y < H; Pixel_y++)
                {
                    //int xx = Pixel_x + 1;
                    //int yy = Pixel_y + 1;

                    //先把圖變灰階
                    Color p0 = input_image.GetPixel(Pixel_x, Pixel_y);//太快會閃退，全世界都在用image_roi
                    int R = p0.R, G = p0.G, B = p0.B;
                    int gray = (R * 313524 + G * 615514 + B * 119538) >> 20;
                    Color p1 = Color.FromArgb(gray, gray, gray);
                    //im1.SetPixel(i, j, p1);
                    ARed[Pixel_x] = ARed[Pixel_x] + R;
                    AGreen[Pixel_x] = AGreen[Pixel_x] + G;
                    ABlue[Pixel_x] = ABlue[Pixel_x] + B;
                    AGray[Pixel_x] = AGray[Pixel_x] + gray;
                }
                IntensityRed[Pixel_x] = ARed[Pixel_x] / H;//平均
                IntensityGreen[Pixel_x] = AGreen[Pixel_x] / H;//平均
                IntensityBlue[Pixel_x] = ABlue[Pixel_x] / H;//平均
                IntensityGray[Pixel_x] = AGray[Pixel_x] / H;//平均
            }
            var sg = new DoubleVector(IntensityGray);
            var IntensitySG1 = sgf.Filter(sg);
            var IntensitySG = IntensitySG1.ToArray();
            for (Pixel_x = 0; Pixel_x < W; Pixel_x++)
            {
                double WL_x = 0;
                double WL_x_max = 0;
                // int k = 0;

                WL_x_max = parameter_buffer[4] * (Math.Pow(W - 1, 4)) + parameter_buffer[3] * (Math.Pow(W - 1, 3)) + parameter_buffer[2] * (Math.Pow(W - 1, 2)) + parameter_buffer[1] * W - 1 + parameter_buffer[0];
                //this.chart3.BackColor = Color.Black;
                //this.chart1.ChartAreas[0].AxisX.Interval = 5;
                this.chart3.ChartAreas[0].AxisY.Minimum = 0;
                //this.chart3.ChartAreas[0].AxisX.Minimum = 0;
                // this.chart3.ChartAreas[0].AxisX.Maximum = Math.Ceiling(WL_x_max); //<一定要改<--

                //製作seriesClb
                seriesClb.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                seriesClb.Color = Color.Brown;

                WL_x = Math.Round(parameter_buffer[4] * (Math.Pow(Pixel_x, 4)) + parameter_buffer[3] * (Math.Pow(Pixel_x, 3)) + parameter_buffer[2] * (Math.Pow(Pixel_x, 2)) + parameter_buffer[1] * Pixel_x + parameter_buffer[0], 2);

                //設定座標大小
                this.chart2.ChartAreas[0].AxisY.Minimum = 0;
                this.chart2.ChartAreas[0].AxisX.Minimum = 0;
                this.chart2.ChartAreas[0].AxisX.Maximum = W;

                //設定標題

                this.chart2.Titles.Clear();
                this.chart2.Titles.Add("S01");
                this.chart2.Titles[0].Text = "Sensor View Point (Y軸灰度平均)";
                this.chart2.Titles[0].ForeColor = Color.Black;
                this.chart2.Titles[0].Font = new System.Drawing.Font("標楷體", 16F);

                //給入數據畫圖
                seriesRed.Color = Color.Red;
                seriesGreen.Color = Color.Green;
                seriesBlue.Color = Color.Blue;
                seriesGray.Color = Color.Gray;
                seriesSG1.Color = Color.Orange;

                seriesRed.Points.AddXY(Pixel_x, IntensityRed[Pixel_x]);
                seriesGreen.Points.AddXY(Pixel_x, IntensityGreen[Pixel_x]);
                seriesBlue.Points.AddXY(Pixel_x, IntensityBlue[Pixel_x]);
                seriesGray.Points.AddXY(Pixel_x, IntensityGray[Pixel_x]);
                seriesClb.Points.AddXY(WL_x, IntensitySG[Pixel_x]);
                seriesSG1.Points.AddXY(Pixel_x, IntensitySG[Pixel_x]);

                seriesRed.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                seriesGreen.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                seriesBlue.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                seriesGray.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                seriesSG1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

                this.chart2.Series.Clear();
                this.chart3.Series.Clear();
                this.chart3.Series.Add(seriesClb);

                if (checkBox1.Checked)
                    this.chart2.Series.Add(seriesRed);
                if (checkBox2.Checked)
                    this.chart2.Series.Add(seriesGreen);
                if (checkBox3.Checked)
                    this.chart2.Series.Add(seriesBlue);
                if (checkBox4.Checked)
                    this.chart2.Series.Add(seriesGray);
                if (checkBox5.Checked)
                    this.chart2.Series.Add(seriesSG1);
            }
            // im1.Save("gray.png");
            //pictureBox2.Refresh();
            //pictureBox2.Image = im1;
        }

        /*  private void DisplayRoiCalibratedView(Bitmap input_image)
          {
               int W = input_image.Width, H = input_image.Height;
              //Bitmap image_roi_for_gray = new Bitmap(w, h);

              // Bitmap im1 = new Bitmap(w, h);//讀出原圖X軸 pixel
              Bitmap im1 = new Bitmap(W, H);//讀出原圖X軸 pixel
              int Pixel_x = 0;//正在被掃描的點
              int Pixel_y = 0;

              double[] AClb = new double[W];

              double[] IntensityClb = new double[W];

              System.Windows.Forms.DataVisualization.Charting.Series seriesClb = new System.Windows.Forms.DataVisualization.Charting.Series("Clb", 1000);

              Console.WriteLine("W:" + W);
              for (Pixel_x = 0; Pixel_x < W; Pixel_x++)
              {
                  for (Pixel_y = 0; Pixel_y < H; Pixel_y++)
                  {
                      //int xx = Pixel_x + 1;
                      //int yy = Pixel_y + 1;

                      //先把圖變灰階
                      Color p0 = input_image.GetPixel(Pixel_x, Pixel_y);//太快會閃退，全世界都在用image_roi
                      int R = p0.R, G = p0.G, B = p0.B;
                      int gray = (R * 313524 + G * 615514 + B * 119538) >> 20;
                      Color p1 = Color.FromArgb(gray, gray, gray);
                      //im1.SetPixel(i, j, p1);

                      AClb[Pixel_x] = AClb[Pixel_x] + gray;
                  }

                  IntensityClb[Pixel_x] = AClb[Pixel_x] / H;//平均

                  //設定座標大小
                  this.chart3.ChartAreas[0].AxisY.Minimum = 0;
                  this.chart3.ChartAreas[0].AxisX.Minimum = 0;
                  this.chart3.ChartAreas[0].AxisX.Maximum = W;
                  //this.chart1.ChartAreas[0].AxisX.Interval = 5;

                  //設定標題

                  this.chart3.Titles.Clear();
                  this.chart3.Titles.Add("S03");
                  this.chart3.Titles[0].Text = "Calibrated View Point ";
                  this.chart3.Titles[0].ForeColor = Color.Black;
                  this.chart3.Titles[0].Font = new System.Drawing.Font("標楷體", 16F);

                  //給入數據畫圖

                  seriesClb.Color = Color.Brown;

                  seriesClb.Points.AddXY(Pixel_x, IntensityClb[Pixel_x]);

                  seriesClb.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

                  this.chart3.Series.Clear();

                      this.chart3.Series.Add(seriesClb);
              }
              im1.Save("Clbgray.png");
              //pictureBox2.Refresh();
              //pictureBox2.Image = im1;
          }*/

        private void Load_SensorView_Chart()
        {
            //標題
            this.chart2.Titles.Add("Sensor View Point (Y軸灰度平均)");
            this.chart2.Titles[0].ForeColor = Color.Black;
            this.chart2.Titles[0].Font = new System.Drawing.Font("標楷體", 16F);

            System.Windows.Forms.DataVisualization.Charting.Series seriesRed = new System.Windows.Forms.DataVisualization.Charting.Series("Red", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesGreen = new System.Windows.Forms.DataVisualization.Charting.Series("Green", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesBlue = new System.Windows.Forms.DataVisualization.Charting.Series("Blue", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesGray = new System.Windows.Forms.DataVisualization.Charting.Series("Gray", 1000);
            seriesRed.Points.AddXY(0, 0);
            seriesGreen.Points.AddXY(0, 0);
            seriesBlue.Points.AddXY(0, 0);
            seriesGray.Points.AddXY(0, 0);
            //this.chart2.Series.Add(seriesRed);
            //this.chart2.Series.Add(seriesGreen);
            // this.chart2.Series.Add(seriesBlue);
            // this.chart2.Series.Add(seriesGray);
            checkBox4.Checked = true;
        }

        private void Load_CalibratedView_Chart()
        {
            this.chart3.Titles.Add("Calibrated View Point");
            this.chart3.Titles[0].ForeColor = Color.Black;
            this.chart3.Titles[0].Font = new System.Drawing.Font("標楷體", 16F);
        }

        private void Load_Cali_Chart()
        {
            //標題
            this.chart1.Titles.Add("Curve Fitting Plot");

            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series("擬和函數", 1000);
            series1.Points.AddXY(0, 0);
        }

        private void Read_TextBox()
        {
            if (WtextBox.Text != "0" && HtextBox.Text != "0")
            {
                //throw new NotImplementedException();
                x = int.Parse(XtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);
                y = int.Parse(YtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);
                w = int.Parse(WtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);
                h = int.Parse(HtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);
                Exp = int.Parse(EXPtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);

                if (x + w > capture.FrameWidth)
                {
                    if (w > capture.FrameWidth)
                    {
                        w = capture.FrameWidth;
                        x = 0;
                    }
                    else
                    {
                        x = capture.FrameWidth - w;
                    }
                    this.XtextBox.Text = x.ToString();
                    this.WtextBox.Text = w.ToString();
                }

                if (y + h > capture.FrameHeight)
                {
                    if (h > capture.FrameHeight)
                    {
                        h = capture.FrameHeight;
                        y = 0;
                    }
                    else
                    {
                        y = capture.FrameHeight - h;
                    }
                    this.YtextBox.Text = y.ToString();
                    this.HtextBox.Text = h.ToString();
                }

                //this.WtextBox.Text = w.ToString();
                // this.HtextBox.Text = h.ToString();
            }
            else
            {
                XtextBox.Text = "0";
                YtextBox.Text = "200";
                WtextBox.Text = "640";
                HtextBox.Text = "30";
                x = 0;
                y = 200;
                w = 640;
                h = 30;
                isCameraRunning = false;
                MessageBox.Show("請選擇有效的ROI");
                isCameraRunning = true;
            }
        }

        /// <summary>
        /// 初始化选择框
        /// </summary>
        /// <param name="StartPoint"></param>
        private void DrawStart(System.Drawing.Point StartPoint)
        {
            this.Capture = true;
            //指定工作区为整个控件 之後要改成只有picturebox1
            Cursor.Clip = RectangleToScreen(new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
            mouseRect = new Rectangle(StartPoint.X, StartPoint.Y, 0, 0);
        }

        /// <summary>
        /// 在鼠标移动的时改变选择框的大小
        /// </summary>
        /// <param name="p">鼠标的位置</param>
        private void ResizeToRectangle(System.Drawing.Point p)
        {
            DrawRectangle();
            mouseRect.Width = p.X - mouseRect.Left;
            mouseRect.Height = p.Y - mouseRect.Top;
            DrawRectangle();
        }

        /// <summary>
        /// 绘制选择框
        /// </summary>
        private void DrawRectangle()
        {
            Rectangle rect = RectangleToScreen(mouseRect);
            ControlPaint.DrawReversibleFrame(rect, Color.Red, FrameStyle.Dashed);
        }

        //PaintEventArgs e
        private void DrawFixRectangle(PaintEventArgs e) //先用全域的xywh
        {
            // Create pen.
            Pen bluePen = new Pen(Color.Blue, 5);

            // Create rectangle.
            Rectangle rect = new Rectangle(x, y, w, h);

            // Draw rectangle to screen.
            e.Graphics.DrawRectangle(bluePen, rect);
        }

        //-----------------------------宜運函數(尾)--------------------------------------

        public Form1()
        {
            InitializeComponent();
            //goUp.FlatAppearance.BorderSize = 0;
        }

        private void Picturebox1_MouseUp(object sender, MouseEventArgs e)
        {
            Capture = false;
            Cursor.Clip = Rectangle.Empty;
            mouseIsDown = false;
            DrawRectangle();
            mouseRect = Rectangle.Empty;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Read_TextBox();
            btnStart.Text = "▶";
            //Console.WriteLine(mouseIsDown);
            Load_Cali_Chart();
            Load_SensorView_Chart();
            Load_CalibratedView_Chart();
            updn_power.Maximum = 4;
            x = 0;
            y = 200;
            w = 640;
            h = 30;
            //goUp.FlatAppearance.BorderSize = 0;
            //  this.goUp.SendToBack();//将背景图片放到最下面
            //this.panel1.BackColor = Color.Transparent;//将Panel设为透明
            //this.panel1.Parent = this.Fine_pic;//将panel父控件设为背景图片控件
            //this.panel1.BringToFront();//将panel放在前面
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (btnStart.Text.Equals("▶"))
            {
                //btnStart_Visible_flag = 0;
                //Read_TextBox();
                CaptureCamera();
                btnStart.Text = "| |";
                isCameraRunning = true;
                btnStart.Visible = false;
                Thread.Sleep(4000);
                btnStart.Visible = true;
                //---參數調整(首)----
                Exp = capture.Get(15);
                EXPtextBox.Text = Exp.ToString();
                Dg = capture.Get(32);
                DGtextBox.Text = Dg.ToString();
                Ag = capture.Get(14);
                AGtextBox.Text = Ag.ToString();
            }
            else
            {
                capture.Release();

                btnStart.Text = "▶";
                isCameraRunning = false;
                btnStart.Visible = false;
                Thread.Sleep(4000);
                btnStart.Visible = true;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Read_TextBox();
            //Console.WriteLine(mouseIsDown);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Capture = false;
            Cursor.Clip = Rectangle.Empty;
            mouseIsDown = false;
            DrawRectangle();
            mouseRect = Rectangle.Empty;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseIsDown)
                ResizeToRectangle(e.Location);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (mouseIsDown == false)//點第一下
            {
                DrawStart(e.Location);

                if (e.X < 30)//X的右邊判斷跟照片大小FrameWidth有關 等SUNPLUS
                {
                    X_Start = 0;
                }
                else
                {
                    X_Start = e.X;
                }

                Y_Start = e.Y;

                XtextBox.Text = Convert.ToString(X_Start);
                YtextBox.Text = Convert.ToString(Y_Start);

                mouseIsDown = true;
            }
            else//點第二下
            {
                Capture = false;

                WtextBox.Text = Convert.ToString(Math.Abs(e.X - X_Start));
                HtextBox.Text = Convert.ToString(Math.Abs(e.Y - Y_Start));

                if (e.X >= X_Start && e.Y >= Y_Start)//左上->右下
                {
                }
                if (e.X >= X_Start && e.Y <= Y_Start)//
                {
                    YtextBox.Text = Convert.ToString(e.Y);
                }
                if (e.X <= X_Start && e.Y >= Y_Start)//不正常情況
                {
                    XtextBox.Text = Convert.ToString(e.X);
                }
                if (e.X <= X_Start && e.Y <= Y_Start)//不正常情況
                {
                    XtextBox.Text = Convert.ToString(e.X);
                    YtextBox.Text = Convert.ToString(e.Y);
                }

                Cursor.Clip = Rectangle.Empty;
                mouseIsDown = false;
                DrawRectangle();
                //mouseRect = Rectangle.Empty;
                Read_TextBox();
                //DrawFixRectangle();
            }
            //Console.WriteLine(mouseIsDown);
        }

        private void btn_cali_Click(object sender, EventArgs e)//開始波長校正
        {
            // xPixel_array = new double[];
            //yWave_array = new double[];
            List<double> xPixel_List = new List<double>();
            List<double> yWave_List = new List<double>();
            int point_used = 0;
            double[] xPixel_array;
            double[] yWave_array;
            //double[] a = new double[10];
            //double[] b = new double[10];
            bool input_error = true;
            double result = 0;
            double[] parameter = new double[4];
            decimal power;
            //Cali-0 冪
            power = updn_power.Value;

            //Cali-1輸入像素-波長
            if (Double.TryParse(p1.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w1.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p2.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w2.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p3.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w3.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p4.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w4.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }

                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p5.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w5.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p6.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w6.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p7.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w7.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p8.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w8.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p9.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w9.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }

            if (Double.TryParse(p10.Text, out result))
            {
                point_used++;
                xPixel_List.Add(result);
                if (Double.TryParse(w10.Text, out result))
                {
                    yWave_List.Add(result);
                }
                else { Console.WriteLine("錯誤，請輸入第" + point_used + "點的波長值(y座標)"); input_error = false; }
                // Console.WriteLine("有進"+result);
            }
            xPixel_array = xPixel_List.ToArray();
            yWave_array = yWave_List.ToArray();

            //Console.WriteLine("List狀態" + xPixel_array + "\n"+ yWave_array + "\n" + point_used);

            //Cali1-1.5 錯誤總結
            if (point_used < 4)
            {
                input_error = false;
            }

            /*
            if self.P1.text():
            a1 = float(self.P1.text())
            pixel_array.append(a1)
            #print("a1=",a1)
            if self.W1.text():
                b1 = float(self.W1.text())
                wave_array.append(b1)
                #print("b1=",b1)

            xPixel_List.Add(value);
            */
            ////Cali-2做線性擬和
            if (input_error)
            {
                parameter = Fit.Polynomial(xPixel_array, yWave_array, Decimal.ToInt32(power));
                //Draw_Cali();
                //Show Parameter
                // Console.WriteLine(parameter[0].ToString()+"\n"+ parameter[1].ToString() + "\n" + parameter[2].ToString() + "\n" + parameter[3].ToString());
            }
            else
            {
                MessageBox.Show("格式錯誤");
            }
            ///Cali-3畫圖囉
            int[,] array = new int[,] {
            {1,8,9,7,105,11,50,999,500,1},
            {12,15,11,18,733,5,4,3,2,500} };

            //標題 最大數值
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series("第一條線", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series("第二條線", 1000);

            //設定線條顏色
            series1.Color = Color.Blue;
            series2.Color = Color.Red;

            //設定字型
            series1.Font = new System.Drawing.Font("新細明體", 14);
            //series2.Font = new System.Drawing.Font("標楷體", 12);

            //折線圖
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

            //將數值顯示在線上
            series1.IsValueShownAsLabel = true;
            series2.IsValueShownAsLabel = false;

            //將數值新增至序列
            //1.點
            for (int index = 0; index < point_used; index++)
            {
                series1.Points.AddXY(xPixel_array[index], yWave_array[index]);
                // series2.
            }
            //2.擬和函數
            double equationVar;
            double[] para_buf = new double[] { 0, 0, 0, 0, 0 };

            for (int i = 0; i <= power; i++) //要改成power
            {
                para_buf[i] = parameter[i];
                parameter_buffer[i] = parameter[i];
            }

            for (int x = -500; x < 1000; x++)  //hardcoding
            {
                equationVar = para_buf[4] * (Math.Pow(x, 4)) + para_buf[3] * (Math.Pow(x, 3)) + para_buf[2] * (Math.Pow(x, 2)) + para_buf[1] * x + para_buf[0];
                series2.Points.AddXY(Convert.ToDouble(x), equationVar);
            }

            //將序列新增到圖上
            this.chart1.Series.Clear();
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            /*
            //製作chart3所需的
            for (int i = 0; i < w; i++)  //先用
            {
                waveLength[i] = para_buf[4] * (Math.Pow(i, 4)) + para_buf[3] * (Math.Pow(i, 3)) + para_buf[2] * (Math.Pow(i, 2)) + para_buf[1] * i + para_buf[0];
                //series2.Points.AddXY(Convert.ToDouble(i), equationVar);
            }*/
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnStart.Text.Equals("▶"))
            {
                isCameraRunning = false;
                capture.Release();

                btnStart.Text = "▶";
                DialogResult dialog = MessageBox.Show("你不能在影像串流進行時關閉程式\n確定強制關閉?", "警告!", MessageBoxButtons.OKCancel);

                if (dialog == DialogResult.OK)
                {
                    Thread.Sleep(1000);
                    Application.Exit();
                }
                else if (dialog == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawFixRectangle(e);
        }

        private void goUp_MouseDown(object sender, MouseEventArgs e)
        {
            //Fine_pic.BackgroundImage = imageList1.Images[0];
            isgoUpMouseDown = true;    //啟用按下標識

            try
            {
                // y = Convert.ToDouble(txtSetValue.Text);   //獲取txtSetValue的初始值
                Read_TextBox();
            }
            catch (Exception)
            {
                y = 0;
            }

            Action handler = new Action(this.roiUp);     //定義委託
            handler.BeginInvoke(null, null);           //非同步呼叫
        }

        private void goUp_MouseUp(object sender, MouseEventArgs e)
        {
            isgoUpMouseDown = false;
        }

        private void goDown_MouseDown(object sender, MouseEventArgs e)
        {
            isgoDownMouseDown = true;    //啟用按下標識

            try
            {
                // y = Convert.ToDouble(txtSetValue.Text);   //獲取txtSetValue的初始值
                Read_TextBox();
            }
            catch (Exception)
            {
                y = 0;
            }

            Action handler = new Action(this.roiDown);     //定義委託
            handler.BeginInvoke(null, null);           //非同步呼叫
        }

        private void goDown_MouseUp(object sender, MouseEventArgs e)
        {
            isgoDownMouseDown = false;
        }

        private void goLeft_MouseDown(object sender, MouseEventArgs e)
        {
            isgoLeftMouseDown = true;    //啟用按下標識

            try
            {
                // y = Convert.ToDouble(txtSetValue.Text);   //獲取txtSetValue的初始值
                Read_TextBox();
            }
            catch (Exception)
            {
                y = 0;
            }

            Action handler = new Action(this.roiLeft);     //定義委託
            handler.BeginInvoke(null, null);           //非同步呼叫
        }

        private void goLeft_MouseUp(object sender, MouseEventArgs e)
        {
            isgoLeftMouseDown = false;
        }

        private void goRight_MouseDown(object sender, MouseEventArgs e)
        {
            isgoRightMouseDown = true;    //啟用按下標識

            try
            {
                // y = Convert.ToDouble(txtSetValue.Text);   //獲取txtSetValue的初始值
                Read_TextBox();
            }
            catch (Exception)
            {
                y = 0;
            }

            Action handler = new Action(this.roiRight);     //定義委託
            handler.BeginInvoke(null, null);           //非同步呼叫
        }

        private void goRight_MouseUp(object sender, MouseEventArgs e)
        {
            isgoRightMouseDown = false;
        }

        //---
        private void roiUp()
        {
            while (isgoUpMouseDown)
            {
                //capture.FrameHeight = 1280;
                //capture.FrameWidth = 1920;
                if (y <= -1 || y >= capture.FrameHeight + h)
                {
                    this.Invoke(new Action(() => MessageBox.Show("超出設定值！", "警告")));
                    y = 0;
                    break;
                }
                else
                {
                    y -= 1;   //計算：每次累加的單位，如果要累加的精度大點，該值設定大一些
                    if (y < 0) y = 0;
                    this.Invoke(new Action(() => this.YtextBox.Text = y.ToString()));  //介面顯示
                    System.Threading.Thread.Sleep(100);    //如果要速度塊，將這個值修改小點
                }
            }
        }

        private void roiDown()
        {
            while (isgoDownMouseDown)
            {
                //capture.FrameHeight = 1280;
                //capture.FrameWidth = 1920;
                if (y <= -1 || y + h >= capture.FrameHeight)
                {
                    //this.Invoke(new Action(() => MessageBox.Show("超出設定值！", "警告")));
                    // y = 0;
                    break;
                }
                else
                {
                    y += 1;   //計算：每次累加的單位，如果要累加的精度大點，該值設定大一些
                    if (y + h > capture.FrameHeight) { y = capture.FrameHeight - h - 1; }
                    this.Invoke(new Action(() => this.YtextBox.Text = y.ToString()));  //介面顯示
                    System.Threading.Thread.Sleep(100);    //如果要速度塊，將這個值修改小點
                }
            }
        }

        private void roiLeft()
        {
            while (isgoLeftMouseDown)
            {
                //capture.FrameHeight = 1280;
                //capture.FrameWidth = 1920;
                if (x <= -1 || x + w >= capture.FrameWidth)
                {
                    this.Invoke(new Action(() => MessageBox.Show("超出設定值！", "警告")));
                    x = 0;
                    break;
                }
                else
                {
                    x -= 1;   //計算：每次累加的單位，如果要累加的精度大點，該值設定大一些
                    if (x < 0) x = 0;
                    this.Invoke(new Action(() => this.XtextBox.Text = x.ToString()));  //介面顯示
                    System.Threading.Thread.Sleep(100);    //如果要速度塊，將這個值修改小點
                }
            }
        }

        private void roiRight()
        {
            while (isgoRightMouseDown)
            {
                //capture.FrameHeight = 1280;
                //capture.FrameWidth = 1920;
                if (x <= -1 || x >= capture.FrameWidth + w)
                {
                    this.Invoke(new Action(() => MessageBox.Show("超出設定值！", "警告")));
                    x = 0;
                    break;
                }
                else
                {
                    x += 1;   //計算：每次累加的單位，如果要累加的精度大點，該值設定大一些

                    if (x + w >= capture.FrameWidth) x = capture.FrameWidth - w - 1;
                    this.Invoke(new Action(() => this.XtextBox.Text = x.ToString()));  //介面顯示
                    System.Threading.Thread.Sleep(100);    //如果要速度塊，將這個值修改小點
                }
            }
        }

        private void XtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void YtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void WtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void HtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {

         
                Exp = Convert.ToDouble(trackBar1.Value); //範圍目前直接寫死
            //int.Parse(XtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);
            if (isCameraRunning)
            {
                capture.Set(15, Exp);
            }
            EXPtextBox.Text = Exp.ToString();
            
            
        }
        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            Dg = Convert.ToDouble(trackBar2.Value); //範圍目前直接寫死
            //int.Parse(XtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);
            if (isCameraRunning)
            {
                capture.Set(32, Dg); 
            }
            DGtextBox.Text = Dg.ToString();
        }
        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            Ag = Convert.ToDouble(trackBar3.Value); //範圍目前直接寫死
            //int.Parse(XtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);
            if (isCameraRunning)
            {
                capture.Set(14, Ag);
            }
            AGtextBox.Text = Ag.ToString();
        }
        private void RESETbutton_Click(object sender, EventArgs e)
        {
            Exp = -2;
            Dg = 0;
            Ag = 32; //範圍目前直接寫死
            //int.Parse(XtextBox.Text, CultureInfo.InvariantCulture.NumberFormat);
            if (isCameraRunning)
            {
                capture.Set(15, Exp);
                capture.Set(32, Dg);
                capture.Set(14, Ag);
            }
            EXPtextBox.Text = Exp.ToString();
            DGtextBox.Text = Dg.ToString();
            AGtextBox.Text = Ag.ToString();
            trackBar1.Value = Convert.ToInt32(Exp);
            trackBar2.Value = Convert.ToInt32(Dg);
            trackBar3.Value = Convert.ToInt32(Ag);
        }

        private void EXPtextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void FPSlabel_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Click(object sender, EventArgs e)
        {

        }


    }
}
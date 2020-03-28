using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpectroChipApp
{
    
    public partial class Form2 : Form
    {
        private Form1 f1;
        public Form2(Form1 form)
        {
            InitializeComponent();
           f1 = form;

           


        }

        //---宜運函數==
        private void display(double[] SG, int start_pixel, int end_pixel,int roi_start_x, int GauOrder)//
        {
            //製造SG_Clip
            double[] SG_Clip = new double[end_pixel - start_pixel];
            //鋪0
            for (int Pixel_x = 0; Pixel_x < end_pixel- start_pixel; Pixel_x++)
            {
                SG_Clip[Pixel_x] = 0;
            }
         //   foreach (int indexs in SG_Clip) { indexs = 0; }

          for (int Pixel_x = 0; Pixel_x < end_pixel - start_pixel; Pixel_x++)
            {
                SG_Clip[Pixel_x] = SG[Pixel_x + start_pixel-roi_start_x];
            }





            //-----------------------畫圖

            System.Windows.Forms.DataVisualization.Charting.Series seriesSG1 = new System.Windows.Forms.DataVisualization.Charting.Series("SG", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesGau = new System.Windows.Forms.DataVisualization.Charting.Series("高斯", 1000);
            //Console.WriteLine("W:"+W);

            /*
               for (k = 0; k < W; k++)
               {
                   //theWL = parameter_buffer[4] * (Math.Pow(k, 4)) + parameter_buffer[3] * (Math.Pow(k, 3)) + parameter_buffer[2] * (Math.Pow(k, 2)) + parameter_buffer[1] * k + parameter_buffer[0];
                   //seriesClb.Points.AddXY(Convert.ToDouble(k), theWL);
               }*/
            //迴圈一: 畫上SG後的圖
            for (int Pixel_x = 0; Pixel_x < SG_Clip.Length; Pixel_x++)
            {


                //設定座標大小
                this.chart4.ChartAreas[0].AxisY.Minimum = 0;
                this.chart4.ChartAreas[0].AxisY.Maximum = 300;
                this.chart4.ChartAreas[0].AxisX.Minimum = start_pixel;
                this.chart4.ChartAreas[0].AxisX.Maximum = end_pixel;



                //設定標題

                this.chart4.Titles.Clear();
                this.chart4.Titles.Add("S01");
                this.chart4.Titles[0].Text = "Sensor View Point (Y軸灰度平均)";
                this.chart4.Titles[0].ForeColor = Color.Black;
                this.chart4.Titles[0].Font = new System.Drawing.Font("標楷體", 16F);

                //給入數據畫圖

                seriesSG1.Color = Color.Orange;

                // seriesSG1.Points.AddXY(Pixel_x+start_pixel, SG[Pixel_x + start_pixel]);
              // seriesSG1.Points.AddXY(Pixel_x + start_pixel- roi_start_x, SG_Clip[Pixel_x]);
                seriesSG1.Points.AddXY(Pixel_x+ start_pixel, SG_Clip[Pixel_x]);

                seriesSG1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                seriesGau.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

                this.chart4.Series.Clear();

                this.chart4.Series.Add(seriesSG1);
                this.chart4.Series.Add(seriesGau);
            }
        }

            //---宜運函數(尾)-----------



            private void button1_Click(object sender, EventArgs e)
        {
            //Form1 f1 = Form();//產生Form2的物件，才可以使用它所提供的Method
            this.Close();
            f1.Enabled = true;//將Form1隱藏。由於在Form1的程式碼內使用this，所以this為Form1的物件本身
                              //f.Visible = true;//顯示第二個視窗
            f1.isForm2Running = false;
            f1.isPixelClick = false;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            display(f1.IntensitySG,f1.X_Start_chart, Convert.ToInt32(f1.chart2.ChartAreas[0].CursorX.SelectionEnd), f1.X_Start, 0);
           // display(f1.IntensitySG, f1.X_Start_chart, f1.X_End_chart, f1.X_Start, 0);
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Form1 f1 = Form();//產生Form2的物件，才可以使用它所提供的Method
         
            f1.Enabled = true;//將Form1隱藏。由於在Form1的程式碼內使用this，所以this為Form1的物件本身
                              //f.Visible = true;//顯示第二個視窗
            f1.isForm2Running = false;
            f1.isPixelClick = false;
        }
    }
    
}

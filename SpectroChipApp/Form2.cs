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

            if (end_pixel - start_pixel < 0) {
               int temp= end_pixel;
                end_pixel = start_pixel;
                start_pixel = temp;

            }
            
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


            double[] IntensityGau;
            double Pixel_Max;
            double Intensity_Max;
            f1.Gaussian(SG_Clip, SG_Clip.Length, GauOrder, out IntensityGau, out Pixel_Max, out Intensity_Max);

            textBox1.Text = (Pixel_Max+start_pixel).ToString();
        
            textBox2.Text = Intensity_Max.ToString();

            //var IntensityGau = f1.Gaussian(SG_Clip, SG_Clip.Length, GauOrder);
            //-----------------------畫圖

            System.Windows.Forms.DataVisualization.Charting.Series seriesSG1 = new System.Windows.Forms.DataVisualization.Charting.Series("SG", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series seriesGau = new System.Windows.Forms.DataVisualization.Charting.Series("高斯", 1000);
            System.Windows.Forms.DataVisualization.Charting.Series Max_Point = new System.Windows.Forms.DataVisualization.Charting.Series("極值", 1);
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

                seriesSG1.Color = Color.Cyan;
                seriesSG1.BorderWidth = 5;
                seriesGau.Color = Color.Red;
                Max_Point.Color = Color.Blue;
                // seriesSG1.Points.AddXY(Pixel_x+start_pixel, SG[Pixel_x + start_pixel]);
                // seriesSG1.Points.AddXY(Pixel_x + start_pixel- roi_start_x, SG_Clip[Pixel_x]);
                seriesSG1.Points.AddXY(Pixel_x+ start_pixel, SG_Clip[Pixel_x]);
                seriesGau.Points.AddXY(Pixel_x + start_pixel, IntensityGau[Pixel_x]);
               
                //Max_Point.BorderWidth = 20;
                


                


               
            }
            Max_Point.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            seriesSG1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            seriesGau.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            this.chart4.Series.Clear();

            Max_Point.Points.AddXY(Pixel_Max + start_pixel, Intensity_Max);

            Max_Point.IsValueShownAsLabel = true;
            //Max_Point.font = "20";
            //Max_Point.AxisLabel = (Pixel_Max + start_pixel).ToString() + "," + Intensity_Max.ToString();
            Max_Point.Label ="("+ (Pixel_Max + start_pixel).ToString() + "," + Intensity_Max.ToString()+")";

            this.chart4.Series.Add(seriesSG1);
            this.chart4.Series.Add(seriesGau);     
            this.chart4.Series.Add(Max_Point);
        }

            //---宜運函數(尾)-----------



            private void button1_Click(object sender, EventArgs e)
        {
            //Form1 f1 = Form();//產生Form2的物件，才可以使用它所提供的Method
            //f1.Max_From_f2 = textBox1.Text;
           // f1.p3.Text = textBox1.Text;
            switch (f1.Selected_P)
            {
                case 1:
                    f1.p1.Text = textBox1.Text;
                    break;
                case 2:
                    f1.p2.Text = textBox1.Text;
                    break;
                case 3:
                    f1.p3.Text = textBox1.Text;
                    break;
                case 4:
                    f1.p4.Text = textBox1.Text;
                    break;
                case 5:
                    f1.p5.Text = textBox1.Text;
                    break;
                case 6:
                    f1.p6.Text = textBox1.Text;
                    break;
                case 7:
                    f1.p7.Text = textBox1.Text;
                    break;
                case 8:
                    f1.p8.Text = textBox1.Text;
                    break;
                case 9:
                    f1.p9.Text = textBox1.Text;
                    break;
                case 10:
                    f1.p10.Text = textBox1.Text;
                    break;
                default:
                    Console.WriteLine("nothing change，都好像沒有變");
                    break;
            }




            f1.Enabled = true;//將Form1隱藏。由於在Form1的程式碼內使用this，所以this為Form1的物件本身
                              //f.Visible = true;//顯示第二個視窗
            f1.isForm2Running = false;
            f1.isPixelClick = false;
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            display(f1.IntensitySG,f1.X_Start_chart, Convert.ToInt32(f1.chart2.ChartAreas[0].CursorX.SelectionEnd), f1.x, 3);
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

        private void updn_power_ValueChanged(object sender, EventArgs e)
        {
            display(f1.IntensitySG, f1.X_Start_chart, Convert.ToInt32(f1.chart2.ChartAreas[0].CursorX.SelectionEnd), f1.x, Convert.ToInt32(updn_power.Value));
        }
    }
    
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpectroChipApp
{
    public partial class Form3 : Form
    {
      
        


        public Form3(Form1 form1)
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
           richTextBox1.Text =  "尚未連接COM\r\n";
          SerialPort serialPort1 = new SerialPort();
           String[] portnames = SerialPort.GetPortNames();
            foreach (var item in portnames)
            {
                comboBox1.Items.Add(item);
            }
        }

        //宜運-----------------------
        public void opencom()
        {
            try
            {
                //波特率
                serialPort1.BaudRate = 9600;
                //資料位
                serialPort1.DataBits = 8;
                serialPort1.PortName = comboBox1.Text;
                //兩個停止位
                serialPort1.StopBits = System.IO.Ports.StopBits.One;
                //無奇偶校驗位
                serialPort1.Parity = System.IO.Ports.Parity.None;
                serialPort1.ReadTimeout = 100;
                serialPort1.Open();
                if (!serialPort1.IsOpen)
                {
                    MessageBox.Show("埠開啟失敗");
                    return;
                }
                else
                {
                    richTextBox1.AppendText("埠" + comboBox1.Text + "開啟成功\r\n");
                }
                serialPort1.DataReceived += serialPort1_DataReceived;
            }
            catch (Exception ex)
            {
                serialPort1.Dispose();
                richTextBox1.AppendText(ex.Message+"\r\n");
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(50);  //（毫秒）等待一定時間，確保資料的完整性 int len        
            int len = serialPort1.BytesToRead;
            string receivedata = string.Empty;
            byte[] array = System.Text.Encoding.ASCII.GetBytes(receivedata);

            if (len != 0)
            {
                byte[] buff = new byte[len];
                serialPort1.Read(buff, 0, len);
                receivedata = Encoding.Default.GetString(buff);

            }

      //      if (Int32.Parse(receivedata) < 91 & Int32.Parse(receivedata) > 40)
      //      {
                this.Invoke(new Action(() =>
                {
                    //richTextBox1.AppendText("收到來自Arduino的訊息:" + receivedata + "\r\n");
                    richTextBox1.AppendText( receivedata );
                }));
       //     }

          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textBox2.Text);
            }
        }

        private void Connectbutton_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                opencom();
                Connectbutton.Text = "斷開";
            }
            else
            {
                serialPort1.Dispose();
                richTextBox1.AppendText("埠" + comboBox1.Text + "已關閉\r\n");
                Connectbutton.Text = "連接";
            }
        }

        //宜運尾-------------------------------


    }
}

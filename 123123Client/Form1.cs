using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace _123123Client
{
    public partial class Form1 : Form
    {

        private TcpClient client;
        private NetworkStream stream;
        private string localIp;
        private string remoteIp;

        public Form1()
        {
            InitializeComponent();
            
        }


        private void button1_Click(object sender, EventArgs e)
        {
            localIp = textBox3.Text;
            remoteIp = textBox4.Text;
            client = new TcpClient(remoteIp, 12345);
            stream = client.GetStream();

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }
        private void ReceiveMessages()
        {
            while (client.Connected)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        Invoke((MethodInvoker)delegate
                        {
                            textBox2.Text += $"{message}" + Environment.NewLine;
                        });
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Произошла ошибка при чтении данных: {ex.Message}");
                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string message = textBox1.Text;
                if (client.Connected)
                {
                    byte[] mes = Encoding.UTF8.GetBytes($"{localIp}:{message}");
                    stream.Write(mes, 0, mes.Length);
                    textBox2.Text += $"{localIp}: {message}" + Environment.NewLine;
                }
                else
                {
                    MessageBox.Show("Ошибка: Соединение разорвано");
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Произошла ошибка при записи данных: {ex.Message}");
            }
        }

       

        
    }
}

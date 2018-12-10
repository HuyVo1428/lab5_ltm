using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Client
{
    public partial class Client : Form
    {
        TcpClient client;
        NetworkStream networkStream;
        private int port;
        private string hostname;

        public Client(string hostname = "127.0.0.1", string port = "8080")
        {
            InitializeComponent();

            this.port = int.Parse(port);
            this.hostname = hostname;
            this.client = new TcpClient(this.hostname, this.port);
            this.networkStream = client.GetStream();
        }

        private void btnRead_Click(object sender, System.EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();

            string path = ofd.FileName;
            int totalLine = countLine(path);

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(fs);

            SendMessage("tt" + totalLine.ToString());
            int line = 0;
            do
            {
                line++;
                string data1 = sr.ReadLine();
                string data2 = "ol" + line.ToString() + "h" + data1;
                SendMessage(data2);

            } while (sr.Peek() != -1);
            fs.Close();
            MessageBox.Show("Completed!");
        }

        private int countLine(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            int count = 0;
            while (sr.ReadLine() != null)
            {
                count++;
            }
            fs.Close();
            return count;
        }


        private void SendMessage(string message)
        {
            Byte[] data = new byte[1024];
            Byte[] temp = System.Text.Encoding.UTF8.GetBytes(message + "\n");
            temp.CopyTo(data, 0);
            this.networkStream.Write(data, 0, data.Length);
            Thread.Sleep(1000);
        }

        void CloseForm(object sender, FormClosingEventArgs e)
        {
            Byte[] data = System.Text.Encoding.UTF8.GetBytes("quit\n");
            this.networkStream.Write(data, 0, data.Length);
            this.networkStream.Close();
            this.client.Close();
        }
    }
}

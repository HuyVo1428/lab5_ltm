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

namespace Server
{
    public struct Client
    {
        public bool canHandle;
        public Socket socket;
        public Thread thread;
    }

    public partial class Server : Form
    {

        private Thread serverThread;
        private Socket listenerSocket;
        private Client[] clients;
        private int totalSocket;
        private IPAddress ipAddress;
        private int port;

        public Server(string hostname = "127.0.0.1", string port = "8080", string totalSocket = "5")
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            this.totalSocket = int.Parse(totalSocket);
            this.port = int.Parse(port);
            this.ipAddress = IPAddress.Parse(hostname);
            this.clients = new Client[this.totalSocket];
        }

        private void StartListen(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            this.TryCancelThread();
            this.serverThread = new Thread(new ThreadStart(this.StartUnsafeThread));
            this.serverThread.Start();
        }

        void TryCancelThread()
        {
            try
            {
                if (this.serverThread.IsAlive)
                {
                    this.serverThread.Abort();
                }
                foreach (Client c in this.clients)
                {
                    if (c.thread.IsAlive)
                    {
                        c.thread.Abort();
                    }
                }
            }
            catch { }
        }

        private void FormClosingEvent(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.listenerSocket.Close();
            }
            catch { }
            this.TryCancelThread();
        }

        void StartUnsafeThread()
        {
            this.listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipepServer = new IPEndPoint(this.ipAddress, this.port);
            listenerSocket.Bind(ipepServer);
            listenerSocket.Listen(-1);
            for (int i = 0; i < this.clients.Length; i++)
            {
                this.clients[i].canHandle = true;
            }

            while (true)
            {
                for (int i = 0; i < this.clients.Length; i++)
                {
                    if (this.clients[i].canHandle)
                    {
                        this.clients[i].socket = this.listenerSocket.Accept();
                        int index = i;
                        this.clients[i].thread = new Thread(() => this.CommunicateClient(index));
                        this.clients[i].thread.Start();
                    }
                }
            }
        }

        private void CommunicateClient(int index)
        {
            int bytesReceived = 0;
            byte[] recv = new byte[1024];
            this.clients[index].canHandle = false;
            while (this.clients[index].socket.Connected)
            {
                string packet = "";
                bytesReceived = this.clients[index].socket.Receive(recv);
                packet = System.Text.Encoding.UTF8.GetString(recv);

                if (packet.Trim().ToLower() == "quit\n")
                {
                    this.clients[index].socket.Disconnect(false);
                    MessageBox.Show("Received!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    string header = packet[0].ToString() + packet[1].ToString();
                    if (header == "tt")
                    {
                        int totalLine = getTotal(ref packet);
                    }
                    else
                    {
                        int currentLine = getLine(ref packet);
                        string data = getData(packet);

                        this.listView1.Items.Add(new ListViewItem(data));
                    }
                }
            }
            this.clients[index].canHandle = true;
            this.clients[index].thread.Abort();
        }

        private int getTotal(ref string packet)
        {
            string ttLine = null;
            int i = 2;
            while (i < packet.Length) 
            {
                ttLine += packet[i].ToString();
                i++;
            }
            return Convert.ToInt32(ttLine);
        }

        private int getLine(ref string packet)
        {
            string line = null;

            int i = 2;
            while(packet[i]!='h')
            {
                line += packet[i];
                i++;
            }

            return Convert.ToInt32(line);
        }

        private string getData(string packet)
        {
            string data;

            int r = packet.IndexOf('h');
            data = packet.Remove(0, r + 1);

            return data;
        }
    }
}

using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Messesger
{
    public partial class Form1 : Form
    {
        Socket s1;
        EndPoint epYou, epFriend;

        public Form1()
        {
            InitializeComponent();
            s1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s1.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            textLocalIP.Text = GetLocalIP();
            textFriendsIP.Text = GetLocalIP();

        }

        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "10.1.10.1";
        }

        private void MsgCallBck(IAsyncResult aResult)
        {
            try
            {
                int size = s1.EndReceiveFrom(aResult, ref epFriend);
                if (size > 0)
                { 
                    byte[] receivedData = new byte[1500];

                    receivedData = (byte[])aResult.AsyncState;

                    ASCIIEncoding eEncoding = new ASCIIEncoding();
                    string receivedMessage = eEncoding.GetString(receivedData);
                    listMessage.Items.Add("Friend: "+ receivedMessage);
                }

                byte[] buffer = new byte[1500];
                s1.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epFriend, new AsyncCallback(MsgCallBck), buffer);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(textMessage.Text);

                s1.Send(msg);

                listMessage.Items.Add("You: "+textMessage.Text);

                textMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                epYou = new IPEndPoint(IPAddress.Parse(textLocalIP.Text),Convert.ToInt32(textLocalPort.Text));
                s1.Bind(epYou);

                epFriend = new IPEndPoint(IPAddress.Parse(textFriendsIP.Text), Convert.ToInt32(textFriendsPort.Text));
                s1.Connect(epFriend);

                byte[] buffer = new byte[1500];
                s1.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epFriend, new AsyncCallback(MsgCallBck), buffer);

                button1.Text = "Connected";
                button1.Enabled = false;
                button2.Enabled = true;
                textMessage.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}

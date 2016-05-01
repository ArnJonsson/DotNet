using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Util;

namespace ClientTesting
{

    public class Client
    {
        private static ManualResetEvent connection = new ManualResetEvent(false);
        private static ManualResetEvent send = new ManualResetEvent(false);
        private static ManualResetEvent receive = new ManualResetEvent(false);

        private Msg message;
        private IPAddress address;
        private int port;
        private int bufferSize;
        public static int sessionSize;

        public Client(Msg message, IPAddress address, int port, int bufferSize)
        {

            this.message = message;
            this.address = address;
            this.port = port;
            this.bufferSize = bufferSize;
            sessionSize = bufferSize;

        }

        public void StartClient()
        {
            try
            {
                IPEndPoint endpoint = new IPEndPoint(address, port);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), client);

                connection.WaitOne();

                SendData(client, message);

                send.WaitOne();

                ReceiveData(client);

                receive.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                connection.Set();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void SendData(Socket client, Msg message)
        {
            byte[] data;
            string randomData = "not so random data";
            if (message == null) data = Encoding.UTF8.GetBytes(randomData);
            else data = message.ToByteArray();

            client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendData), client);
        }

        public static void SendData(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int read = client.EndSend(ar);

                send.Set();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void ReceiveData(Socket client)
        {
            try
            {
                Session session = new Session(sessionSize);
                session.workingSocket = client;

                client.BeginReceive(session.buffer, 0, session.bufferSize, 0, new AsyncCallback(ReceiveData), session);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void ReceiveData(IAsyncResult ar)
        {
            try
            {
                Session session = (Session)ar.AsyncState;
                Socket client = session.workingSocket;

                int read = client.EndReceive(ar);

                if(read > 0)
                {
                    foreach(byte bit in session.buffer)
                    {
                        session.receivedData.Add(bit);
                    }

                    client.BeginReceive(session.buffer, 0, session.bufferSize, 0, new AsyncCallback(ReceiveData), session);
                }
                else
                {
                    if(session.receivedData.Count() > 0)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(session.receivedData.ToArray()));
                    }

                    receive.Set();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press enter to fire a client");
            Console.ReadLine();

            Msg message = new Msg();
            message.AddElement(new XElement("Data", new string('.', 500000)));

            Client client = new Client(null, Dns.Resolve(Dns.GetHostName()).AddressList[0], 11000, 1024);
            client.StartClient();

            Console.WriteLine("Press enter to exit");

            Console.ReadLine();
        }
    }
}

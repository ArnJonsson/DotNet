using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Util;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace MsgSocket
{
    /// <summary>
    /// Tcp Socket server for message exchange
    /// </summary>
    public class MsgSocket : ConsoleApp
    {
        private bool isRunning = false;

        // initial listener
        private Thread listener;

        // thread designated for session cleanup
        private System.Timers.Timer reclaimer;

        // list of active session -- read up on better methods, this looks as wrong as it feels
        // pushing problem at the end of todos
        private List<Session> sessions = new List<Session>();

        // async signaling
        public static ManualResetEvent eventHandler = new ManualResetEvent(false);

        private IPAddress address;
        private int port;
        private int bufferSize;
        public static int sessionSize;

        const string callBackFlag = "The message was transfered successfully";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param> 
        /// <param name="port"></param>
        /// <param name="bufferSize"></param>
        public MsgSocket(IPAddress address, int port, int bufferSize)
        {
            this.address = address;
            this.port = port;
            this.bufferSize = bufferSize;
            sessionSize = bufferSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public override void Invoke(Command command)
        {
            switch(command.command)
            {
                case "Start":
                    Start();
                    break;
                case "Stop":
                    Stop();
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if(isRunning)
            {
                Console.WriteLine("".PadLeft(4) + "Server is already running");
                return;
            }
            else
            {
                Console.WriteLine("".PadLeft(4) + "Attempting to start TcpSocketServer...");
                
                listener = new Thread(() => StartListening(address, port, bufferSize));
                listener.Start();
                

                // the resource reclaiming thread fires every 3 seconds
                /*
                reclaimer = new System.Timers.Timer(3000);
                reclaimer.Elapsed += Reclaim;
                */

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if(!isRunning)
            {
                Console.WriteLine("".PadLeft(4) + "Server is not running");
            }
            else
            {
                listener.Join(5000);

                if(listener.IsAlive)
                {
                    listener = null;
                }

                Console.WriteLine("".PadLeft(4) + "Server terminated");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="bufferSize"></param>
        public static void StartListening(IPAddress address, int port, int bufferSize)
        {
            IPEndPoint endpoint = new IPEndPoint(address, port);
            byte[] dataBuffer = new byte[bufferSize];

            Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                tcpListener.Bind(endpoint);
                // 10 for testing purposes
                tcpListener.Listen(10);

                while(true)
                {
                    // nonsignal state
                    eventHandler.Reset();

                    Console.WriteLine("".PadLeft(4) + "MsgSocket started listening at port : " + port);

                    tcpListener.BeginAccept(new AsyncCallback(BeginRecieve), tcpListener);

                    // wait untill a message is received

                    eventHandler.WaitOne();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("".PadLeft(4) + "Failed to start MsgSocket succefully");
                Console.WriteLine("".PadLeft(4) + e.Message);
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void BeginRecieve(IAsyncResult ar)
        {
            Console.WriteLine("".PadLeft(4) + "BeginReceive");
            //set signal
            eventHandler.Set();

            Socket tcpListener = (Socket)ar.AsyncState;
            Socket handler = tcpListener.EndAccept(ar);

            Session session = new Session(sessionSize);
            session.workingSocket = handler;
            handler.BeginReceive(session.buffer, 0, session.bufferSize, 0, new AsyncCallback(CheckReceived), session); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void CheckReceived(IAsyncResult ar)
        {
            Console.WriteLine("".PadLeft(4) + "Checkreceived");

            Session session = (Session)ar.AsyncState;
            Socket handler = session.workingSocket;

            int read = handler.EndReceive(ar);

            // check received data
            if(!session.isAuthenticated && session.receivedData.Count() > 5)
            {
          
                List<byte> auList = session.receivedData.GetRange(0, 5);

                if(auList.ToString() == "<root")
                {
                    session.isAuthenticated = true;
                    Console.WriteLine("".PadLeft(4) + "Session authenticated");
                }
                else
                {
                    Console.WriteLine("".PadLeft(4) + "Failed to authenticate session");
                    BadRecieve(handler, "Socket shutdown");
                    session.terminate = true;
                    // look into premature cancellation
                    return;
                }
       
            }


            if(read > 0)
            {
                // store data received so far
                foreach (byte bit in session.buffer)
                {
                    session.receivedData.Add(bit);
                }

                string endFlag = session.receivedData.ToString();

                if(endFlag.IndexOf("</root>") > -1 || endFlag.IndexOf("<root />") > -1)
                {
                    SendCallback(handler, callBackFlag);
                }




            }
            else
            {
                handler.BeginReceive(session.buffer, 0, session.bufferSize, 0, new AsyncCallback(CheckReceived), session);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void BadRecieve(Socket handler, string message)
        {
            // TODO : this;
        }

        public static void BadReceive(IAsyncResult ar)
        {
            // and this;
        }

        /// <summary>
        /// z
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(Socket handler, string message)
        {
            Console.WriteLine("".PadLeft(4) + "SendCallback");
            byte[] response = Encoding.UTF8.GetBytes(message);

            handler.BeginSend(response, 0, response.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int sent = handler.EndSend(ar);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                Console.WriteLine("".PadLeft(4) + "Callback successfull");
            }
            catch(Exception e)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public void Reclaim(object source, ElapsedEventArgs e)
        {
            
        }
    }
}

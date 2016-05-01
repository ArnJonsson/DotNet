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
        // isrunning
        private bool isRunning = false;

        // initial listener
        private Thread listener;

        // thread designated for session cleanup
        private System.Timers.Timer reclaimer;

        // list of active session -- read up on better methods, this looks as wrong as it feels
        // pushing problem at the end of todos
        private static List<Session> sessions = new List<Session>();

        // async signaling
        public static ManualResetEvent eventHandler = new ManualResetEvent(false);


        private IPAddress address;
        private int port;
        private int bufferSize;
        public static int sessionSize;

        // Data authentication?
        private static string[] startTokens;
        private static string[] stopTokens;


        const string serverTag = "%Server";
        const string callBackFlag = "The message was transfered successfully";

        /// <summary>
        /// MsgSocket()
        /// A skeleton for a low level Tcp socket server
        /// 
        /// Server and client exchange Msg.class which houses an xml object 
        /// </summary>
        /// <param name="address">Target address</param> 
        /// <param name="port">Target port</param>
        /// <param name="bufferSize">Connection session buffersize</param>
        /// <param name="tokens">Data authentication</param>
        public MsgSocket(IPAddress address, int port, int bufferSize, RecieveTokens tokens)
        {
            this.address = address;
            this.port = port;
            this.bufferSize = bufferSize;
            sessionSize = bufferSize;

            startTokens = (string[])tokens.startTokens;
            stopTokens = (string[])tokens.stopTokens;
        }

        /// <summary>
        /// Invoke()
        /// ConsoleApp.class override
        /// 
        /// Intended use is for all communication with the "outside" world 
        /// (read up on Reflections to get rid of hard coding)
        /// </summary>
        /// <param name="command">Command command</param>
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
        /// Start()
        /// 
        /// Starts the listening thread aswell as periodic resource reclaimer
        /// </summary>
        public void Start()
        {
            if(isRunning)
            {
                Print("Server is already running");
                return;
            }
            else
            {
                Print("Attempting to start TcpSocketServer...");
                
                listener = new Thread(() => StartListening(address, port, bufferSize));
                listener.IsBackground = true;
                listener.Start();


                // the resource reclaiming thread fires every 3 seconds
                
                reclaimer = new System.Timers.Timer(3000);
                reclaimer.Elapsed += Reclaim;
                reclaimer.Enabled = true;
                isRunning = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if(!isRunning)
            {
                Print("Server is not running");
            }
            else
            {
                // Random number
                listener.Join(5000);

                // Opinions seem to differ on Thread.Abort() - read up on it
                if(listener.IsAlive)
                {
                    listener = null;
                }

                isRunning = false;
                Print("Server terminated");
            }
        }

        /// <summary>
        /// StartListening(...)
        /// 
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

                Print(string.Format("Attempting to start listening at port {0}", port));

                while(true)
                {
                    // nonsignal state
                    eventHandler.Reset();

                    tcpListener.BeginAccept(new AsyncCallback(BeginRecieve), tcpListener);

                    // wait untill a message is received
                    eventHandler.WaitOne();
                }
            }
            catch(Exception e)
            {
                Print("Failed to start MsgSocket succefully");
                Print(e.Message);
            }
        }
        

        /// <summary>
        /// BeginRecieve()
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void BeginRecieve(IAsyncResult ar)
        {
            //set signal
            eventHandler.Set();

            Socket tcpListener = (Socket)ar.AsyncState;
            Socket handler = tcpListener.EndAccept(ar);

            Session session = new Session(sessionSize);
            session.workingSocket = handler;

            lock(sessions)
            {
                sessions.Add(session);
            }

            Print((string.Format("ClientSession created with Guid {0}", session.gId.ToString())));

            handler.BeginReceive(session.buffer, 0, session.bufferSize, 0, new AsyncCallback(CheckReceived), session); 
        }

        /// <summary>
        /// CheckRecieved
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void CheckReceived(IAsyncResult ar)
        {

            Session session = (Session)ar.AsyncState;
            Socket handler = session.workingSocket;

            int read;
            try {
                read = handler.EndReceive(ar);
            }
            catch(Exception e)
            {
                // The connection was forcibly closed by the remote host?
                // add to reading material
                Print(e.Message);
                return;
            }

            if(read > 0)
            { 

                foreach(byte bit in session.buffer)
                {
                    session.receivedData.Add(bit);
                }

                // End strings
                if(session.Contains(stopTokens))
                {
                    Print((string.Format("ClientSession {0} data received", session.gId.ToString())));

                    SendCallback(handler, callBackFlag, session.gId);
                }
                else
                {
                    if(session.isAuthenticated)
                    {
                        handler.BeginReceive(session.buffer, 0, session.bufferSize, 0, new AsyncCallback(CheckReceived), session);
                    }
                    else
                    {
                        // """Start""" strings
                        if(session.Contains(startTokens))
                        {
                            Print(string.Format("session {0} successfully authenticated", session.gId.ToString()));
                            session.isAuthenticated = true;
                            handler.BeginReceive(session.buffer, 0, session.bufferSize, 0, new AsyncCallback(CheckReceived), session);
                        }
                        else
                        {
                            Print(string.Format("authentication for session {0} failed, marking for termination", session.gId.ToString()));
                            session.terminate = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// BadRecieve()
        /// 
        /// Gets called if data decryption fails (onTodo)
        /// </summary>
        /// <param name="ar"></param>
        public static void BadRecieve(Socket handler, string message)
        {
            // TODO : this;
        }

        /// <summary>
        /// See above
        /// </summary>
        /// <param name="ar"></param>
        public static void BadReceive(IAsyncResult ar)
        {
            // and this;
        }

        /// <summary>
        /// SendCallback
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(Socket handler, string message, Guid id)
        {
            byte[] response = Encoding.UTF8.GetBytes(message);

            Print((string.Format("Sending transfer signal to Client {0}", id.ToString())));

            handler.BeginSend(response, 0, response.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int sent = handler.EndSend(ar);

                Print("Client closed");

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                
            }
            catch(Exception e)
            {
                Print(e.Message);
            }
        }

        /// <summary>
        /// Reclaim (timer)
        /// 
        /// Periodically scan active clients - removing those signaled for termination
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public static void Reclaim(object source, ElapsedEventArgs e)
        {
            lock(sessions)
            {
                //Print("Trying to reclaim resources");
                foreach(Session session in sessions.ToList())
                {
                    if(session.terminate)
                    {
                        Print(string.Format("Session {0} terminated", session.gId.ToString()));
                        session.workingSocket = null;
                        session.receivedData = null;
                        sessions.Remove(session);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void Print(string message = "")
        {
            Console.WriteLine(serverTag + "".PadLeft(4) + message);
        } 
    }
}

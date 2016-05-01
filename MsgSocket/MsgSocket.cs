using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Util;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MsgSocket
{
    /// <summary>
    /// Tcp Socket server for message exchange
    /// </summary>
    public class MsgSocket : ConsoleApp
    {
        private bool isRunning = false;

        private Socket listener;
        private Thread reclaimer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param> 
        /// <param name="port"></param>
        /// <param name="bufferSize"></param>
        public MsgSocket(IPAddress address, int port, int bufferSize)
        {
            Console.WriteLine("uselessness");
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
                return;
            }
            else
            {
                // start listening thread
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if(!isRunning)
            {
                return;
            }
            else
            {
                // cleanup
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void BeginRecieve(IAsyncResult ar)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void BadRecieve(IAsyncResult ar)
        {

        }

        /// <summary>
        /// z
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(IAsyncResult ar)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public static void Reclaim(IAsyncResult ar)
        {

        }
    }
}

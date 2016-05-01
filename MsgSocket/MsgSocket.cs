using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Util;
using System.Net;
using System.Net.Sockets;

namespace MsgSocket
{
    /// <summary>
    /// 
    /// </summary>
    public class MsgSocket : ConsoleApp
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param> 
        /// <param name="port"></param>
        public MsgSocket(IPAddress address, int port)
        {
            Console.WriteLine("uselessness");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public override void Invoke(Command command)
        {
            Console.WriteLine("damn this is nice");

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using Util;
using System.Reflection;
using System.Xml.Linq;
using System.Timers;

namespace MsgSocket
{

    /// <summary>
    /// 
    /// </summary>
    public class Controller
    {
        private ConsoleApp ca;
        private string className;
        private MethodInfo[] classMethods;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ca"></param>
        public Controller(ConsoleApp ca)
        {
            this.ca = ca;
            className = ca.GetType().FullName;
            classMethods = ca.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            PrintInstructions();
        }


        private void PrintInstructions()
        {
            Console.WriteLine("Console application - TcpSocket");
            Print("Controller currently running - " + className);
            Print("Legal Methods are as follows:");
            foreach (MethodInfo method in classMethods)
            {
                string info = "".PadLeft(4) + " " + method.Name;
                if (method.GetParameters().Length > 0)
                {
                    foreach (ParameterInfo paramInfo in method.GetParameters())
                    {
                        info += string.Format(" ({0} , {1})", paramInfo.ParameterType, paramInfo.Name);
                    }

                }
                else
                {
                    info += " (void)";
                }
                Console.WriteLine(info);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public void Process(string command)
        {
            Command cmd = new Command(command);
            MethodInfo targetCommand = null;
            foreach (MethodInfo info in classMethods)
            {
                if (info.Name == cmd.command)
                {
                    targetCommand = info;
                    break;
                }
            }

            if (targetCommand == null)
            {
                Print("BAD command - The method you wanted to run does not exist");
                return;
            }

            if (targetCommand.GetParameters().Length != cmd.arguments.Count)
            {
                Print(string.Format("BAD command - Method {0} requires {1} arguments", targetCommand.Name, targetCommand.GetParameters().Length));
                return;
            }
            else
            {
                ca.Invoke(cmd);
            }



        }


        private void Print(string message = "")
        {
            Console.WriteLine("console>: " + message);
            //Console.WriteLine("gittest");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            bool debug = false;
            if(debug)
            {
                Debug();
                return;
            }

            string[] startTokens = { "<root>", "<root />" };
            string[] stopTokens = { "</root>", "<root />" };
            //
            Controller cntrl = new Controller(new MsgSocket(Dns.Resolve(Dns.GetHostName()).AddressList[0], 11000, 1024, new RecieveTokens(startTokens, stopTokens)));

            // while true loop
            var input = "";
            while(true)
            {
                input = Console.ReadLine();
                if (input == "exit") break;
                cntrl.Process(input);
            }

        }

        static void Debug()
        {
            tPrint("Begin testing");

            tPrint("Msg empty constructor");

            Msg msg = new Msg();
            tPrint("TEST : Expenting a single root element");
            tPrint(msg.AsString());

            msg.AddElement(new XElement("child", "some data"));
            tPrint("TEST : Expecting a single child element in root with some data");
            tPrint(msg.AsString());

            XDocument doc = new XDocument();
            XElement ele1 = new XElement("dataOne", "somedata1");
            XElement ele2 = new XElement("dataTwo", "somedata2");
            XElement ele3 = new XElement("dataThree", "somedata3");
            doc.Add(ele1);
            ele1.Add(ele2);
            ele2.Add(ele3);

            Msg msgTwo = new Msg(doc);
            tPrint("TEST : Expecting a msg with 3 data elements and some data");
            tPrint(msgTwo.AsString());

            Timer timer = new Timer(1000);
            timer.Elapsed += Timerararar;
            timer.AutoReset = true;
            timer.Enabled = true;

            // preventing app shutdown
            Console.ReadLine();
        }

        static void Timerararar(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Hello");
        }

        static void tPrint(string message = "")
        {
            Console.WriteLine(message);
        }
    }
}

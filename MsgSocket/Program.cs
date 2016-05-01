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
using System.IO;
using System.Threading;

namespace MsgSocket
{

    public class CompTest
    {
        public CompTest()
        {

        }

        public void MsgTest1()
        {
            
            string dir = "C:\\Users\\arnar\\AndroidStudioProjects\\Beygdu\\app\\src\\main\\res\\layout";

            string[] files = Directory.GetFiles(dir);

        
            foreach(string file in files)
            {
                try {
                    Msg message = new Msg(XDocument.Load(file));
                    Console.WriteLine(message.AsString());
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.ToString());
                }
            }
           
        }

        public void MsgTest2()
        {

          string dir = "C:\\Users\\arnar\\AndroidStudioProjects\\Beygdu\\app\\src\\main\\java\\is\\example\\aj\\beygdu\\Utils";

            string[] files = Directory.GetFiles(dir);


            foreach (string file in files)
            {
                try
                {
                    Msg message = new Msg(XDocument.Load(file));
                    Console.WriteLine(message.AsString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void MsgTest3()
        {
            byte[] randomArray = new byte[1024];
            Random r = new Random();
            r.NextBytes(randomArray);

            try
            {
                Msg message = Msg.FromByteArray(randomArray);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
            }
        }
    }

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
            bool debug = true;
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

            CompTest test = new CompTest();

            //test.MsgTest1(); succ
            //test.MsgTest2(); fail, xmlException on XDocument - if the XDocument is valid current version vill not fail
            test.MsgTest3();


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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Util;
using System.Reflection;

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
            Console.WriteLine("gittest");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //
            Controller cntrl = new Controller(new MsgSocket(null, 0));

            // while true loop
            var input = "";
            do
            {
                input = Console.ReadLine();
                cntrl.Process(input);
            }
            while (input != "exit");

        }
    }
}

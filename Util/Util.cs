using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace Util
{
    public class Util
    {
    }

    /// <summary>
    /// abstract class ConsoleApp
    /// </summary>
    abstract public class ConsoleApp
    {

        abstract public void Invoke(Command command);
    }

    /// <summary>
    /// class Command
    /// </summary>
    public class Command
    {
        public string command;
        public List<string> arguments;

        /// <summary>
        /// Command(string str)
        /// 
        /// </summary>
        /// <param name="str">input from console</param>
        public Command(string str)
        {
            var input = Regex.Split(str, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            arguments = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                // command (method)
                if (i == 0)
                {
                    command = input[i];
                }
                // arguments
                else
                {
                    string argument = input[i];

                    // are the parameters within quotes?
                    var regex = new Regex("\"(.*?)\"", RegexOptions.Singleline);
                    var match = regex.Match(argument);

                    if (match.Captures.Count > 0)
                    {
                        var unedited = new Regex("[^\"]*[^\"]");
                        var edited = unedited.Match(match.Captures[0].Value);
                        argument = edited.Captures[0].Value;
                    }
                    arguments.Add(argument);
                }
            }
        }
    }

    /// <summary>
    /// Base class for message-exchange between client and server socket
    /// </summary>
    public class Msg
    {
        public bool isLegal { get; set; }
        public Exception exeption { get; set; }

        public Msg()
        {
            isLegal = true;
            exeption = new Exception();
        }
    }

    public class TransferMsg : Msg
    {
        public List<byte> data = new List<byte>();

        public static byte[] ToByteArray(TransferMsg msg)
        {
            if(msg.data != null)
            {
                return msg.data.ToArray();
            }
            return null;
            
        }

        public static TransferMsg FromByteArray(byte[] array)
        {
            if(array != null)
            {
                var list = new List<byte>(array);
                TransferMsg msg = new TransferMsg();
                msg.data = list;
                return msg;
            }

            return null;
        }
    }
}

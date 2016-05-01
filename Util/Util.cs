using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

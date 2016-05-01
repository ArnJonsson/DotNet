using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace Util
{
    /// <summary>
    /// TODO: create a private real c# class library
    /// </summary>
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
    /// Message class - hold on to data from socket-to-socket communication
    /// </summary>
    public class Msg
    {
        // XML document
        public XDocument doc { get; private set; }
        // root element
        private XElement root;

        private const string wrapper = "root";

        /// <summary>
        /// Msg(XDocument document)
        /// Creates a new Msg xml object containing elements from document
        /// </summary>
        /// <param name="document">XML file to be sent to/from socket</param>
        public Msg(XDocument document)
        {
            doc = new XDocument(new XDeclaration("1,0", "utf-8", "no"));
            root = new XElement(wrapper);

            XElement documentRoot = document.Root;

            root.Add(documentRoot);

            doc.Add(root);
        }

        /// <summary>
        /// Msg()
        /// Creates a new empty XML object intended for data transfer from socket to socket
        /// </summary>
        public Msg()
        {
            doc = new XDocument(new XDeclaration("1,0", "utf-8", "no"));
            root = new XElement(wrapper);
            doc.Add(root);
        }

        /// <summary>
        /// AddElement(XElement element)
        /// Adds element to the Msg root element
        /// </summary>
        /// <param name="element">XElement element</param>
        /// <returns></returns>
        public bool AddElement(XElement element)
        {
            try
            {
                root.Add(element);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// AsString()
        /// </summary>
        /// <returns>A string representation of the Msg xml document</returns>
        public string AsString()
        {
            return doc.ToString();
        }

        /// <summary>
        /// Converts the Msg to a byte array
        /// </summary>
        /// <returns>String representation of the Msg xml document</returns>
        public byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();
            doc.Save(ms);
            // resetting the streams position - read up on functionality
            ms.Position = 0;
            return ms.ToArray();
        }

        /// <summary>
        /// Converts a byte array to a Msg xml document
        /// </summary>
        /// <param name="array">byte[] array</param>
        /// <returns>Msg</returns>
        public static Msg FromByteArray(byte[] array)
        {
            MemoryStream ms = new MemoryStream(array);
            return new Msg(XDocument.Load(ms));
        }
    }

    /// <summary>
    /// State object for socket server connections
    /// </summary>
    public class Session
    {
        public Guid gId;

        public bool terminate = false;

        public bool isAuthenticated = false;

        public Socket workingSocket = null;

        public int bufferSize;

        public byte[] buffer;

        public List<byte> receivedData = new List<byte>();

        public Session(int bufferSize = 1024)
        {
            this.bufferSize = bufferSize;
            buffer = new byte[bufferSize];
            gId = Guid.NewGuid();
        }
    }

    
}

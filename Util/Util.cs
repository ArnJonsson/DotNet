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
    /// Parent of classes used by ApplicationControl (see Program.c in MsgSocket)
    /// </summary>
    abstract public class ConsoleApp
    {

        abstract public void Invoke(Command command);
    }

    /// <summary>
    /// Command()
    /// 
    /// Class intended to be used as a command for ApplicationControl (see Program.c in MsgSocket)
    /// Takes in a string read from console and splits it into a command and arguments
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
    /// Msg()
    /// 
    /// A very (bad?) Xml object intended for socket-to-socket comminucation
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
        /// Creates a new Msg xml object containing the elements from document
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
        /// Creates a new XML document with one (root) element
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
        /// <returns>byte[] representing the Msg class</returns>
        public byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();
            doc.Save(ms);
            // resetting the streams position - read up on functionality (Position/Flush/general)
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
            try {
                MemoryStream ms = new MemoryStream(array);
                return new Msg(XDocument.Load(ms));
            }
            catch(System.Xml.XmlException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }

    /// <summary>
    /// State object for socket server connections
    /// Represents a client
    /// </summary>
    public class Session
    {
        // global id
        public Guid gId;

        // termination flag
        public bool terminate = false;

        // authentication flag
        public bool isAuthenticated = false;

        public Socket workingSocket = null;

        public int bufferSize;

        public byte[] buffer;

        // total bytes recieved
        public List<byte> receivedData = new List<byte>();

        public Session(int bufferSize = 1024)
        {
            this.bufferSize = bufferSize;
            buffer = new byte[bufferSize];
            gId = Guid.NewGuid();
        }

        /// <summary>
        /// Contains(str)
        /// Checks if sent data contains certain strings
        /// Used for data authentication
        /// Read up on authentication methods?
        /// </summary>
        /// <param name="str">String array</param>
        /// <returns>True if Msg contains a string in str, false otherwise</returns>
        public bool Contains(string[] str)
        {
            string dataSoFar = Encoding.UTF8.GetString(receivedData.ToArray());
            foreach(string s in str)
            {
                if (dataSoFar.Contains(s)) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// RecieveTokens
    /// 
    /// Object tokens for data authentication
    /// </summary>
    public class RecieveTokens
    {
        public object[] startTokens;
        public object[] stopTokens;

        public RecieveTokens(object[] startTokens, object[] stopTokens)
        {
            this.startTokens = startTokens;
            this.stopTokens = stopTokens;
        }
    }

    
}

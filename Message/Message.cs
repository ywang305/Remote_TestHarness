/////////////////////////////////////////////////////////////////////
// Messages.cs - defines communication messages                    //
// ver 1.0                                                         //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Messages provides helper code for building and parsing XML messages.
 *
 * Required files:
 * ---------------
 * - Messages.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 16 Nov 2016
 * - first release
 */
using System;
using System.Runtime.Serialization;

namespace MessageNameSpace
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public string destination { get; set; }
        [DataMember]
        public string author { get; set; } = "";
        [DataMember]
        public string type { get; set; } = "default";
        [DataMember]
        public DateTime time { get; set; } = DateTime.Now;
        [DataMember]
        public string testkey { get; set; } = "";
        [DataMember]
        public string body { get; set; } = "";

        public Message(string bodyStr="")
        {
            body = bodyStr;
        }

    }

    public static class ExtMethods
    {
        public static void ShowMsg(this Message msg)
        {
            Console.WriteLine("{0, 16}: {1}", "source", msg.source);
            Console.WriteLine("{0, 16}: {1}", "desti", msg.destination);
            Console.WriteLine("{0, 16}: {1}", "author", msg.author);
            Console.WriteLine("{0, 16}: {1}", "type", msg.type);
            Console.WriteLine("{0, 16}: {1}", "time", msg.time.ToString());
            Console.WriteLine("{0, 16}: {1}", "body", msg.body);
            Console.WriteLine();
        }
    } 


#if (TEST_MESSAGE)
    public class TestMessage
    {
        [STAThread]
        static void Main(string[] args)
        {
            Message msg = new Message();
            msg.source = "http://localhost:4040/ICommuService/WSHttp";
            msg.destination = "http://localhost:4041/ICommuService/WSHttp";
            msg.author = "Fawcett";
            msg.type = "TestRequest";
            msg.ShowMsg();
            Console.ReadKey();
            Console.Write("\n\n");
        }
    }
#endif
}

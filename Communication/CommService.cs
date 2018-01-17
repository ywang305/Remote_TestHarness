///////////////////////////////////////////////////////////////////////
// StreamService.cs - WCF Message Service in Self Hosted Configuration //
//                                                                   //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Summer 2009 //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - Uses Programmatic configuration, no app.config file used.
 * - Uses ChannelFactory to create proxy programmatically. 
 * - Expects to recv test request message from clients
 * - Will create child threads to handle messages.
 * 
 * Required Files:
 * - CS-BlockingQueue.cs, Message.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 17 Nov 2016
 * - optimize code structure.
 *  
 * ver 1.0 : 16 Nov 2016
 * - first release 
 */
using MessageNameSpace;
using System;
using System.Threading.Tasks;
using System.Threading;
using CS_BlockingQueue;
using System.ServiceModel;

namespace Communication
{
    public class Receiver<T>: ICommService
    { //tricky: distinct T for distinct receivers
        static BlockingQueue<Message> rcvBlockingQ = null;
        ServiceHost servicehost = null;
        public string name { get; set; }

        public Receiver()
        {
            if (rcvBlockingQ == null)
                rcvBlockingQ = new BlockingQueue<Message>();
        }

        public Thread StartRcvThead(ThreadStart rcvThreadProc)
        {
            Thread rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            return rcvThread;
        }

        public void CreateRecvChannel(string address)
        {
            while(servicehost==null)
            {
                try
                {
                    Uri baseAddress = new Uri(address);
                    servicehost = new ServiceHost(typeof(Receiver<T>), baseAddress);
                    servicehost.AddServiceEndpoint(typeof(ICommService), new WSHttpBinding(), baseAddress);
                    servicehost.Open();
                    Console.WriteLine("\n  Service is open listening on {0}", address);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("{0,16}", ex.InnerException.Message);
                    System.Threading.Thread.Sleep(2000);
                    servicehost = null;
                }
            }
            
        }

        public void CloseRecvChannel()
        {
            servicehost.Close();
        }

        public void PostMessage(Message msg)
        {
            rcvBlockingQ.enQ(msg);
        }

        public Message GetMessage()
        {
            Message msg = rcvBlockingQ.deQ();
            return msg;
        }
    }



    //----< processing for send thread >-----------------------------

    public class Sender
    {
        public string name { get; set; }
        ICommService channel;
        string lastError = "";
        BlockingQueue<Message> sndBlockingQ = null;
        int tryCount = 0, MaxCount = 100;
        string currEndpoint = "";

        public Sender()
        {
            sndBlockingQ = new BlockingQueue<Message>();
            Task.Run(() => ThreadProc());
        }
        // post msg into sender's queue
        public void PostMessage(Message msg)
        {
            sndBlockingQ.enQ(msg);
            Console.WriteLine("\nPost a request message into TestHarness's rcvBlockingQ");
        }

        void ThreadProc()
        {
            tryCount = 0;
            while(true)
            {
                Message msg = sndBlockingQ.deQ();  // out of snd Queue
                if(msg.destination!=currEndpoint)
                {
                    currEndpoint = msg.destination;
                    while(true)
                    {
                        try
                        {
                            CreateSendChannel(currEndpoint);
                            break;
                        }
                        catch(Exception)
                        {
                            System.Threading.Thread.Sleep(200);
                        }     
                    }
                }
                while(true)
                {
                    try
                    {
                        channel.PostMessage(msg);  // enQueue to rcver's queue, serverend
                        Console.Write("\n  posted message from {0} to {1}", name, msg.destination);
                        tryCount = 0;
                        break;
                    }
                    catch(Exception)
                    {
                        Console.WriteLine("\n connection failed, reconnecting...");
                        if (++tryCount < MaxCount)
                            System.Threading.Thread.Sleep(100);
                        else
                        {
                            Console.WriteLine("\n  {0}", "can't connect");
                            currEndpoint = "";
                            tryCount = 0;
                            break;
                        }
                    }
                }
                if (msg.body == "quit")
                    break;
            }
        }

        //----< Create proxy to another Peer's Communicator >------------

        public void CreateSendChannel(string address)
        {
            ChannelFactory<ICommService> factory
              = new ChannelFactory<ICommService>(new WSHttpBinding(), new EndpointAddress(address));
            channel = factory.CreateChannel();
            Console.Write("\n  service proxy created for {0}", address);
        }

        //----< closes the send channel >--------------------------------

        public void CloseSendChannel()
        {
            ChannelFactory<ICommService> temp = (ChannelFactory<ICommService>)channel;
            temp.Close();
        }

        public string GetLastError()
        {
            string temp = lastError;
            lastError = "";
            return temp;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Comm class simply aggregates a Sender and a Receiver
    //
    public class CommService<T>
    {
        public string name { get; set; }
        public Receiver<T> rcvr { get; set; }
        public Sender sndr { get; set; }
        public CommService()
        {
            name = typeof(T).Name;
            rcvr = new Receiver<T>();
            sndr = new Sender();
            rcvr.name = name;
            sndr.name = name;       
        }
        public static string makeEndPoint(string url, int port)
        {
            string endPoint = url + ":" + port.ToString() + "/ICommuService/WSHttp";
            return endPoint;
        }
    }


#if (TEST_COMMSERVICE)
    public class Cat { }
    public class TestStub
    {
        [STAThread]
        static void Main(string[] args)
        {
            CommService<Cat> comm = new CommService<Cat>();
            string endPoint = CommService<Cat>.makeEndPoint("http://localhost", 4040);
            comm.rcvr.CreateRecvChannel(endPoint);
            comm.sndr.CreateSendChannel(endPoint);
            comm.sndr.PostMessage(new Message("Message #1"));
            comm.sndr.PostMessage(new Message("quit"));
            Console.ReadKey();
            Console.Write("\n\n");
        }
    }
#endif

}

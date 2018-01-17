/////////////////////////////////////////////////////////////////////
// TestHarness.cs - TestHarness Engine: creates child domains      //
// ver 1.0                                                         //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * WCF based service host
 * - self-host as a message communication provider
 * - expect client's request message via service channel
 * WCF based client
 * - create FileStream service proxy of repository service
 * TestHarness package provides integration testing services.  It:
 * - receives structured test requests
 * - retrieves cited files from a repository
 * - executes tests on all code that implements an ITest interface,
 *   e.g., test drivers.
 * - reports pass or fail status for each test in a test request
 * - stores test logs in the repository
 * It contains classes:
 * - TestHarness that runs all tests in child AppDomains
 * - Callback to support sending messages from a child AppDomain to
 *   the TestHarness primary AppDomain.
 * - Test and RequestInfo to support transferring test information
 *   from TestHarness to child AppDomain
 * 
 * Required Files:
 * ---------------
 * - TestHarness.cs, BlockingQueue.cs
 * - ITest.cs, ICommService.cs
 * - LoadAndTest, Logger, Messages
 *
 * Maintanence History:
 * --------------------
 * ver 1.0 : 19 Nov 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using System.Security.Policy;    // defines evidence needed for AppDomain construction
using System.Runtime.Remoting;   // provides remote communication between AppDomains
using Communication;
using MessageNameSpace;
using Interfaces;
using System.Runtime.Serialization;
using FileStreamService;
using System.ServiceModel;

namespace TestHarenssServer
{
    public class Callback : MarshalByRefObject, ICallback
    {
        public class TestResults: ITestResults
        {
            public string testKey { get; set; }
            public DateTime dateTime { get; set; } = DateTime.Now;
            public List<ITestResult> testResultList { get; set; } = new List<ITestResult>();
        }
        ITestResults trs = new TestResults();
        public void sendMessage(ITestResult msg)
        {
            Console.WriteLine("\n\n  received msg from childDomain:\n test name :  {0}\n test result :  {1}\n test log :  {2}", msg.testName, msg.testResult, msg.testLog);
            trs.testResultList.Add(msg);
        }
        public ITestResults GetMessage()
        {
            return trs;
        }
    }

    class TestHarness
    {
        [Serializable]
        class TestInfo: ITestInfo
        {
            public string testName { get; set; }
            public string testCode { get; set; }
            public string testDriver { get; set; }
        }
        static IStreamService CreateClientFileServiceChannel(string url)
        {
            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;

            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 500000000;
            EndpointAddress address = new EndpointAddress(url);

            ChannelFactory<IStreamService> factory
              = new ChannelFactory<IStreamService>(binding, address);
            return factory.CreateChannel();
        }


        private ICallback cb_= new Callback();

        private FileStreamService.IStreamService streamproxy = null;
        public CommService<TestHarness> comm { get; set; }
        public string endPoint { get; }
        public TestHarness()
        {
            Console.Title = "TestHareness Server";
            streamproxy = CreateClientFileServiceChannel("http://localhost:8000/StreamService");
            comm = new CommService<TestHarness>();
            endPoint = CommService<TestHarness>.makeEndPoint("http://localhost", 4040);
            comm.rcvr.CreateRecvChannel(endPoint);
            comm.rcvr.StartRcvThead(rcvThreadProc);
        }

        void rcvThreadProc()
        {
            while (true)
            {
                HRTimer.HiResTimer hrt = new HRTimer.HiResTimer();
                Message msg = comm.rcvr.GetMessage();
                hrt.Start();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message:\n", comm.name);
                msg.ShowMsg();
                if (msg.body == "quit")
                    break;
                switch(msg.type)
                {
                    case "testrequest":
                        Task.Run(() => RunTestThrdProc(msg));
                        break;
                    default:
                        break;
                }
                hrt.Stop();
                Console.Write(
                    "\n  Complete \"{0}\"'s test request in Thread_ID: {1} in {2} microsec.",
                    msg.author, Thread.CurrentThread.ManagedThreadId, hrt.ElapsedMicroseconds);
            }
        }

        void RunTestThrdProc(Message msg) // msg= testreqeust message
        {
            List<TestInfo> testInfoList = new List<TestInfo>();
            XDocument xdoc = XParse(testInfoList, msg.body);
            xdoc.ShowXMLBody();
            AppDomain ad = ChildDomain();
            ILoadAndTest ilt = installLoader(ad); // LoadAndTest proxy  
            foreach ( var tstinfomsg in testInfoList)
            {
                ilt.LoadTests(tstinfomsg); // pass testInfo msg to proxy method
            }
            ITestResults trs =  cb_.GetMessage();  // cb is callback object that returns testResults
            trs.testKey = msg.author +"_"+ trs.dateTime.ToString("yyyy-mm-dd-hh-mm-ss-ffff") + "_ThreadID" + Thread.CurrentThread.ManagedThreadId+".txt";

            Message resultMsg = MakeTestRsultMessage(trs, msg);

            //------  logger to Repository -------------

            streamproxy.writeLog(resultMsg);

            // ------reply testResult msg to client ---------
            
            this.comm.sndr.PostMessage(resultMsg);

            testInfoList.Clear();
            AppDomain.Unload(ad);
        }

        XDocument XParse(List<TestInfo> testInfoList, string strBody)
        {       
            XDocument xdoc = XDocument.Parse(strBody);
            foreach (var body in xdoc.Descendants("test"))
            {
                TestInfo tif = new TestInfo();
                tif.testName = body.Attribute("name").Value;
                tif.testCode = body.Element("testCode").Value;
                tif.testDriver = body.Element("testDriver").Value;
                testInfoList.Add(tif);
            }
            return xdoc;
        }

        Message MakeTestRsultMessage(ITestResults trs, Message testrequest)
        {
            string strBody = @"<test_result>";
            foreach(var item in trs.testResultList)
            {
                string tempString = @"<test name='" + item.testName + @"'>" +
                    @"<testResult>" + item.testResult + @"</testResult>" +
                    @"<testLog>" + item.testLog + @"</testLog>"+
                    @"</test>";
                strBody += tempString;
            }
            strBody += @"</test_result>";
            Message resultMsg = new Message(strBody);
            resultMsg.author = testrequest.author;
            resultMsg.source = this.endPoint;
            resultMsg.destination = testrequest.source;
            resultMsg.type = "testresult";
            resultMsg.testkey = trs.testKey;
            return resultMsg;
        }



        [LoaderOptimizationAttribute(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        AppDomain ChildDomain()
        {
            AppDomainSetup domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase
                = "file:///" + System.Environment.CurrentDirectory;  // defines search path for LoadAndTest library
            Evidence adevidence = AppDomain.CurrentDomain.Evidence;
            AppDomain ad = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);
            Console.WriteLine("\n  creating {0} in ThreadID {1} - Req #4", ad.FriendlyName,Thread.CurrentThread.ManagedThreadId);
            return ad;
        }

        //----< Load and Test is responsible for testing >---------------

        ILoadAndTest installLoader(AppDomain ad)
        {
            ad.Load("LoadAndTest");
            ILoadAndTest landt=null;
            try
            {
                landt = (ILoadAndTest)ad.CreateInstanceAndUnwrap("LoadAndTest", "TestHarnessServer.AssemLoadAndTest");
                landt.setCallback(cb_);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //return new global::TestHarnessServer.AssemLoadAndTest();
            }
            return landt;
        }






        static void Main(string[] args)
        {
            TestHarness testHareness = new TestHarness();            
        }

    }

    public static class ExtMethods
    {
        public static void ShowXMLBody(this XDocument xdoc)
        {
            Console.WriteLine("  parsing message body ... ");
            foreach (var body in xdoc.Descendants("test"))
            {
                Console.WriteLine("test name : {0, 12}", body.Attribute("name").Value);
                Console.WriteLine("test code : {0, 12}", body.Element("testCode").Value);
                foreach (var tdr in body.Elements("testDriver"))
                {
                    Console.WriteLine("test driver: {0, 12}", tdr.Value);
                }
                Console.WriteLine("\n extract library files {0}, {1} from Repository", 
                        body.Element("testDriver").Value, body.Element("testCode").Value);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using MessageNameSpace;
using Communication;
using FileStreamService;
using System.ServiceModel;
using HRTimer;
using Interfaces;

namespace SecondClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ToSendPath = "..\\..\\ToSend";
        string SavePath = "..\\..\\SavedFiles";
        int BlockSize = 1024;
        byte[] block = new byte[1024];
        List<string> testKeyList = new List<string>();
        HRTimer.HiResTimer hrt = new HiResTimer();
        IStreamService fileProxy = CreateClientFileServiceChannel("http://localhost:8000/StreamService"); // for WCF file transfer
        ITestResults testResults = null;
        Client client; //for msg communication
        public MainWindow()
        {
            InitializeComponent();
            //----------as a host, listen to channel to receive test result msg ---------
            Console.Title = "client 2 demo";
            client = new Client(rcvThreadProc);

            //--------as a client, sending test request msg-------------------------
            for (int i = 0; i < 10; i++)
            {
                client.MakeMessage("Yaodong", "testrequest", client.endPoint,
                    CommService<Client>.makeEndPoint("http://localhost", 4040));
                client.comm.sndr.PostMessage(client.requestMsg);// put into sndBlockingQ, if there is any msg in the queue, a thread will deal with it.
                Thread.Sleep(400);
            }
        }

        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = client.comm.rcvr.GetMessage(); // get msg from rcvBlockingQ
                Console.WriteLine("\n\n  {0} received message:", client.comm.name);
                msg.ShowMsg();
                if (msg.body == "quit")
                    break;
                testKeyList.Add(msg.testkey);
                switch (msg.type)
                {
                    case "testresult":
                        // update GUI
                        testResults = Logging.Logger.XParse(msg.body);
                        /* called on asynch delegate's thread */
                        if (Dispatcher.CheckAccess())
                            ShowResult(msg);
                        else
                            Dispatcher.BeginInvoke(
                              new Action<Message>(ShowResult),
                              System.Windows.Threading.DispatcherPriority.Background,
                              msg
                            );
                        break;
                    default:
                        break;
                }
            }
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

        void ShowResult(Message testresultmsg)
        {
            string display = "author: " + testresultmsg.author + "  |  time: " + testresultmsg.time;

            foreach (var item in testResults.testResultList)
            {
                display += "\n test name = '";
                display += item.testName;
                display += "'";
                display += "\t |\t test result = '";
                display += item.testResult;
                display += "'";
                display += "\t |\t test log = '";
                display += item.testLog;
                display += "'";
            }
            listBox1.Items.Insert(0, display);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // event handler: upload -> DoWork ->action: DoUpload
            textBox.Text = "working on uploading...";
            System.Windows.Forms.OpenFileDialog ofdlg = new System.Windows.Forms.OpenFileDialog();
            ofdlg.InitialDirectory = ToSendPath;
            ofdlg.Filter = "dll|*.dll";
            if (ofdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ClientUploadFile(ofdlg.SafeFileName);
            }
            textBox.Text = "waiting for button click";
        }
        void ClientUploadFile(string filename)
        {
            hrt.Start();
            string fqname = System.IO.Path.Combine(ToSendPath, filename);
            using (var inputStream = new FileStream(fqname, FileMode.Open))
            {
                FileTransferMessage msg = new FileTransferMessage();
                msg.filename = filename;
                msg.transferStream = inputStream;
                fileProxy = CreateClientFileServiceChannel("http://localhost:8000/StreamService");
                int i = 3;
                while (i > 0)
                {
                    try
                    {
                        fileProxy.upLoadFile(msg);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nfileupload exception: {0}", ex.Message);
                        i--;
                        Thread.Sleep(500);
                    }
                }
            }
            hrt.Stop();
            Console.WriteLine("\n  Uploaded file \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            listBox2.Items.Insert(0, "uploaded file " + filename + " in " + hrt.ElapsedMicroseconds + " microsec");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            textBox.Text = "working on downloading ...";
            listBox2.Items.Clear();
            foreach (var log in testKeyList)
            {
                ClientDownload(log);
                string fqname = System.IO.Path.Combine(SavePath, log);
                try
                {
                    using (StreamReader sr = new StreamReader(fqname))
                    {
                        bool flag = true;
                        string line = null;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (flag) listBox3.Items.Insert(0, "downloading log:\n" + line); // read first line as log's title in listBox3
                            listBox2.Items.Insert(0, line);
                            flag = false;
                        }
                    }
                }
                catch { continue; }
            }
            textBox.Text = "waiting for button click";
        }
        void ClientDownload(string filename)
        {
            int totalBytes = 0;
            hrt.Start();
            try
            {
                Stream strm = fileProxy.downLoadFile(filename);
                string rfilename = System.IO.Path.Combine(SavePath, filename);
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);
                using (var outputStream = new FileStream(rfilename, FileMode.Create))
                {
                    while (true)
                    {
                        int bytesRead = strm.Read(block, 0, BlockSize);
                        totalBytes += bytesRead;
                        if (bytesRead > 0)
                            outputStream.Write(block, 0, bytesRead);
                        else
                            break;
                    }
                }
                hrt.Stop();
                ulong time = hrt.ElapsedMicroseconds;
                Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", filename, totalBytes, time);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}\n", ex.Message);
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            listBox2.Items.Clear();
            foreach (var file in Directory.GetFiles(SavePath))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        string line = null;
                        while ((line = sr.ReadLine()) != null)
                        {
                            listBox2.Items.Insert(0, line);
                        }
                    }
                }
                catch { continue; }
            }
        }
    }

    public class Client
    {
        public Message requestMsg;

        public CommService<Client> comm { get; set; }
        public string endPoint { get; }
        public Client(ThreadStart rcvThreadProc)
        {
            comm = new CommService<Client>();
            endPoint = CommService<Client>.makeEndPoint("http://localhost", 4038);
            comm.rcvr.CreateRecvChannel(endPoint);
            comm.rcvr.StartRcvThead(rcvThreadProc);
        }


        public void MakeMessage(string author, string type, string source, string desti)
        {
            string bodyStr =
                @"<test_request>
                    <test name='FirstTest'>
                        <testCode>tc1.dll</testCode>
                        <testDriver>td1.dll</testDriver>               
                    </test>
                    <test name='SecondTest'>
                        <testCode>tc2.dll</testCode>
                        <testDriver>td2.dll</testDriver>
                    </test>
                    <test name='ThirdTest'>
                        <testCode>tc1.dll</testCode>
                        <testDriver>td3.dll</testDriver>
                    </test>
                 </test_request>";
            requestMsg = new Message(bodyStr);
            requestMsg.author = author;
            requestMsg.type = type;
            requestMsg.source = source;
            requestMsg.destination = desti;
        }

    }
}

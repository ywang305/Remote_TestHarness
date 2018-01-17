///////////////////////////////////////////////////////////////////////
// StreamService.cs - WCF StreamService in Self Hosted Configuration //
//                                                                   //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Summer 2009 //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - Uses Programmatic configuration, no app.config file used.
 * - Uses ChannelFactory to create proxy programmatically. 
 * - Expects to find ToSend directory under application with files
 *   to send.
 * - Will create SavedFiles directory if it does not already exist.
 * - Users HRTimer.HiResTimer class to measure elapsed microseconds.
 * 
 * Required Files:
 * - HiResTimer.cs, interface.cs, logger.cs, message.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 19 Nov 2016
 * - HiRestimer is added to time meansure.
 *  
 * ver 1.0 : 16 Nov 2016
 * - first release 
 */


using System;
using System.IO;
using System.ServiceModel;
using HRTimer;
using Interfaces;
using System.Threading;


namespace FileStreamService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class StreamService : IStreamService
    {
        string filename;
        string savePath = "..\\..\\SavedFiles";
        string ToSendPath = "..\\..\\ToSend";
        int BlockSize = 1024;
        byte[] block;
        HRTimer.HiResTimer hrt = null;

        StreamService()
        {
            block = new byte[BlockSize];
            hrt = new HRTimer.HiResTimer();
        }

        public void upLoadFile(FileTransferMessage msg)
        {
            int totalBytes = 0;
            hrt.Start();
            filename = msg.filename;
            string rfilename = Path.Combine(savePath, filename);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                    totalBytes += bytesRead;
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }
            hrt.Stop();
            Console.Write(
              "\n  Received file \"{0}\" of {1} bytes in {2} microsec.",
              filename, totalBytes, hrt.ElapsedMicroseconds
            );
        }

        public Stream downLoadFile(string filename)
        {            
            hrt.Start();
            string sfilename = Path.Combine(ToSendPath, filename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
            {
                outStream = new FileStream(sfilename, FileMode.Open);
            }
            else
                try { throw new Exception("open failed for \"" + filename + "\""); }
                catch (Exception ex) { Console.WriteLine("\ndownload service exception: {0}", ex.Message); }
            hrt.Stop();
            Console.Write("\n  Sent \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            return outStream;
        }
        
        public void writeLog(MessageNameSpace.Message testresultmsg)
        {
            ITestResults body = Logging.Logger.XParse(testresultmsg.body);

            string logname = testresultmsg.testkey;
            string sfilename = Path.Combine(ToSendPath, logname);
            if (!Directory.Exists(ToSendPath))
                Directory.CreateDirectory(ToSendPath);
            while(true)
            {
                try
                {
                    using (TextWriter tw = File.CreateText(sfilename))
                    {
                        tw.WriteLine(testresultmsg.testkey);
                        tw.WriteLine(testresultmsg.author);
                        tw.WriteLine(testresultmsg.time);
                        foreach (var item in body.testResultList)
                        {
                            tw.WriteLine("test name  : " + item.testName);
                            tw.WriteLine("test result: " + item.testResult);
                            tw.WriteLine("test log   : " + item.testLog);
                        }
                    }
                    break;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("writeLog stream service exception: {0}", ex.Message);
                    Thread.Sleep(1000);
                }
            }
            
        }
        

        public static ServiceHost CreateHostServiceChannel(string url)
        {
            // Can't configure SecurityMode other than none with streaming.
            // This is the default for BasicHttpBinding.
            //   BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            //   BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 50000000;
            Uri baseAddress = new Uri(url);
            Type service = typeof(FileStreamService.StreamService);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IStreamService), binding, baseAddress);
            return host;
        }

        
    }

#if (Test_StreamService)
    class TestStreamService
    {
        static void Main(string[] args)
        {
            ServiceHost host = StreamService.CreateHostServiceChannel("http://localhost:8000/StreamService");

            host.Open();

            Console.Write("\n  SelfHosted File Stream Service started");
            Console.Write("\n ========================================\n");
            Console.Write("\n  Press key to terminate service:\n");
            Console.ReadKey();
            Console.Write("\n");
            host.Close();
        }
    }
#endif
}

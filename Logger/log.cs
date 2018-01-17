/////////////////////////////////////////////////////////////////////
// Logger.cs - logs test information                               //
//                                                                 //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Logger provides facilities for logging strings to an arbitrary 
 * number of streams, simultaneously.
 * 
 * The Package provides classes:
 * - Logger that provides all of the core logger functionality
 *   all instances of the class.
 * - Parse xml body for logger writing process.
 * 
 * Required Files:
 * ---------------
 * - Logger.cs, BlockingQueue.cs, Interface.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 19 Nov 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Interfaces;

namespace Logging
{
    public class TestResults: ITestResults
    {
        public string testKey { get; set; }
        public DateTime dateTime { get; set; }
        public List<ITestResult> testResultList { get; set; } = new List<ITestResult>();
    }
    public class TestResult: ITestResult
    {
        public string testName { get; set; }
        public string testResult { get; set; }
        public string testLog { get; set; }
    }

    public class Logger
    {
        static public TestResults XParse(string strBody)
        {
            XDocument xdoc = XDocument.Parse(strBody);
            TestResults tRsults = new TestResults();
            foreach (var body in xdoc.Descendants("test"))
            {
                TestResult resultitem = new TestResult();
                resultitem.testName = body.Attribute("name").Value;
                resultitem.testResult = body.Element("testResult").Value;
                resultitem.testLog = body.Element("testLog").Value;
                tRsults.testResultList.Add(resultitem);
            }
            return tRsults;
        }
    }
    class LoggerTest
    {
#if (TEST_LOGGER)
    static void Main(string[] args)
    {
      Logger logger = new Logger();
      logger.write("won't write this - not started");
      logger.start();
      StreamWriter sw = logger.makeConsoleStream();
      logger.attach(sw);
      sw = logger.makeFileStream("log.txt");
      logger.attach(sw);
      logger.title("Testing Loggers", true);
      logger.putLine();
      logger.title("Demonstrating Logger class");
      logger.write("\n  first visible log");
      logger.write("\n  second visible log");
      logger.write("\n");
      logger.flush();
      logger.showTimeStamp(true);
      logger.write("\n  pausing logger");
      logger.flush();
      logger.waitForKey();
      logger.write("\n    writing while unpaused");
      logger.write("\n    writing while unpaused");
      logger.write("\n    writing while unpaused");
      logger.write("\n    writing while unpaused");
      logger.flush();
      logger.pause(true);
      logger.write("\n    writing while paused");
      logger.write("\n    writing while paused");
      logger.write("\n    writing while paused");
      logger.write("\n    writing while paused");
      logger.waitForKey();
      logger.pause(false);
      logger.write("\n  unpausing logger");
      logger.flush();
      logger.putLine();
      logger.write("\n  writing after pause");
      logger.showTimeStamp(false);
      logger.stop();

      RLog.attach(RLog.makeConsoleStream());
      RLog.attach(RLog.makeFileStream("StaticLog.txt"));
      RLog.start();
      RLog.title("Demonstrating StaticLogger<ResultsLog>");
      RLog.write("\n  first line from static logger");
      RLog.write("\n  second line from static logger");
      RLog.write("\n\n");
      RLog.stop();

      LoggerTestDriver ltd = new LoggerTestDriver();
      if (ltd.test())
        Console.Write("logger test passed");
      else
        Console.Write("logger test failed");

      Console.Write("\n\n");
    }
#endif
    }
}

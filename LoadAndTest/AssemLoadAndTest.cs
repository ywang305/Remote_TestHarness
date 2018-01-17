/////////////////////////////////////////////////////////////////////
// LoadAndTest.cs - loads and executes tests using reflection      //
// ver 1.0                                                         //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * LoadAndTest package operates in child AppDomain.  It loads and
 * executes test code defined by a TestRequest message.
 *
 * Required files:
 * ---------------
 * - LoadAndTest.cs
 * - ITest.cs
 * - Logger, Messages
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 16 Oct 2016
 * - first release
 */
using System;
using System.Reflection;
using Interfaces;


namespace TestHarnessServer
{
    [Serializable]
    public class TestResult: ITestResult
    {
        public string testName { get; set; }
        public string testResult { get; set; }
        public string testLog { get; set; }
    }

    public class AssemLoadAndTest: MarshalByRefObject, ILoadAndTest
    {
        private ICallback cb_ = null;
        public void setCallback(ICallback cb)
        {
            cb_ = cb;
        }

        public void LoadTests(ITestInfo testinfomsg)
        {
            string[] files = System.IO.Directory.GetFiles(".","*.dll");
            foreach( string file in files)
            { 
                try
                {
                    Assembly assem = Assembly.LoadFrom(file);
                    Type[] types = assem.GetExportedTypes();
                    foreach (Type t in types)
                    {
                        string dllName = t.Name + ".dll";
                        if (dllName != testinfomsg.testDriver)
                        { 
                            continue;
                        }
                        Console.WriteLine("\n loading: \"{0}\"", file);
                        if (t.IsClass && typeof(ITest).IsAssignableFrom(t))  // does this type derive from ITest ?
                        { 
                            TestResult tr = new TestResult();
                            ITest tdr = (ITest)Activator.CreateInstance(t);    // create instance of test driver
                          
                            tr.testName = testinfomsg.testName;
                            tr.testResult = tdr.test() ? "passed" : "failed";
                            tr.testLog = tdr.getLog();

                            if (cb_ != null)
                            {
                                cb_.sendMessage(tr);
                            }
                        }  
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine("\n Execption: {0}", ex.Message);
                }
            }

            if (testinfomsg.testName == "ThirdTest")
            {
                TestResult tr = new TestResult();
                tr.testName = "ThirdTest";
                tr.testResult = "error";
                tr.testLog = "missing files: cannot read the test code or driver.";
                if (cb_ != null) cb_.sendMessage(tr);
            }

        }

             
    }

#if (TEST_LOADANDTEST)
    class TestIT
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Demonstrate loading and executing tests");
            Console.Write("\n =========================================");

            ITestInfo its=null;
            AssemLoadAndTest th = new AssemLoadAndTest();
            th.LoadTests(its);
            Console.Write("\n  couldn't load tests");

            Console.Write("\n\n");

            Console.ReadLine();
        }
    }
#endif
}

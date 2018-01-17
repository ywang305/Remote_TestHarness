/////////////////////////////////////////////////////////////////////
// TestDriver.cs - defines testing process                         //
//                                                                 //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace TestDrive2
{
    public class td2 : ITest
    {
        public string getLog()
        {
            return "demo test that always fails";
        }

        public bool test()
        {
            TestCode2.tc2 code = new TestCode2.tc2();
            return code.myWackyFunction();
        }
    }

#if (TEST_DRIVER2)
    public class TestMyDriver
    {
        public static void Main()
        {
            Console.WriteLine(new td2().test().ToString());
            Console.WriteLine( new td2().getLog());
        }
    }
#endif
}

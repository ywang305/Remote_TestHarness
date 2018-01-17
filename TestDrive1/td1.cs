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

namespace TestDrive1
{
    public class td1 : ITest
    {       
        public string getLog()
        {
                return "demo test that always passes";
        }

        public bool test()
        {
            TestCode1.tc1 code = new TestCode1.tc1();
            return code.myWackyFunction();
        }
    }

#if (TEST_DRIVER1)
    public class TestMyDriver
    {
        public static void Main()
        {
            Console.WriteLine(new td1().test().ToString());
            Console.WriteLine( new td1().getLog());
        }
    }
#endif
}

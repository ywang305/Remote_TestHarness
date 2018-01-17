using System;
using System.IO;
using System.Diagnostics;


namespace TestExecutive
{
    class TestExec
    {
        static void Main(string[] args)
        {
            string testHarnessServer = Path.GetFullPath("../../../TestHarenssServer/bin/Debug/TestHarenssServer.exe");
            string RepositoryServer = Path.GetFullPath("../../../RepositoryServer/bin/Debug/RepositoryServer.exe");
            string Clien1 = Path.GetFullPath("../../../Client/bin/Debug/Client.exe");
            string Clien2 = Path.GetFullPath("../../../AnotherClientEnd/bin/Debug/AnotherClientEnd.exe");

            Process.Start(testHarnessServer);
            Process.Start(RepositoryServer);
            Process.Start(Clien1);
            Process.Start(Clien2);
            
            
            //Process.Start(Clien2);
        }
    }
}

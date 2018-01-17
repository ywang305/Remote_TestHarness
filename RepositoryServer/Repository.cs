/////////////////////////////////////////////////////////////////////
// Repository.cs - holds test code for TestHarness                 //
//                                                                 //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * - WCF based service to provide file/stream stransfer
 * - Write file using streamwriter from test harness to repository
 * - extract dll library by test harness
 * - Queries for Logs and Libraries by clients.
 * 
 * Required Files:
 * - Client.cs, ITest.cs, Logger.cs, FileStreamService.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 19 Nov 2016
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;
using FileStreamService;

namespace RepositoryServer
{
    class Repository
    {
        ServiceHost host = null;
        public Repository()
        {
            Console.Title = "Repository Server";
            using (host = StreamService.CreateHostServiceChannel("http://localhost:8000/StreamService"))
            {
                while(true)
                {
                    try
                    {
                        host.Open();
                        Console.WriteLine("Start file transfer service at host Repository (http://localhost:8000/StreamService)\n***********************************");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nexecption when oepn service on host:", ex.Message);
                        Thread.Sleep(1000);
                    }
                }
                while (true)
                {

                }
            }// disposal host         
        }

        static void Main(string[] args)
        {
            Repository repo = new Repository();
        }
    }
}

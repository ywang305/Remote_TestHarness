﻿/////////////////////////////////////////////////////////////////////
// Interface.cs - interfaces for communication between system parts    //
//                                                                 //
// Yaodong Wang, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * ITest.cs provides interfaces:
 * - ITestHarness   used by TestExec and Client
 * - ICallback      used by child AppDomain to send messages to TestHarness
 * - IRequestInfo   used by TestHarness
 * - ITestInfo      used by TestHarness
 * - ILoadAndTest   used by TestHarness
 * - ITest          used by LoadAndTest
 * - IRepository    used by Client and TestHarness
 * - IClient        used by TestExec and TestHarness
 *
 * Required files:
 * ---------------
 * - ITest.cs, Message.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 1 Nov 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageNameSpace;

namespace Interfaces
{
  /////////////////////////////////////////////////////////////
  // used by child AppDomain to send messages to TestHarness

  public interface ICallback
  {
    void sendMessage(ITestResult msg);
        ITestResults GetMessage();
  }
  public interface ITestHarness
  {
    void setClient(IClient client);
    void sendTestRequest(Message testRequest);
    Message sendMessage(Message msg);
  }
  /////////////////////////////////////////////////////////////
  // used by child AppDomain to invoke test driver's test()

  public interface ITest
  {
    bool test();
    string getLog();
  }
  /////////////////////////////////////////////////////////////
  // used by child AppDomain to communicate with Repository
  // via TestHarness Comm

  public interface IRepository
  {
    bool getFiles(string path, string fileList);  // fileList is comma separated list of files
    void sendLog(string log);
    List<string> queryLogs(string queryText);
  }
  /////////////////////////////////////////////////////////////
  // used by child AppDomain to send results to client
  // via TestHarness Comm

  public interface IClient
  {
    void sendResults(Message result);
    void makeQuery(string queryText);
  }
  /////////////////////////////////////////////////////////////
  // used by TestHarness to communicate with child AppDomain

  public interface ILoadAndTest
  {
    void LoadTests (ITestInfo requestInfo);
    void setCallback(ICallback cb);
  }
  public interface ITestInfo
  {
    string testName { get; set; }
    string testCode { get; set; }
    string testDriver { get; set; }
  }
  public interface IRequestInfo
  {
    string tempDirName { get; set; }
    List<ITestInfo> requestInfo { get; set; }
  }
  public interface ITestResult
  {
    string testName { get; set; }
    string testResult { get; set; }
    string testLog { get; set; }
  }
  public interface ITestResults
  {
        string testKey { get; set; }
        DateTime dateTime { get; set; }
        List<ITestResult> testResultList { get; set; }
  }
}

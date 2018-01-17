/////////////////////////////////////////////////////////////////////
// ICommService.cs - Peer-To-Peer Communicator Service Contract   //
// ver 2.0                                                         //
// Yaodong Wang, CSE681 - Software Modeling & Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Maintenance History:
 * ====================
 * ver 2.0 : 10 Nov 16
 * - removed [OperationContract] from GetMessage() so only local client
 *   can dequeue messages
 * ver 1.0 : 28 Oct 16
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using MessageNameSpace;

namespace Communication
{
    [ServiceContract]
    public interface ICommService
    {
        [OperationContract(IsOneWay = true)]
        void PostMessage(Message msg);

        Message GetMessage();
    }
}

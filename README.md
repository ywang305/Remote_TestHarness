# Remote_TestHarness
> an implementation for accessing and using a remote Test Harness server from multiple concurrent clients. 


- The Test Harness will retrieve test code from a Repository server1.  
- One or more client(s) will concurrently supply the Test Harness with Test Requests. 
- The Test Harness will request test drivers and code to test, as cited in a Test Request, from the Repository. 
- One or more client(s) will concurrently extract Test Results and logs by enqueuing requests and displaying Test Harness and/or Repository replies2 when they arrive. 
- The TestHarness, Repository, and Clients are all distinct projects that compile to separate executables. 
- All communication between these processes will be based on message-passing Windows Communication Foundation (WCF) channels. 
- Client activities will be defined by user actions in a Windows Presentation Foundation (WPF) user interface. 
- On startup, Client, Repository, and TestHarness instances will demonstrate that all functional requirements are met with no input from the user.

#### 
![testharness](https://user-images.githubusercontent.com/24782000/36355054-2d2f0626-14ab-11e8-8bc0-899403584f7d.PNG)

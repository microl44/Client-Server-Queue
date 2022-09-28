# Readme
## Programmers
* José Gabriel Gomez Palomäki - a20gabpa
* Michael Rolén - a20micro 

## Language used
* Python - Version 3.9
* C# - Version 3.1

## Configured and executed
1. Server: Before the server is up and running, the user needs to input an ID, amount of seconds for timeout and which port to connect to. This is done through the console when the server starts.
2. Student and supervisor: The client for student and supervisor needs a name, IP or URL and a port. This 
is inputed in the three fields in the top left corner of the application window. If one of the fields is empty, a set of default values will be used when connecting to the server.

## Dependencies 
1. Python
* pyzmq - Installed by "pip install pyzmq"
2. C#
* NetMq \- Installed by "NuGet\Install-Package NetMQ -Version 4.0.1.9" with NuGet packet manager
* Newtonsoft.Json \- Installed by "NuGet\Install-Package Newtonsoft.Json -Version 13.0.1" with NuGet packet manager

## Reliability?
For the assignment, the second model is chosen, called the Brutal Shotgun Massacre. This works by letting the client send every message to every server using multiple sockets, one for every server. The client then process all responses from the servers but only use and display the first message. The client receives the responses by "polling" over sockets and firing correct event for the socket. If one server would go down during normal operation, the client would still continue as long as one socket receives messages.

However it is possible that some inconsistency exists between the servers and that clients could display different information. This could possible be solved by allowing communication between the servers, which is out of the scope for this assignment at the moment.

## Supervisor API
* Attend / join class

    Used to allow supervisor with a certain name to attend or leave class. Use true to attend and false to leave class.\
    Server to supervisor: No expected response

    {
        "attend": true,
        "name": "\<name\>"
    }

* Help / remove first student in queue

    Sent to server to remove the first student from the queue and send a message to said student.\
    Server to supervisor: No expected response\
    Server to student: Expected response *Queue Status*

    {
        "remove": true,
        "name": "\<name\>",
        "message": "\<message\>"
    }

## Updates biased on feedback
* Server:
    * Changed the ticket system to use a static ticket system instead of dynamic
    * Updated the "protocol_remove" to send an more appropriate message to student

* Student:
    * Updated the way the student handles queue updates
    * Added patch for problem with queue updates from local server

* Supervisor:
    * Removed functionality to only send message to first student due to inconveniences with a possible refactor
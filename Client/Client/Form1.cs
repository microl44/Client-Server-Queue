using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Client
{
    public partial class Form1 : Form
    {
        //constants to change timeout and second client port
        private const double TIMEOUT_LIMIT = 30;
        private const string SECOND_CLIENT_PORT = "5556";

        private static string currentStudentQueue = "";
        private static string currentSupervisorQueue = "";
        private static List<string> serverList = new List<string>();
        private static List<string> studentQueue = new List<string>();
        private static List<string> supervisorList = new List<string>();

        private static string ip;
        private static string port;
        private static string username;
        private static string lastAdminMessage = "";

        private static bool isConnected;
        private static bool isInQueue = false;
        private static bool firstMessage = true;

        private static System.Diagnostics.Stopwatch stopwatch;

        public Form1()
        {
            InitializeComponent();
            Connecter.socket = new DealerSocket();
            Connecter.socket1 = new DealerSocket();
            Connecter.messageToSend = new NetMQMessage();
            Connecter.messageToRecieve = new NetMQMessage();

            TBStudentQueue.ReadOnly = true;
            TBStudentQueue.ShortcutsEnabled = false;

            stopwatch = new System.Diagnostics.Stopwatch();

            Thread doSomething = new Thread(() => ThreadWork());
            doSomething.Start();

            Thread UpdateGUI = new Thread(() => UpdateQueueGUI());
            UpdateGUI.Start();
        }

        public class Connecter
        {
            public static DealerSocket socket;
            public static DealerSocket socket1;
            public static NetMQMessage messageToSend;
            public static NetMQMessage messageToRecieve;

            public static string textBoxText;
            public static string currentPlace;

        }

        //main function. Creates poller with two ports, each listens on port and processes msg when buffer isn't empty.
        //Restarts timeout stopwatch after a message arrives.
        public void ThreadWork()
        {
            while (true)
            {
                var poller = new NetMQPoller { Connecter.socket, Connecter.socket1 };
                {
                    Connecter.socket.ReceiveReady += (s, a) =>
                    {
                        // Recieve multipart message, encode and store into variable, parse message to json object.
                        Connecter.messageToRecieve = Connecter.socket.ReceiveMultipartMessage();

                        stopwatch.Restart();

                        var content = Encoding.UTF8.GetString(Connecter.messageToRecieve[0].Buffer);
                        System.Diagnostics.Debug.WriteLine(content);
                        JObject jsonContent = JObject.Parse(content);

                        HandleMessage(jsonContent);
                    };
                    Connecter.socket1.ReceiveReady += (s, a) =>
                    {
                        // Recieve multipart message, encode and store into variable, parse message to json object.
                        Connecter.messageToRecieve = Connecter.socket1.ReceiveMultipartMessage();

                        stopwatch.Restart();

                        var content = Encoding.UTF8.GetString(Connecter.messageToRecieve[0].Buffer);
                        System.Diagnostics.Debug.WriteLine(content);
                        JObject jsonContent = JObject.Parse(content);

                        HandleMessage(jsonContent);
                    };
                };
                poller.Run();
            };
        }

        //main functionality for interpreting each message. Runs different functionality depending on what array keys exists in json object.
        public void HandleMessage(JObject jsonContent)
        {
            //if it's the first message and contains only serverId, it means some heartbeat got missdirected. Ignore it and listen for new message.
            if (firstMessage == true && jsonContent.Count == 1 && jsonContent.ContainsKey("serverId"))
            {
                firstMessage = false;
                return;
            }

            // is heartbeat. Send heartbeat back
            if (firstMessage == false && jsonContent.Count == 1 && jsonContent.ContainsKey("serverId"))
            {
                string heartbeat = "{}";
                SendMessage(heartbeat);
                System.Diagnostics.Debug.WriteLine("heartbeat recieved and sent");
                return;
            }

            // if message is ticket response, add serverId to list of servers and update the users current place. WIP.
            if (jsonContent.ContainsKey("ticket") && jsonContent.ContainsKey("name") && jsonContent.ContainsKey("serverId"))
            {
                if (!serverList.Contains(jsonContent.GetValue("serverId").ToString()))
                {
                    serverList.Add(jsonContent.GetValue("serverId").ToString());
                }

                firstMessage = false;
                Connecter.currentPlace = jsonContent.GetValue("ticket").ToString();
            }

            // if message is a queue update, extract the queue and add client if client doesn't exist in client list.
            else if (jsonContent.ContainsKey("queue") && jsonContent.ContainsKey("supervisors") && jsonContent.ContainsKey("serverId"))
            {
                if (firstMessage)
                {
                    serverList.Remove("temp");
                }
                if (!serverList.Contains(jsonContent.GetValue("serverId").ToString()))
                {
                    serverList.Add(jsonContent.GetValue("serverId").ToString());
                }

                firstMessage = false;

                extractQueue(jsonContent, "student");
                extractQueue(jsonContent, "supervisor");
            }

            // admin response, update text string determening admin msg textbox.
            else if (jsonContent.ContainsKey("name") && jsonContent.ContainsKey("clientMessage") && jsonContent.ContainsKey("serverId"))
            {
                lastAdminMessage = jsonContent.GetValue("clientMessage").ToString();
            }
        }

        // send every message to both ports.
        public void SendMessage(string MessageToSend)
        {
            Connecter.socket.SendFrame(MessageToSend);
            Connecter.socket1.SendFrame(MessageToSend);
            stopwatch.Start();
        }

        public void ShowMessage(string windowTitle, string message)
        {
            Form popupForm = new Form();
            RichTextBox popupFormText = new RichTextBox();
            popupFormText.Width = popupForm.Width - 5;
            popupFormText.Height = popupForm.Height;

            popupForm.Text = windowTitle;
            popupFormText.Text = message;
            popupFormText.ReadOnly = true;
            popupForm.Controls.Add(popupFormText);

            popupForm.Show();
        }

        // Establishes a connection if none exists. If connection exists, update username based on textbox.
        // Responsible for showing proper error message to user in case of faulty input.
        public bool FetchInfo()
        {
            if (isConnected)
            {
                username = textBox1.Text;
            }
            if (!isConnected)
            {
                ip = TBIP.Text;
                port = TBPort.Text;
                username = textBox1.Text;

                // if field empty
                if (port == "" || TBPort.Text == "" || username == "")
                {
                    ShowMessage("Empty Field Error", "One or more textfields were left empty. Using the default config which is IP = 'LOCALHOST' PORT = '5555' USERNAME = 'Micke.");

                    ip = "localhost";
                    port = "5555";
                    username = "Micke";
                }

                // if port out of range
                else if (Int64.Parse(port) > 65535)
                {
                    ShowMessage("PORT out of bounds error", "Port must be a valid number between 1 and 65355.");

                    isConnected = false;
                    return false;
                }

                Connecter.socket.Connect("tcp://" + ip + ":" + port);
                Connecter.socket1.Connect("tcp://" + ip + ":" + SECOND_CLIENT_PORT);
                System.Diagnostics.Debug.WriteLine("connected to both");
                isConnected = true;

                return true;
            }
            return true;
        }

        // updates corresponding text-box based on latest fetched values.
        public void changeTextBox(string stringToAdd, string type)
        {
            if (type == "student")
            {
                try
                {
                    TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = "---Current Student Queue--- \n " + stringToAdd));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
            else if (type == "supervisor")
            {
                try
                {
                    TBSupervisorQueue.Invoke((MethodInvoker)(() => TBSupervisorQueue.Text = "---Current Supervisors--- \n " + stringToAdd));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
            else if (type == "admin_message")
            {
                TBAdminMessage.Invoke((MethodInvoker)(() => TBAdminMessage.Text = "---Last Admin Message--- \n " + stringToAdd));
            }
        }

        // resets text boxes via MethodInvoker as it's cross-thread communication.
        public void clearTextBox()
        {
            TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = "---Current Student Queue---"));
            TBSupervisorQueue.Invoke((MethodInvoker)(() => TBSupervisorQueue.Text = "---Current Supervisors---"));
            TBAdminMessage.Invoke((MethodInvoker)(() => TBAdminMessage.Text = "---Last Admin Message--"));
        }

        // extracts queue values from json object.
        public void extractQueue(JObject Jmessage, string type)
        {
            // for ticket queue
            if (Jmessage.ContainsKey("queue") && type == "student")
            {
                currentStudentQueue = "";
                studentQueue.Clear();

                foreach (var x in Jmessage.GetValue("queue"))
                {
                    studentQueue.Add(x["ticket"].ToString() + " ");
                    studentQueue.Add(x["name"].ToString() + " \n ");
                }

                foreach (var s in studentQueue)
                {
                    currentStudentQueue += s;
                }
            }

            // for supervisor queue
            else if (Jmessage.ContainsKey("supervisors") && type == "supervisor")
            {
                currentSupervisorQueue = "";
                supervisorList.Clear();

                foreach (var x in Jmessage.GetValue("supervisors"))
                {
                    supervisorList.Add(x["name"].ToString() + " ");
                    supervisorList.Add(x["status"].ToString() + " ");

                    if (x["status"].ToString() == "")
                    {
                        foreach (var y in Jmessage.GetValue("supervisors"))
                        {
                            supervisorList.Add(y["name"].ToString() + " ");
                        }
                    }
                    //supervisorList.Add(x["clientMessage"].ToString() + " \n ");
                }

                // string builder for the final string used to update text-box
                foreach (var x in supervisorList)
                {
                    currentSupervisorQueue += x;
                }
            }
        }

        // Update text-boxes if connection timed out.
        public void ConnectionTimeOut()
        {
            TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = "CONNECTION TIMED OUT"));
            TBSupervisorQueue.Invoke((MethodInvoker)(() => TBSupervisorQueue.Text = "CONNECTION TIMED OUT"));
            TBAdminMessage.Invoke((MethodInvoker)(() => TBAdminMessage.Text = "CONNECTION TIMED OUT"));
        }

        // Updates GUI every second. Sleep 1 second for main form controller to have time to build.
        // If time passes TIMEOUT_LIMIT, raise error flag and put system in error mode until an incoming message resumes the process.
        public void UpdateQueueGUI()
        {
            Thread.Sleep(1000);
            while (true)
            {
                double timeElapsed = stopwatch.Elapsed.TotalSeconds;
                if(timeElapsed >= TIMEOUT_LIMIT)
                {
                    ConnectionTimeOut();
                }

                Thread.Sleep(1000);
                if (isInQueue)
                {
                    changeTextBox(currentStudentQueue, "student");
                    changeTextBox(currentSupervisorQueue, "supervisor");
                    if(lastAdminMessage != "")
                    {
                        changeTextBox(lastAdminMessage, "admin_message");
                    }
                }
                else if (!isInQueue)
                {
                    changeTextBox("", "student");
                    changeTextBox("", "supervisor");
                }
            }
        }

        // following clicks are self-explanatory. Simply send message to server. Read function name for explanation.
        private void BtnEnterQueue_Click(object sender, EventArgs e)
        {
            if (FetchInfo())
            {
                string enterQueueTicket = "{\"enterQueue\":true,\"name\":\"" + username + "\"}";
                SendMessage(enterQueueTicket);
            }
        }

        private void BtnSubscribe_Click(object sender, EventArgs e)
        {
            if (FetchInfo())
            {
                string subscribeToQueue = "{\"subscribe\":true}";
                SendMessage(subscribeToQueue);
                isInQueue = true;
            }
        }

        private void BtnLeaveQueue(object sender, EventArgs e)
        {
            if (isConnected)
            {
                string RemovesubscribeToQueue = "{\"subscribe\":false}";
                SendMessage(RemovesubscribeToQueue);
                clearTextBox();
                isInQueue = false;
            }
        }
    }
}
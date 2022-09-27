using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace SupervisorClient
{
    public partial class Form1 : Form
    {
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

        private static bool isConnected;
        private static bool isInQueue = false;
        private static bool firstMessage = true;
        private static System.Diagnostics.Stopwatch stopwatch;

        private DateTime lastCalled;
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

            public static string currentPlace;
            public static string textBoxText;
        }

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

                        var content = Encoding.UTF8.GetString(Connecter.messageToRecieve[0].Buffer);
                        System.Diagnostics.Debug.WriteLine(content);
                        JObject jsonContent = JObject.Parse(content);

                        HandleMessage(jsonContent);
                    };
                };
                poller.Run();
            };
        }

        public void HandleMessage(JObject jsonContent)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine(jsonContent.ToString());
                //if it's the first message and contains only serverId, it means some heartbeat got missdirected. Ignore it and listen for new message.
                if (firstMessage == true && jsonContent.Count == 1 && jsonContent.ContainsKey("serverId"))
                {
                    firstMessage = false;
                    return;
                }

                if (firstMessage == false && jsonContent.Count == 1 && jsonContent.ContainsKey("serverId"))
                {
                    string heartbeat = "{}";
                    SendMessage(heartbeat);
                    System.Diagnostics.Debug.WriteLine("heartbeat recieved and sent");
                    return;
                }

                // if message is ticket response, add serverId to list of servers and update the users current place.
                if (jsonContent.ContainsKey("ticket") && jsonContent.ContainsKey("name") && jsonContent.ContainsKey("serverId"))
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
                    Connecter.currentPlace = jsonContent.GetValue("ticket").ToString();
                }
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
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return;
            }
        }
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

        public bool CreateConnection()
        {
            if(isConnected)
            {
                username = textBox1.Text;
            }
            if (!isConnected)
            {
                ip = TBIP.Text;
                port = TBPort.Text;
                username = textBox1.Text;

                if (port == "" || TBPort.Text == "" || username == "")
                {
                    ShowMessage("Empty Field Error", "One or more textfields were left empty. Using the default config which is IP = 'LOCALHOST' PORT = '5555' USERNAME = 'Micke.");

                    ip = "localhost";
                    port = "5555";
                    username = "Micke";
                }
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
        public void changeTextBox(string stringToAdd, string type)
        {
            if(type == "student")
            {
                try
                {
                    TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = "---Current Student Queue--- \n " + stringToAdd));
                }
                catch(Exception e)
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
        }

        public void clearTextBox()
        {
            TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = "---Current Student Queue---"));
        }

        public void extractQueue(JObject Jmessage, string type)
        {
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
            else if (type == "supervisor")
            {
                currentSupervisorQueue = "";
                supervisorList.Clear();

                foreach (var x in Jmessage.GetValue("supervisors"))
                {
                    supervisorList.Add(x["name"].ToString() + " ");
                    supervisorList.Add(x["status"].ToString() + " ");

                    if(x["status"].ToString( )== "")
                    {
                        foreach (var y in Jmessage.GetValue("supervisors"))
                        {
                            supervisorList.Add(y["name"].ToString() + " ");
                        }
                    }
                    //supervisorList.Add(x["clientMessage"].ToString() + " \n ");
                }

                foreach (var x in supervisorList)
                {
                    currentSupervisorQueue += x;
                }
            }
        }

        public void ConnectionTimeOut()
        {
            TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = "CONNECTION TIMED OUT"));
            TBSupervisorQueue.Invoke((MethodInvoker)(() => TBSupervisorQueue.Text = "CONNECTION TIMED OUT"));
        }
        public void UpdateQueueGUI()
        {
            Thread.Sleep(3000);
            while (true)
            {
                double timeElapsed = stopwatch.Elapsed.TotalSeconds;
                if (timeElapsed >= TIMEOUT_LIMIT)
                {
                    ConnectionTimeOut();
                }

                Thread.Sleep(1000);
                if (isInQueue)
                {
                    changeTextBox(currentStudentQueue, "student");
                    changeTextBox(currentSupervisorQueue, "supervisor");
                }
                else if (!isInQueue)
                {
                    changeTextBox("", "student");
                    changeTextBox("", "supervisor");
                }
            }
        }

        private void BtnEnterQueue_Click(object sender, EventArgs e)
        {
            if (CreateConnection())
            {
                string attendTicket = "{\"attend\":true,\"name\":\"" + username + "\"}";
                SendMessage(attendTicket);
            }
        }

        private void BtnSubscribe_Click(object sender, EventArgs e)
        {
            if (CreateConnection())
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
                currentStudentQueue = "";
                isInQueue = false;
            }
        }

        private void BtnRemoveFirst(object sender, EventArgs e)
        {
            if (CreateConnection())
            {
                string message = richTextBoxMessageToSend.Text;
                string enterQueueTicket = "{\"remove\":true,\"name\":\"" + username + "\", \"message\":\"" + message + "\"}";
                SendMessage(enterQueueTicket);
            }
        }

        private void BtnSendMessageToFirst(object sender, EventArgs e)
        {
            if (CreateConnection())
            {
                string message = richTextBoxMessageToSend.Text;
                string enterQueueTicket = "{\"remove\":false,\"name\":\"" + username + "\", \"message\":\"" + message + "\"}";
                SendMessage(enterQueueTicket);
            }
        }

        private void BtnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnStopSupervising(object sender, EventArgs e)
        {
            username = textBox1.Text;
            string attendTicket = "{\"attend\":false,\"name\":\"" + username + "\"}";
            SendMessage(attendTicket);
            System.Diagnostics.Debug.WriteLine("Should not be in supervisor queue anymore");
        }
    }
}
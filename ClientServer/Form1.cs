﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SupervisorClient
{
    public partial class Form1 : Form
    {
        public static string lastKnownStudentQueue = "";
        public static string currentStudentQueue = "";
        private static string currentSupervisorQueue = "";
        private static List<string> serverList = new List<string>();
        private static List<string> studentQueue = new List<string>();
        private static List<string> supervisorList = new List<string>();

        private static string ip;
        private static string port;
        private static string username;

        private static int heartbeatCount = 0;

        private static bool isConnected;
        private static bool isInQueue;
        private static bool firstMessage = true;
        public Form1()
        {
            InitializeComponent();
            Connecter.socket = new DealerSocket();
            Connecter.messageToSend = new NetMQMessage();
            Connecter.messageToRecieve = new NetMQMessage();
            Connecter.asyncSocket = new NetMQRuntime();

            TBStudentQueue.ReadOnly = true;
            TBStudentQueue.ShortcutsEnabled = false;

            Thread doSomething = new Thread(() => ThreadWork());
            doSomething.Start();

            Thread UpdateGUI = new Thread(() => UpdateQueueGUI());
            UpdateGUI.Start();
        }

        public class Connecter
        {
            public static DealerSocket socket;
            public static NetMQMessage messageToSend;
            public static NetMQMessage messageToRecieve;
            public static NetMQRuntime asyncSocket;

            public static string currentPlace;
            public static string textBoxText;
        }

        public void ThreadWork()
        {
            Form popupForm = new Form();
            RichTextBox popupFormText = new RichTextBox();
            popupFormText.Width = popupForm.Width - 5;
            popupFormText.Height = popupForm.Height;

            while (true)
            {
                // Recieve multipart message, encode and store into variable, parse message to json object.
                Connecter.messageToRecieve = Connecter.socket.ReceiveMultipartMessage();
                var content = Encoding.UTF8.GetString(Connecter.messageToRecieve[0].Buffer);
                System.Diagnostics.Debug.WriteLine(content);
                JObject jsonContent = JObject.Parse(content);

                //if it's the first message and contains only serverId, it means some heartbeat got missdirected. Ignore it and listen for new message.
                if(firstMessage == true && jsonContent.Count == 1 && jsonContent.ContainsKey("serverId"))
                {
                    firstMessage = false;
                    continue;
                }

                if (firstMessage == false && jsonContent.Count == 1 && jsonContent.ContainsKey("serverId"))
                {
                    heartbeatCount++;
                    if (heartbeatCount < 5)
                    {
                        string heartbeat = "{}";
                        SendMessage(heartbeat);
                        System.Diagnostics.Debug.WriteLine("heartbeat recieved and sent");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Heartbeat has ended...");
                    }
                    continue;
                }

                // if message is ticket response, add serverId to list of servers and update the users current place.
                if (jsonContent.ContainsKey("ticket") && jsonContent.ContainsKey("name") && jsonContent.ContainsKey("serverId"))
                {
                    if(firstMessage)
                    {
                        if(!serverList.Contains(jsonContent.GetValue("serverId").ToString()))
                        {
                            serverList.Add(jsonContent.GetValue("serverId").ToString());
                        }
                        firstMessage = false;
                    }
                    Connecter.currentPlace = jsonContent.GetValue("ticket").ToString();
                }
                else if (jsonContent.ContainsKey("queue") && jsonContent.ContainsKey("supervisors") && jsonContent.ContainsKey("serverId"))
                {
                    if (firstMessage)
                    {
                        if (!serverList.Contains(jsonContent.GetValue("serverId").ToString()))
                        {
                            serverList.Add(jsonContent.GetValue("serverId").ToString());
                        }
                        firstMessage = false;
                    }
                    extractQueue(jsonContent, "student");
                }
                else if (jsonContent.ContainsKey("remove") && jsonContent.ContainsKey("message") && jsonContent.ContainsKey("serverId"))
                {
                    popupForm.Text = "Message from Supervisor";
                    popupFormText.Text = jsonContent.GetValue("remove").ToString();
                    popupFormText.ReadOnly = true;
                    popupForm.Controls.Add(popupFormText);
                    popupForm.Show(this);
                }
            }
        }

        public void SendMessage(string MessageToSend)
        {
            Connecter.socket.SendFrame(MessageToSend);
        }

        public bool FetchInfo()
        {
            if (isConnected == false)
            {
                Form popupForm = new Form();
                RichTextBox popupFormText = new RichTextBox();
                popupFormText.Width = popupForm.Width - 5;
                popupFormText.Height = popupForm.Height;

                ip = TBIP.Text;
                port = TBPort.Text;
                username = textBox1.Text;

                if (port == "" || TBPort.Text == "" || username == "")
                {
                    popupFormText.Text = "One or more textfields were left empty. Using the default config which is IP = 'LOCALHOST' PORT = '5555' USERNAME = 'Micke.";
                    popupForm.Text = "Empty Field Error";
                    popupFormText.ReadOnly = true;
                    popupForm.Controls.Add(popupFormText);
                    popupForm.Show(this);

                    ip = "localhost";
                    port = "5555";
                    username = "Micke";
                }
                else if (Int64.Parse(port) > 65535)
                {
                    popupFormText.Text = "Port number is too large. Please enter a port number between 1 and 65535";
                    popupForm.Text = "Port Error";
                    popupFormText.ReadOnly = true;
                    popupForm.Controls.Add(popupFormText);
                    popupForm.Show(this);

                    isConnected = false;
                    return false;
                }

                Connecter.socket.Connect("tcp://" + ip + ":" + port);
                isConnected = true;

                return true;
            }
            return true;
        }
        public void changeTextBox(string stringToAdd)
        {
            TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = "---Current Student Queue--- \n " + stringToAdd));

            foreach (string s in studentQueue)
            {
                System.Diagnostics.Debug.WriteLine(s);
            }
        }

        public void clearTextBox()
        {
            TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = ""));
        }

        public void extractQueue(JObject Jmessage, string type)
        {
            if (Jmessage.ContainsKey("queue") && type == "student")
            {
                System.Diagnostics.Debug.WriteLine("updates queue 2 from within type == student");
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
                supervisorList.Clear();
                currentSupervisorQueue = "";

                foreach (var x in Jmessage.GetValue("supervisor"))
                {
                    supervisorList.Add(x["name"].ToString() + " ");
                    supervisorList.Add(x["status"].ToString() + " \n ");
                }

                foreach (var x in supervisorList)
                {
                    currentSupervisorQueue += x;
                }
            }
        }
        public void UpdateQueueGUI()
        {
            currentStudentQueue = "";
            Thread.Sleep(3000);
            while (true)
            {
                if (lastKnownStudentQueue != currentStudentQueue && isInQueue)
                {
                    changeTextBox(currentStudentQueue);
                    lastKnownStudentQueue = currentStudentQueue;
                }
            }
        }

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
                Thread.Sleep(500);
                string RemovesubscribeToQueue = "{\"subscribe\":false}";
                SendMessage(RemovesubscribeToQueue);
                clearTextBox();
                currentStudentQueue = "";
                isInQueue = false;
            }
        }

        private void BtnRemoveFirst(object sender, EventArgs e)
        {
            if (FetchInfo())
            {
                string message = richTextBoxMessageToSend.Text;
                string enterQueueTicket = "{\"remove\":true,\"message\":\"" + message + "\"}";
                SendMessage(enterQueueTicket);
            }
        }

        private void BtnSendMessageToFirst(object sender, EventArgs e)
        {
            if(FetchInfo())
            {
                string message = richTextBoxMessageToSend.Text;
                string enterQueueTicket = "{\"remove\":false,\"message\":\"" + message + "\"}";
                SendMessage(enterQueueTicket);
            }
        }
    }
}
using System;
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

namespace ClientGUI
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
        private static bool isConnected;
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

            Thread UpdateGUI = new Thread(() => ThreadWork2ElectricBogaloo());
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

        private void BtnEnterQueue_Click(object sender, EventArgs e)
        {
            if(FetchInfo())
            {
                string enterQueueTicket = "{\"enterQueue\":true,\"name\":\"" + username + "\"}";
                SendMessage(enterQueueTicket);
            }
        }

        private void BtnSubscribe_Click(object sender, EventArgs e)
        {
            if(FetchInfo())
            {
                string subscribeToQueue = "{\"subscribe\":true}";
                SendMessage(subscribeToQueue);
            }
        }

        public void SendMessage(string MessageToSend)
        {
            Connecter.socket.SendFrame(MessageToSend);
        }

        public bool FetchInfo()
        {
            Form popupForm = new Form();
            RichTextBox popupFormText = new RichTextBox();
            popupFormText.Width = popupForm.Width - 5;
            popupFormText.Height = popupForm.Height;

            ip = TBIP.Text;
            port = TBPort.Text;
            username = textBox1.Text;

            if (ip == "" || port == "" || TBPort.Text == "" || username == "test")
            {
                popupFormText.Text = "One or more textfields were left empty. Please provide appropriate values to each config box.";
                popupForm.Text = "Empty Field Error";
                popupFormText.ReadOnly = true;
                popupForm.Controls.Add(popupFormText);
                popupForm.Show(this);

                isConnected = false;
                return false;
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

            if(username == "")
            {
                username = "UNK";
            }
            Connecter.socket.Connect("tcp://" + ip + ":" + port);
            isConnected = true;

            return true;
        }

        public void ThreadWork()
        {
            while (true)
            {
                Connecter.messageToRecieve = Connecter.socket.ReceiveMultipartMessage();
                var content = Encoding.UTF8.GetString(Connecter.messageToRecieve[0].Buffer);

                JObject jsonContent = JObject.Parse(content);
                System.Diagnostics.Debug.WriteLine(jsonContent);

                if (jsonContent.ContainsKey("ticket"))
                {
                    Connecter.currentPlace = jsonContent.GetValue("ticket").ToString();
                }
                else if (jsonContent.ContainsKey("queue"))
                {
                    extractQueue(jsonContent, "student");
                    System.Diagnostics.Debug.WriteLine("updates queue");
                }
                else if (jsonContent.ContainsKey("remove"))
                {
                    Form popupForm = new Form();
                    popupForm.Text = jsonContent.GetValue("remove").ToString();
                    popupForm.Show(this);
                }
                /*else if(jsonContent.Count == 1)
                {
                    string heartbeat = "{}";
                    SendMessage(heartbeat);
                }*/
            }
        }

        public void changeTextBox(string stringToAdd)
        {
            TBStudentQueue.Invoke((MethodInvoker)(() => TBStudentQueue.Text = "---Current Student Queue--- \n " + stringToAdd));

            foreach(string s in studentQueue)
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
            if(Jmessage.ContainsKey("queue") && type == "student")
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
            else if(type == "supervisor")
            {
                supervisorList.Clear();
                currentSupervisorQueue = "";

                foreach(var x in Jmessage.GetValue("supervisor"))
                {
                    supervisorList.Add(x["name"].ToString() + " ");
                    supervisorList.Add(x["status"].ToString() + " \n ");
                }

                foreach(var x in supervisorList)
                {
                    currentSupervisorQueue += x;
                }
            }

        }

        public void ThreadWork2ElectricBogaloo()
        {
            currentStudentQueue = "";
            
            Thread.Sleep(3000);
            while (true)
            {
                if (lastKnownStudentQueue != currentStudentQueue)
                {
                    foreach(string s in studentQueue)
                    {
                        currentStudentQueue += s;
                    }
                    changeTextBox(currentStudentQueue);
                    lastKnownStudentQueue = currentStudentQueue;
                }
            }
        }

        private void BtnLeaveQueue(object sender, EventArgs e)
        {
            if (isConnected == true)
            {
                Thread.Sleep(500);
                string RemovesubscribeToQueue = "{\"subscribe\":false}";
                SendMessage(RemovesubscribeToQueue);
                clearTextBox();
                currentStudentQueue = "";
            }
        }
    }
}
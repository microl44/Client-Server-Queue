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
        public static string CurrentQueue;
        public Form1()
        {
            InitializeComponent();
            Connecter.socket = new DealerSocket();
            Connecter.messageToSend = new NetMQMessage();
            Connecter.messageToRecieve = new NetMQMessage();
            Connecter.asyncSocket = new NetMQRuntime();


            Connecter.socket.Connect("tcp://localhost:5555");


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
            string username = textBox1.Text;
            if(username == null || username == "")
            {
                username = "Micke";
            }

            string enterQueueTicket = "{\"enterQueue\":true,\"name\":\"" + username + "\"}";

            SendMessage(enterQueueTicket);
        }

        private void BtnSubscribe_Click(object sender, EventArgs e)
        {
            string subscribeToQueue = "{\"subscribe\":true}";
            SendMessage(subscribeToQueue);
        }

        public void SendMessage(string MessageToSend)
        {
            Connecter.socket.SendFrame(MessageToSend);
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
                    extractQueue(jsonContent);
                }
                /*else if(jsonContent.Count < 1)
                {
                    string heartbeat = "{}";
                    SendMessage(heartbeat);
                }*/
            }
        }

        public void changeTextBox()
        {
            richTextBox1.Invoke((MethodInvoker)(() => richTextBox1.Text = CurrentQueue));
        }

        public void clearTextBox()
        {
            richTextBox1.Invoke((MethodInvoker)(() => richTextBox1.Text = ""));
        }

        public string extractQueue(JObject Jmessage)
        {
            CurrentQueue = "";
            List<string> tempList = new List<string>();
            if(Jmessage.ContainsKey("queue"))
            {
                foreach (var x in Jmessage.GetValue("queue"))
                {
                    tempList.Add(x["ticket"].ToString() + " ");
                    tempList.Add(x["name"].ToString() + "\n");
                }

                foreach (string s in tempList)
                {
                    CurrentQueue = CurrentQueue + s;
                }
            }
            return null;
        }

        public void ThreadWork2ElectricBogaloo()
        {
            Thread.Sleep(4000);
            string lastSavedText = "";
            while (true)
            {
                if (lastSavedText != CurrentQueue)
                {
                    lastSavedText = CurrentQueue;
                    changeTextBox();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string RemovesubscribeToQueue = "{\"subscribe\":false}";
            SendMessage(RemovesubscribeToQueue);
            clearTextBox();
            CurrentQueue = "";
        }
    }
}

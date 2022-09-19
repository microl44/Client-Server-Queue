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
        public static string CurrentQueue = "hello!";
        public Form1()
        {
            InitializeComponent();
            Connecter.socket = new DealerSocket();
            Connecter.messageToSend = new NetMQMessage();
            Connecter.messageToRecieve = new NetMQMessage();
            Connecter.asyncSocket = new NetMQRuntime();


            Connecter.socket.Connect("tcp://localhost:5555");

            RichTextBox richBox = richTextBox1;

            Thread doSomething = new Thread(() => ThreadWork(richBox));
            doSomething.Start();
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

        public void ThreadWork(RichTextBox boxToUpdate)
        {
            while (true)
            {
                Connecter.messageToRecieve = Connecter.socket.ReceiveMultipartMessage();
                var content = Encoding.UTF8.GetString(Connecter.messageToRecieve[0].Buffer);

                JObject jsonContent = JObject.Parse(content);

                if (jsonContent.ContainsKey("ticket"))
                {
                    Connecter.currentPlace = jsonContent.GetValue("ticket").ToString();
                    System.Diagnostics.Debug.WriteLine(Connecter.currentPlace);
                    Connecter.textBoxText = jsonContent.GetValue("ticket").ToString();
                }
                else if (jsonContent.ContainsKey("queue"))
                {
                    System.Diagnostics.Debug.WriteLine(jsonContent.GetValue("queue").ToString());
                }
                else if (jsonContent.ContainsKey(null))

                foreach (var pair in jsonContent)
                {
                    System.Diagnostics.Debug.WriteLine(pair);
                }
            }
        }

        public void changeTextBox()
        {
            richTextBox1.Text = CurrentQueue;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void BtnUpdateUI(object sender, EventArgs e)
        {
            richTextBox1.Text = CurrentQueue;
        }
    }
}

namespace ClientGUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.TBStudentQueue = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.TBIP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TBPort = new System.Windows.Forms.TextBox();
            this.TBSupervisorQueue = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 157);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(352, 96);
            this.button1.TabIndex = 0;
            this.button1.Text = "Connect To Server";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.BtnEnterQueue_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(120, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(211, 20);
            this.textBox1.TabIndex = 1;
            // 
            // TBStudentQueue
            // 
            this.TBStudentQueue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.TBStudentQueue.Location = new System.Drawing.Point(370, 29);
            this.TBStudentQueue.Name = "TBStudentQueue";
            this.TBStudentQueue.Size = new System.Drawing.Size(352, 163);
            this.TBStudentQueue.TabIndex = 2;
            this.TBStudentQueue.TabStop = false;
            this.TBStudentQueue.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Enter Queue Alias:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(11, 258);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(173, 96);
            this.button2.TabIndex = 4;
            this.button2.Text = "Subscribe To Queue";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.BtnSubscribe_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(191, 258);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(173, 96);
            this.button4.TabIndex = 6;
            this.button4.Text = "Unsubscribe from Queue";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.BtnLeaveQueue);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Enter Server IP/URL:";
            // 
            // TBIP
            // 
            this.TBIP.Location = new System.Drawing.Point(120, 31);
            this.TBIP.Name = "TBIP";
            this.TBIP.Size = new System.Drawing.Size(211, 20);
            this.TBIP.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Enter PORT:";
            // 
            // TBPort
            // 
            this.TBPort.Location = new System.Drawing.Point(120, 56);
            this.TBPort.Name = "TBPort";
            this.TBPort.Size = new System.Drawing.Size(211, 20);
            this.TBPort.TabIndex = 10;
            // 
            // TBSupervisorQueue
            // 
            this.TBSupervisorQueue.Location = new System.Drawing.Point(369, 198);
            this.TBSupervisorQueue.Name = "TBSupervisorQueue";
            this.TBSupervisorQueue.ReadOnly = true;
            this.TBSupervisorQueue.Size = new System.Drawing.Size(352, 155);
            this.TBSupervisorQueue.TabIndex = 11;
            this.TBSupervisorQueue.TabStop = false;
            this.TBSupervisorQueue.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 365);
            this.Controls.Add(this.TBSupervisorQueue);
            this.Controls.Add(this.TBPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TBIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TBStudentQueue);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.RichTextBox TBStudentQueue;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TBIP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TBPort;
        public System.Windows.Forms.RichTextBox TBSupervisorQueue;
    }
}
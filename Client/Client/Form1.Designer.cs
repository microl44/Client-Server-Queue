namespace Client
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
            this.button1.Location = new System.Drawing.Point(13, 181);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(411, 111);
            this.button1.TabIndex = 4;
            this.button1.Text = "Connect To Server";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.BtnEnterQueue_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(140, 7);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(245, 23);
            this.textBox1.TabIndex = 1;
            // 
            // TBStudentQueue
            //
            this.TBStudentQueue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.TBStudentQueue.Location = new System.Drawing.Point(432, 33);
            this.TBStudentQueue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TBStudentQueue.Name = "TBStudentQueue";
            this.TBStudentQueue.Size = new System.Drawing.Size(410, 187);
            this.TBStudentQueue.TabIndex = 7;
            this.TBStudentQueue.TabStop = false;
            this.TBStudentQueue.Text = "---Current Student Queue---";
            // 
            // TBSupervisorQueue
            // 
            this.TBSupervisorQueue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.TBSupervisorQueue.Location = new System.Drawing.Point(430, 228);
            this.TBSupervisorQueue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TBSupervisorQueue.Name = "TBSupervisorQueue";
            this.TBSupervisorQueue.ReadOnly = true;
            this.TBSupervisorQueue.Size = new System.Drawing.Size(410, 178);
            this.TBSupervisorQueue.TabIndex = 8;
            this.TBSupervisorQueue.TabStop = false;
            this.TBSupervisorQueue.Text = "---Current Supervisors---";
            //
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Enter Queue Alias:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(13, 298);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(202, 111);
            this.button2.TabIndex = 5;
            this.button2.Text = "Subscribe To Queue";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.BtnSubscribe_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(223, 298);
            this.button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(202, 111);
            this.button4.TabIndex = 6;
            this.button4.Text = "Unsubscribe from Queue";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.BtnLeaveQueue);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Enter Server IP/URL:";
            // 
            // TBIP
            // 
            this.TBIP.Location = new System.Drawing.Point(140, 36);
            this.TBIP.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TBIP.Name = "TBIP";
            this.TBIP.Size = new System.Drawing.Size(245, 23);
            this.TBIP.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 68);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 15);
            this.label3.TabIndex = 9;
            this.label3.Text = "Enter PORT:";
            // 
            // TBPort
            // 
            this.TBPort.Location = new System.Drawing.Point(140, 65);
            this.TBPort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TBPort.Name = "TBPort";
            this.TBPort.Size = new System.Drawing.Size(245, 23);
            this.TBPort.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 421);
            this.Controls.Add(this.TBPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TBIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TBStudentQueue);
            this.Controls.Add(this.TBSupervisorQueue);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = "Student Client";
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
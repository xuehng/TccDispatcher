using System.Windows.Forms;

namespace renstech.sharpsip
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.dialButton = new System.Windows.Forms.Button();
            this.dialStringTextBox = new System.Windows.Forms.TextBox();
            this.hangupButton = new System.Windows.Forms.Button();
            this.answerButton = new System.Windows.Forms.Button();
            this.regButton = new System.Windows.Forms.Button();
            this.regStateButton = new System.Windows.Forms.Button();
            this.btnChannel1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDomain = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtProxy = new System.Windows.Forms.TextBox();
            this.t = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxReg = new System.Windows.Forms.CheckBox();
            this.btnStartUA = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.labelIncomeMsgQueu = new System.Windows.Forms.Label();
            this.labelOutgoMsgQueue = new System.Windows.Forms.Label();
            this.checkBoxAutoAnswer = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(531, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusLabel1});
            this.statusStrip.Location = new System.Drawing.Point(0, 343);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(531, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // dialButton
            // 
            this.dialButton.Location = new System.Drawing.Point(7, 98);
            this.dialButton.Name = "dialButton";
            this.dialButton.Size = new System.Drawing.Size(198, 32);
            this.dialButton.TabIndex = 15;
            this.dialButton.Text = "dial";
            this.dialButton.Click += new System.EventHandler(this.dialButton_Click);
            // 
            // dialStringTextBox
            // 
            this.dialStringTextBox.Location = new System.Drawing.Point(7, 71);
            this.dialStringTextBox.Name = "dialStringTextBox";
            this.dialStringTextBox.Size = new System.Drawing.Size(198, 21);
            this.dialStringTextBox.TabIndex = 14;
            // 
            // hangupButton
            // 
            this.hangupButton.Location = new System.Drawing.Point(7, 136);
            this.hangupButton.Name = "hangupButton";
            this.hangupButton.Size = new System.Drawing.Size(198, 32);
            this.hangupButton.TabIndex = 28;
            this.hangupButton.Text = "hangup";
            this.hangupButton.Click += new System.EventHandler(this.hangupButton_Click);
            // 
            // answerButton
            // 
            this.answerButton.Location = new System.Drawing.Point(7, 174);
            this.answerButton.Name = "answerButton";
            this.answerButton.Size = new System.Drawing.Size(198, 32);
            this.answerButton.TabIndex = 31;
            this.answerButton.Text = "answer";
            this.answerButton.Click += new System.EventHandler(this.answerButton_Click);
            // 
            // regButton
            // 
            this.regButton.Location = new System.Drawing.Point(7, 212);
            this.regButton.Name = "regButton";
            this.regButton.Size = new System.Drawing.Size(198, 32);
            this.regButton.TabIndex = 32;
            this.regButton.Text = "register";
            this.regButton.Click += new System.EventHandler(this.regButton_Click);
            // 
            // regStateButton
            // 
            this.regStateButton.Location = new System.Drawing.Point(7, 33);
            this.regStateButton.Name = "regStateButton";
            this.regStateButton.Size = new System.Drawing.Size(198, 32);
            this.regStateButton.TabIndex = 33;
            this.regStateButton.Text = "idle";
            // 
            // btnChannel1
            // 
            this.btnChannel1.Location = new System.Drawing.Point(220, 33);
            this.btnChannel1.Name = "btnChannel1";
            this.btnChannel1.Size = new System.Drawing.Size(297, 25);
            this.btnChannel1.TabIndex = 34;
            this.btnChannel1.Text = "channel";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(220, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 35;
            this.label1.Text = "user";
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(278, 87);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(239, 21);
            this.txtUser.TabIndex = 36;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(220, 121);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 37;
            this.label2.Text = "password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(278, 114);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(239, 21);
            this.txtPassword.TabIndex = 38;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(220, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 39;
            this.label3.Text = "domain";
            // 
            // txtDomain
            // 
            this.txtDomain.Location = new System.Drawing.Point(278, 141);
            this.txtDomain.Name = "txtDomain";
            this.txtDomain.Size = new System.Drawing.Size(239, 21);
            this.txtDomain.TabIndex = 40;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(220, 199);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 41;
            this.label4.Text = "input";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(220, 225);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 42;
            this.label5.Text = "output";
            // 
            // txtProxy
            // 
            this.txtProxy.Location = new System.Drawing.Point(278, 168);
            this.txtProxy.Name = "txtProxy";
            this.txtProxy.Size = new System.Drawing.Size(239, 21);
            this.txtProxy.TabIndex = 43;
            // 
            // t
            // 
            this.t.Location = new System.Drawing.Point(278, 195);
            this.t.Name = "t";
            this.t.Size = new System.Drawing.Size(239, 21);
            this.t.TabIndex = 44;
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(278, 222);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(239, 21);
            this.textBox6.TabIndex = 45;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(220, 173);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 12);
            this.label6.TabIndex = 46;
            this.label6.Text = "proxy";
            // 
            // checkBoxReg
            // 
            this.checkBoxReg.AutoSize = true;
            this.checkBoxReg.Location = new System.Drawing.Point(222, 65);
            this.checkBoxReg.Name = "checkBoxReg";
            this.checkBoxReg.Size = new System.Drawing.Size(138, 16);
            this.checkBoxReg.TabIndex = 47;
            this.checkBoxReg.Text = "enable registration";
            this.checkBoxReg.UseVisualStyleBackColor = true;
            // 
            // btnStartUA
            // 
            this.btnStartUA.Location = new System.Drawing.Point(362, 257);
            this.btnStartUA.Name = "btnStartUA";
            this.btnStartUA.Size = new System.Drawing.Size(154, 70);
            this.btnStartUA.TabIndex = 48;
            this.btnStartUA.Text = "启动UA";
            this.btnStartUA.UseVisualStyleBackColor = true;
            this.btnStartUA.Click += new System.EventHandler(this.btnStartUA_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 257);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 49;
            this.label7.Text = "Handset ID :";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(96, 257);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 50;
            this.label8.Text = "0000";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(14, 276);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(125, 12);
            this.label9.TabIndex = 51;
            this.label9.Text = "incoming msg queue :";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(14, 295);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(125, 12);
            this.label10.TabIndex = 52;
            this.label10.Text = "outgoing msg queue :";
            // 
            // labelIncomeMsgQueu
            // 
            this.labelIncomeMsgQueu.AutoSize = true;
            this.labelIncomeMsgQueu.Location = new System.Drawing.Point(145, 276);
            this.labelIncomeMsgQueu.Name = "labelIncomeMsgQueu";
            this.labelIncomeMsgQueu.Size = new System.Drawing.Size(29, 12);
            this.labelIncomeMsgQueu.TabIndex = 53;
            this.labelIncomeMsgQueu.Text = "0000";
            // 
            // labelOutgoMsgQueue
            // 
            this.labelOutgoMsgQueue.AutoSize = true;
            this.labelOutgoMsgQueue.Location = new System.Drawing.Point(145, 295);
            this.labelOutgoMsgQueue.Name = "labelOutgoMsgQueue";
            this.labelOutgoMsgQueue.Size = new System.Drawing.Size(29, 12);
            this.labelOutgoMsgQueue.TabIndex = 54;
            this.labelOutgoMsgQueue.Text = "0000";
            // 
            // checkBoxAutoAnswer
            // 
            this.checkBoxAutoAnswer.AutoSize = true;
            this.checkBoxAutoAnswer.Location = new System.Drawing.Point(366, 65);
            this.checkBoxAutoAnswer.Name = "checkBoxAutoAnswer";
            this.checkBoxAutoAnswer.Size = new System.Drawing.Size(90, 16);
            this.checkBoxAutoAnswer.TabIndex = 55;
            this.checkBoxAutoAnswer.Text = "auto answer";
            this.checkBoxAutoAnswer.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 365);
            this.Controls.Add(this.checkBoxAutoAnswer);
            this.Controls.Add(this.labelOutgoMsgQueue);
            this.Controls.Add(this.labelIncomeMsgQueu);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnStartUA);
            this.Controls.Add(this.checkBoxReg);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.t);
            this.Controls.Add(this.txtProxy);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDomain);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnChannel1);
            this.Controls.Add(this.regStateButton);
            this.Controls.Add(this.regButton);
            this.Controls.Add(this.answerButton);
            this.Controls.Add(this.hangupButton);
            this.Controls.Add(this.dialButton);
            this.Controls.Add(this.dialStringTextBox);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "SPNV SIP Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private Button dialButton;
        private TextBox dialStringTextBox;
        private Button hangupButton;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private Button answerButton;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private Button regButton;
        private Button regStateButton;
        private Button btnChannel1;
        private Label label1;
        private TextBox txtUser;
        private Label label2;
        private TextBox txtPassword;
        private Label label3;
        private TextBox txtDomain;
        private Label label4;
        private Label label5;
        private TextBox txtProxy;
        private TextBox t;
        private TextBox textBox6;
        private Label label6;
        private CheckBox checkBoxReg;
        private Button btnStartUA;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
        private Label labelIncomeMsgQueu;
        private Label labelOutgoMsgQueue;
        private CheckBox checkBoxAutoAnswer;
    }
}


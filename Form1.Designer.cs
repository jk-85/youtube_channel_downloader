using System.Drawing;

namespace DownloadYTChannel
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button buttonGenerateOnly;


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
            textBoxChannel = new TextBox();
            textBoxOutput = new TextBox();
            buttonStart = new Button();
            buttonClose = new Button();
            label1 = new Label();
            buttonGenerateOnly = new Button();
            button1 = new Button();
            label2 = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // textBoxChannel
            // 
            textBoxChannel.Location = new Point(159, 14);
            textBoxChannel.Name = "textBoxChannel";
            textBoxChannel.Size = new Size(129, 23);
            textBoxChannel.TabIndex = 0;
            // 
            // textBoxOutput
            // 
            textBoxOutput.Location = new Point(12, 43);
            textBoxOutput.Multiline = true;
            textBoxOutput.Name = "textBoxOutput";
            textBoxOutput.ReadOnly = true;
            textBoxOutput.ScrollBars = ScrollBars.Vertical;
            textBoxOutput.Size = new Size(590, 282);
            textBoxOutput.TabIndex = 5;
            textBoxOutput.TextChanged += textBoxOutput_TextChanged;
            // 
            // buttonStart
            // 
            buttonStart.Location = new Point(294, 12);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(55, 25);
            buttonStart.TabIndex = 1;
            buttonStart.Text = "Start";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += buttonStart_Click;
            // 
            // buttonClose
            // 
            buttonClose.Location = new Point(407, 12);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(55, 25);
            buttonClose.TabIndex = 3;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 18);
            label1.Name = "label1";
            label1.Size = new Size(140, 15);
            label1.TabIndex = 4;
            label1.Text = "YouTube Channel Name:";
            // 
            // buttonGenerateOnly
            // 
            buttonGenerateOnly.Location = new Point(464, 12);
            buttonGenerateOnly.Name = "buttonGenerateOnly";
            buttonGenerateOnly.Size = new Size(138, 25);
            buttonGenerateOnly.TabIndex = 4;
            buttonGenerateOnly.Text = "Only create index.html";
            buttonGenerateOnly.UseVisualStyleBackColor = true;
            buttonGenerateOnly.Click += buttonGenerateOnly_Click;
            // 
            // button1
            // 
            button1.Location = new Point(351, 12);
            button1.Name = "button1";
            button1.Size = new Size(54, 25);
            button1.TabIndex = 2;
            button1.Text = "Cancel";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 330);
            label2.Name = "label2";
            label2.Size = new Size(453, 15);
            label2.TabIndex = 6;
            label2.Text = "yt-dlp for win7 is used. Included here. If it gets problems in the future, update it here:";
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = Color.Blue;
            label3.Location = new Point(12, 348);
            label3.Name = "label3";
            label3.Size = new Size(437, 15);
            label3.TabIndex = 7;
            label3.Text = "Download the latest yt-dlp_win7.exe on the right margin on the page (\"Releases\").";
            label3.Click += label3_Click;
            // 
            // Form1
            // 
            ClientSize = new Size(615, 370);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(label1);
            Controls.Add(buttonClose);
            Controls.Add(buttonStart);
            Controls.Add(textBoxOutput);
            Controls.Add(textBoxChannel);
            Controls.Add(buttonGenerateOnly);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Form1";
            Text = "Hans Welder's Complete YouTube Channel Downloader";
            Load += Form1_Load_1;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBoxChannel;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}

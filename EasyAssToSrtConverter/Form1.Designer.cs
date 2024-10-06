namespace EasyAssToSrtConverter
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            LLB_AssFolder = new LinkLabel();
            BTN_SelectAssFolder = new Button();
            BTN_Run = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(26, 19);
            label1.Name = "label1";
            label1.Size = new Size(101, 21);
            label1.TabIndex = 0;
            label1.Text = "ass字幕目录:";
            // 
            // LLB_AssFolder
            // 
            LLB_AssFolder.LinkColor = Color.FromArgb(255, 128, 0);
            LLB_AssFolder.Location = new Point(133, 19);
            LLB_AssFolder.Name = "LLB_AssFolder";
            LLB_AssFolder.Size = new Size(374, 25);
            LLB_AssFolder.TabIndex = 1;
            LLB_AssFolder.TabStop = true;
            LLB_AssFolder.Text = "未选择";
            LLB_AssFolder.LinkClicked += LLB_AssFolder_LinkClicked;
            // 
            // BTN_SelectAssFolder
            // 
            BTN_SelectAssFolder.Location = new Point(522, 16);
            BTN_SelectAssFolder.Name = "BTN_SelectAssFolder";
            BTN_SelectAssFolder.Size = new Size(82, 30);
            BTN_SelectAssFolder.TabIndex = 2;
            BTN_SelectAssFolder.Text = "选择";
            BTN_SelectAssFolder.UseVisualStyleBackColor = true;
            BTN_SelectAssFolder.Click += BTN_SelectAssFolder_Click;
            // 
            // BTN_Run
            // 
            BTN_Run.Location = new Point(457, 79);
            BTN_Run.Name = "BTN_Run";
            BTN_Run.Size = new Size(147, 65);
            BTN_Run.TabIndex = 3;
            BTN_Run.Text = "运行";
            BTN_Run.UseVisualStyleBackColor = true;
            BTN_Run.Click += BTN_Run_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(631, 163);
            Controls.Add(BTN_Run);
            Controls.Add(BTN_SelectAssFolder);
            Controls.Add(LLB_AssFolder);
            Controls.Add(label1);
            Font = new Font("Microsoft YaHei UI", 12F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "简单Ass到Srt字幕转换器";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private LinkLabel LLB_AssFolder;
        private Button BTN_SelectAssFolder;
        private Button BTN_Run;
    }
}

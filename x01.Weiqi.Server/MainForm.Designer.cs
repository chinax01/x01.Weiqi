namespace Server
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
            this.m_ButtonStartServer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_ButtonStartServer
            // 
            this.m_ButtonStartServer.Location = new System.Drawing.Point(51, 27);
            this.m_ButtonStartServer.Name = "m_ButtonStartServer";
            this.m_ButtonStartServer.Size = new System.Drawing.Size(126, 39);
            this.m_ButtonStartServer.TabIndex = 0;
            this.m_ButtonStartServer.Text = "&Start Server";
            this.m_ButtonStartServer.UseVisualStyleBackColor = true;
            this.m_ButtonStartServer.Click += new System.EventHandler(this.ButtonStartServer_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(221, 97);
            this.Controls.Add(this.m_ButtonStartServer);
            this.Name = "MainForm";
            this.Text = "Main Form";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_ButtonStartServer;
    }
}


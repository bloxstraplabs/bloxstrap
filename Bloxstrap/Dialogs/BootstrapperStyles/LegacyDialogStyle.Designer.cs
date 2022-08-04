namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    partial class LegacyDialogStyle
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
            this.Message = new System.Windows.Forms.Label();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.IconBox = new System.Windows.Forms.PictureBox();
            this.CancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).BeginInit();
            this.SuspendLayout();
            // 
            // Message
            // 
            this.Message.Location = new System.Drawing.Point(55, 23);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(287, 17);
            this.Message.TabIndex = 0;
            this.Message.Text = "Please wait...";
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(58, 51);
            this.ProgressBar.MarqueeAnimationSpeed = 33;
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(287, 26);
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressBar.TabIndex = 1;
            // 
            // IconBox
            // 
            this.IconBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.IconBox.ImageLocation = "";
            this.IconBox.Location = new System.Drawing.Point(19, 16);
            this.IconBox.Name = "IconBox";
            this.IconBox.Size = new System.Drawing.Size(32, 32);
            this.IconBox.TabIndex = 2;
            this.IconBox.TabStop = false;
            // 
            // CancelButton
            // 
            this.CancelButton.Enabled = false;
            this.CancelButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CancelButton.Location = new System.Drawing.Point(271, 83);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Visible = false;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // LegacyDialogStyle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 131);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.IconBox);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.Message);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(378, 170);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(378, 170);
            this.Name = "LegacyDialogStyle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LegacyDialogStyle";
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Label Message;
        private ProgressBar ProgressBar;
        private PictureBox IconBox;
        private Button CancelButton;
    }
}
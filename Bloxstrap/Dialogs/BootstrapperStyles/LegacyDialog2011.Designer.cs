namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    partial class LegacyDialog2011
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
            this.labelMessage = new System.Windows.Forms.Label();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.IconBox = new System.Windows.Forms.PictureBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).BeginInit();
            this.SuspendLayout();
            // 
            // labelMessage
            // 
            this.labelMessage.Location = new System.Drawing.Point(55, 23);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(287, 17);
            this.labelMessage.TabIndex = 0;
            this.labelMessage.Text = "Please wait...";
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
            // buttonCancel
            // 
            this.buttonCancel.Enabled = false;
            this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonCancel.Location = new System.Drawing.Point(271, 83);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Visible = false;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // LegacyDialog2011
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 131);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.IconBox);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.labelMessage);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(378, 170);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(378, 170);
            this.Name = "LegacyDialog2011";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LegacyDialog2011";
            this.Load += new System.EventHandler(this.LegacyDialog2011_Load);
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Label labelMessage;
        private ProgressBar ProgressBar;
        private PictureBox IconBox;
        private Button buttonCancel;
    }
}
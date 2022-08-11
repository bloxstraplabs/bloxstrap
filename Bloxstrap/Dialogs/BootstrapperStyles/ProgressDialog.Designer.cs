namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    partial class ProgressDialog
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
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.Message = new System.Windows.Forms.Label();
            this.IconBox = new System.Windows.Forms.PictureBox();
            this.ButtonCancel = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonCancel)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProgressBar
            // 
            this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBar.Location = new System.Drawing.Point(29, 241);
            this.ProgressBar.MarqueeAnimationSpeed = 20;
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(460, 20);
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressBar.TabIndex = 0;
            // 
            // Message
            // 
            this.Message.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Message.Location = new System.Drawing.Point(29, 199);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(460, 18);
            this.Message.TabIndex = 1;
            this.Message.Text = "Please wait...";
            this.Message.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Message.UseMnemonic = false;
            // 
            // IconBox
            // 
            this.IconBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.IconBox.ImageLocation = "";
            this.IconBox.Location = new System.Drawing.Point(212, 66);
            this.IconBox.Name = "IconBox";
            this.IconBox.Size = new System.Drawing.Size(92, 92);
            this.IconBox.TabIndex = 2;
            this.IconBox.TabStop = false;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Enabled = false;
            this.ButtonCancel.Image = global::Bloxstrap.Properties.Resources.CancelButton;
            this.ButtonCancel.Location = new System.Drawing.Point(194, 264);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(130, 44);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.TabStop = false;
            this.ButtonCancel.Visible = false;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            this.ButtonCancel.MouseEnter += new System.EventHandler(this.ButtonCancel_MouseEnter);
            this.ButtonCancel.MouseLeave += new System.EventHandler(this.ButtonCancel_MouseLeave);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.Message);
            this.panel1.Controls.Add(this.IconBox);
            this.panel1.Controls.Add(this.ButtonCancel);
            this.panel1.Controls.Add(this.ProgressBar);
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(518, 318);
            this.panel1.TabIndex = 4;
            // 
            // ProgressDialogStyle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ClientSize = new System.Drawing.Size(520, 320);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximumSize = new System.Drawing.Size(520, 320);
            this.MinimumSize = new System.Drawing.Size(520, 320);
            this.Name = "ProgressDialogStyle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ProgressDialogStyle";
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonCancel)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ProgressBar ProgressBar;
        private Label Message;
        private PictureBox IconBox;
        private PictureBox ButtonCancel;
        private Panel panel1;
    }
}
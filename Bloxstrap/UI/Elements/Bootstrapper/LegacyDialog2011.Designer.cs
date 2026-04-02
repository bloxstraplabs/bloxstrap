using System.Windows.Forms;

namespace Bloxstrap.UI.Elements.Bootstrapper
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
            labelMessage = new Label();
            ProgressBar = new ProgressBar();
            IconBox = new PictureBox();
            buttonCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)IconBox).BeginInit();
            SuspendLayout();
            // 
            // labelMessage
            // 
            labelMessage.Location = new System.Drawing.Point(55, 23);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new System.Drawing.Size(287, 17);
            labelMessage.TabIndex = 0;
            labelMessage.Text = "Please wait...";
            // 
            // ProgressBar
            // 
            ProgressBar.Location = new System.Drawing.Point(58, 51);
            ProgressBar.MarqueeAnimationSpeed = 33;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new System.Drawing.Size(287, 26);
            ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressBar.TabIndex = 1;
            // 
            // IconBox
            // 
            IconBox.BackgroundImageLayout = ImageLayout.Zoom;
            IconBox.ImageLocation = "";
            IconBox.Location = new System.Drawing.Point(19, 16);
            IconBox.Name = "IconBox";
            IconBox.Size = new System.Drawing.Size(32, 32);
            IconBox.TabIndex = 2;
            IconBox.TabStop = false;
            // 
            // buttonCancel
            // 
            buttonCancel.Enabled = false;
            buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonCancel.Location = new System.Drawing.Point(271, 83);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 23);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Visible = false;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // LegacyDialog2011
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(362, 131);
            Controls.Add(buttonCancel);
            Controls.Add(IconBox);
            Controls.Add(ProgressBar);
            Controls.Add(labelMessage);
            Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(378, 170);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(378, 170);
            Name = "LegacyDialog2011";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LegacyDialog2011";
            FormClosing += Dialog_FormClosing;
            Load += LegacyDialog2011_Load;
            ((System.ComponentModel.ISupportInitialize)IconBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label labelMessage;
        private ProgressBar ProgressBar;
        private PictureBox IconBox;
        private Button buttonCancel;
    }
}
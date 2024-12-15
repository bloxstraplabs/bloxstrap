using System.Windows.Forms;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    partial class LegacyDialog2008
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
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelMessage
            // 
            labelMessage.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            labelMessage.Location = new System.Drawing.Point(12, 16);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new System.Drawing.Size(287, 17);
            labelMessage.TabIndex = 0;
            labelMessage.Text = "Please wait...";
            // 
            // ProgressBar
            // 
            ProgressBar.Location = new System.Drawing.Point(15, 47);
            ProgressBar.MarqueeAnimationSpeed = 33;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new System.Drawing.Size(281, 20);
            ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressBar.TabIndex = 1;
            // 
            // buttonCancel
            // 
            buttonCancel.Enabled = false;
            buttonCancel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonCancel.Location = new System.Drawing.Point(221, 83);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 23);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // LegacyDialog2008
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(311, 122);
            Controls.Add(buttonCancel);
            Controls.Add(ProgressBar);
            Controls.Add(labelMessage);
            Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(327, 161);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(327, 161);
            Name = "LegacyDialog2008";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LegacyDialog2008";
            FormClosing += Dialog_FormClosing;
            Load += LegacyDialog2008_Load;
            ResumeLayout(false);
        }

        #endregion

        private Label labelMessage;
        private ProgressBar ProgressBar;
        private Button buttonCancel;
    }
}
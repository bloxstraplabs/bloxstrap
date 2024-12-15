using System.Windows.Forms;

namespace Bloxstrap.UI.Elements.Bootstrapper
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
            ProgressBar = new ProgressBar();
            labelMessage = new Label();
            IconBox = new PictureBox();
            panel1 = new Panel();
            buttonCancel = new Label();
            ((System.ComponentModel.ISupportInitialize)IconBox).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // ProgressBar
            // 
            ProgressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ProgressBar.Location = new System.Drawing.Point(29, 241);
            ProgressBar.MarqueeAnimationSpeed = 20;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new System.Drawing.Size(460, 20);
            ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressBar.TabIndex = 0;
            // 
            // labelMessage
            // 
            labelMessage.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            labelMessage.Location = new System.Drawing.Point(29, 199);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new System.Drawing.Size(460, 18);
            labelMessage.TabIndex = 1;
            labelMessage.Text = "Please wait...";
            labelMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            labelMessage.UseMnemonic = false;
            // 
            // IconBox
            // 
            IconBox.BackgroundImageLayout = ImageLayout.Zoom;
            IconBox.ImageLocation = "";
            IconBox.Location = new System.Drawing.Point(212, 66);
            IconBox.Name = "IconBox";
            IconBox.Size = new System.Drawing.Size(92, 92);
            IconBox.TabIndex = 2;
            IconBox.TabStop = false;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.SystemColors.Window;
            panel1.Controls.Add(buttonCancel);
            panel1.Controls.Add(labelMessage);
            panel1.Controls.Add(IconBox);
            panel1.Controls.Add(ProgressBar);
            panel1.Location = new System.Drawing.Point(1, 1);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(518, 318);
            panel1.TabIndex = 4;
            // 
            // buttonCancel
            // 
            buttonCancel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            buttonCancel.ForeColor = System.Drawing.Color.FromArgb(75, 75, 75);
            buttonCancel.Image = Properties.Resources.CancelButton;
            buttonCancel.Location = new System.Drawing.Point(194, 264);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(130, 44);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Cancel";
            buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            buttonCancel.UseMnemonic = false;
            buttonCancel.Click += ButtonCancel_Click;
            buttonCancel.MouseEnter += ButtonCancel_MouseEnter;
            buttonCancel.MouseLeave += ButtonCancel_MouseLeave;
            // 
            // ProgressDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.ActiveBorder;
            ClientSize = new System.Drawing.Size(520, 320);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximumSize = new System.Drawing.Size(520, 320);
            MinimumSize = new System.Drawing.Size(520, 320);
            Name = "ProgressDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ProgressDialog";
            FormClosing += Dialog_FormClosing;
            Load += ProgressDialog_Load;
            ((System.ComponentModel.ISupportInitialize)IconBox).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ProgressBar ProgressBar;
        private Label labelMessage;
        private PictureBox IconBox;
        private Panel panel1;
        private Label buttonCancel;
    }
}
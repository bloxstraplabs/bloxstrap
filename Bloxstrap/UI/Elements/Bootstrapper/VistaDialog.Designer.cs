namespace Bloxstrap.UI.Elements.Bootstrapper
{
    partial class VistaDialog
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
            SuspendLayout();
            // 
            // VistaDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(0, 0);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "VistaDialog";
            Opacity = 0D;
            ShowInTaskbar = false;
            Text = "VistaDialog";
            WindowState = System.Windows.Forms.FormWindowState.Minimized;
            FormClosing += Dialog_FormClosing;
            Load += Dialog_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
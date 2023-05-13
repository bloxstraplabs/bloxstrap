using System;
using System.Windows;
using System.Windows.Forms;

using Bloxstrap.Extensions;

namespace Bloxstrap.Dialogs
{
    public class BootstrapperDialogForm : Form, IBootstrapperDialog
    {
        public Bootstrapper? Bootstrapper { get; set; }

        #region UI Elements
        protected virtual string _message { get; set; } = "Please wait...";
        protected virtual ProgressBarStyle _progressStyle { get; set; }
        protected virtual int _progressValue { get; set; }
        protected virtual bool _cancelEnabled { get; set; }

        public string Message 
        { 
            get => _message; 
            set 
            { 
                if (this.InvokeRequired)
                    this.Invoke(() => _message = value);
                else
                    _message = value;
            } 
        }

        public ProgressBarStyle ProgressStyle
        {
            get => _progressStyle;
            set
            {
                if (this.InvokeRequired)
                    this.Invoke(() => _progressStyle = value);
                else
                    _progressStyle = value;
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (this.InvokeRequired)
                    this.Invoke(() => _progressValue = value);
                else
                    _progressValue = value;
            }
        }

        public bool CancelEnabled
        {
            get => _cancelEnabled;
            set
            {
                if (this.InvokeRequired)
                    this.Invoke(() => _cancelEnabled = value);
                else
                    _cancelEnabled = value;
            }
        }

        // Byfron specific - not required here, bypassing
        public bool VersionVisibility { get; set; }
        #endregion

        public void ScaleWindow()
        {
            this.Size = this.MinimumSize = this.MaximumSize = WindowScaling.GetScaledSize(this.Size);

            foreach (Control control in this.Controls)
            {
                control.Size = WindowScaling.GetScaledSize(control.Size);
                control.Location = WindowScaling.GetScaledPoint(control.Location);
                control.Padding = WindowScaling.GetScaledPadding(control.Padding);
            }
        }

        public void SetupDialog()
        {
            this.Text = App.Settings.Prop.BootstrapperTitle;
            this.Icon = App.Settings.Prop.BootstrapperIcon.GetIcon();
        }

        public void ButtonCancel_Click(object? sender, EventArgs e)
        {
            Bootstrapper?.CancelInstall();
            this.Close();
        }

        #region IBootstrapperDialog Methods
        public void ShowBootstrapper() => this.ShowDialog();

        public virtual void CloseBootstrapper()
        {
            if (this.InvokeRequired)
                this.Invoke(CloseBootstrapper);
            else
                this.Close();
        }

        public virtual void ShowSuccess(string message)
        {
            App.ShowMessageBox(message, MessageBoxImage.Information);
            App.Terminate();
        }

        public virtual void ShowError(string message)
        {
            App.ShowMessageBox($"An error occurred while starting Roblox\n\nDetails: {message}", MessageBoxImage.Error);
            App.Terminate(Bootstrapper.ERROR_INSTALL_FAILURE);
        }

        public void PromptShutdown()
        {
            MessageBoxResult result = App.ShowMessageBox(
                "Roblox is currently running, but needs to close. Would you like close Roblox now?",
                MessageBoxImage.Information,
                MessageBoxButton.OKCancel
            );

            if (result != MessageBoxResult.OK)
                Environment.Exit(Bootstrapper.ERROR_INSTALL_USEREXIT);
        }
        #endregion
    } 
}

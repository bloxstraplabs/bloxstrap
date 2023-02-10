using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

using Bloxstrap.Enums;
using Bloxstrap.Helpers;

namespace Bloxstrap.Dialogs
{
    public class BootstrapperDialogForm : Form, IBootstrapperDialog
    {
        public Bootstrapper? Bootstrapper { get; set; }

        protected override bool ShowWithoutActivation => App.IsQuiet;

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

        public BootstrapperDialogForm(Bootstrapper? bootstrapper = null)
        {
            Bootstrapper = bootstrapper;
        }

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
            this.Text = App.ProjectName;
            this.Icon = App.Settings.Prop.BootstrapperIcon.GetIcon();

            if (Bootstrapper is null)
            {
                Message = "Style preview - Click Cancel to close";
                CancelEnabled = true;
            }
            else
            {
                Bootstrapper.Dialog = this;
                Task.Run(RunBootstrapper);
            }
        }


        public async void RunBootstrapper()
        {
            if (Bootstrapper is null)
                return;

#if DEBUG
            await Bootstrapper.Run();
#else
            try
            {
                await Bootstrapper.Run();
            }
            catch (Exception ex)
            {
                // string message = String.Format("{0}: {1}", ex.GetType(), ex.Message);
                string message = ex.ToString();
                ShowError(message);
            }
#endif

            App.Terminate();
        }

        public void ShowAsPreview()
        {
            this.ShowDialog();
        }

        public void ShowAsBootstrapper()
        {
            System.Windows.Forms.Application.Run(this);
        }

        public virtual void HideBootstrapper()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(HideBootstrapper);
            }
            else
            {
                this.Opacity = 0;
                this.ShowInTaskbar = false;
            }
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

        public void ButtonCancel_Click(object? sender, EventArgs e)
        {
            if (Bootstrapper is null)
                this.Close();
            else
                Task.Run(() => Bootstrapper.CancelButtonClicked());
        }
    }
}

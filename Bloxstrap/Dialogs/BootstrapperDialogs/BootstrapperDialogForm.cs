using Bloxstrap.Enums;
using Bloxstrap.Helpers;

namespace Bloxstrap.Dialogs.BootstrapperDialogs
{
    public class BootstrapperDialogForm : Form, IBootstrapperDialog
    {
        public Bootstrapper? Bootstrapper { get; set; }

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
            if (Program.IsQuiet)
                this.Hide();

            this.Text = Program.ProjectName;
            this.Icon = Program.Settings.BootstrapperIcon.GetIcon();

            if (Bootstrapper is null)
            {
                Message = "Style Preview - Click Cancel to return";
                CancelEnabled = true;
            }
            else
            {
                Bootstrapper.Dialog = this;
                Task.Run(() => RunBootstrapper());
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
        }

        public virtual void ShowSuccess(string message)
        {
            Program.ShowMessageBox(message, MessageBoxIcon.Information);
            Program.Exit();
        }

        public virtual void ShowError(string message)
        {
            Program.ShowMessageBox($"An error occurred while starting Roblox\n\nDetails: {message}", MessageBoxIcon.Error);
            Program.Exit(Bootstrapper.ERROR_INSTALL_FAILURE);
        }

        public virtual void CloseDialog()
        {
            if (this.InvokeRequired)
                this.Invoke(CloseDialog);
            else
                this.Hide();
        }

        public void PromptShutdown()
        {
            DialogResult result = Program.ShowMessageBox(
                "Roblox is currently running, but needs to close. Would you like close Roblox now?",
                MessageBoxIcon.Information,
                MessageBoxButtons.OKCancel
            );

            if (result != DialogResult.OK)
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

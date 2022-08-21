using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    public class BootstrapperStyleForm : Form, IBootstrapperDialog
    {
        public Bootstrapper? Bootstrapper { get; set; }

        protected virtual string _message { get; set; }
        protected virtual ProgressBarStyle _progressStyle { get; set; }
        protected virtual int _progressValue { get; set; }
        protected virtual bool _cancelEnabled { get; set; }

        public string Message 
        { 
            get => _message; 
            set 
            { 
                if (this.InvokeRequired)
                    this.Invoke(new Action(() => { Message = value; }));
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
                    this.Invoke(new Action(() => { ProgressStyle = value; }));
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
                    this.Invoke(new Action(() => { ProgressValue = value; }));
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
                    this.Invoke(new Action(() => { CancelEnabled = value; }));
                else
                    _cancelEnabled = value;
            }
        }

        public void SetupDialog()
        {
            this.Text = Program.ProjectName;
            this.Icon = IconManager.GetIconResource();

            if (Bootstrapper is null)
            {
                Message = "Select Cancel to return to preferences";
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
        }

        public virtual void ShowSuccess(string message)
        {
            MessageBox.Show(
                message,
                Program.ProjectName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Program.Exit();
        }

        public virtual void ShowError(string message)
        {
            MessageBox.Show(
                $"An error occurred while starting Roblox\n\nDetails: {message}",
                Program.ProjectName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            Program.Exit();
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
            DialogResult result = MessageBox.Show(
                "Roblox is currently running, but needs to close. Would you like close Roblox now?",
                Program.ProjectName,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information
            );

            if (result != DialogResult.OK)
                Environment.Exit(0);
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

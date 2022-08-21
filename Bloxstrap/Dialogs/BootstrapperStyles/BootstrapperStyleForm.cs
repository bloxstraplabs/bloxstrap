using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    public class BootstrapperStyleForm : Form, IBootstrapperDialog
    {
        public Bootstrapper? Bootstrapper { get; set; }

        public virtual string Message { get; set; }
        public virtual ProgressBarStyle ProgressStyle { get; set; }
        public virtual int ProgressValue { get; set; }
        public virtual bool CancelEnabled { get; set; }


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
                Bootstrapper.CloseDialogEvent += new EventHandler(CloseDialog);
                Bootstrapper.PromptShutdownEvent += new EventHandler(PromptShutdown);
                Bootstrapper.ShowSuccessEvent += new ChangeEventHandler<string>(ShowSuccess);
                Bootstrapper.MessageChanged += new ChangeEventHandler<string>(MessageChanged);
                Bootstrapper.ProgressBarValueChanged += new ChangeEventHandler<int>(ProgressBarValueChanged);
                Bootstrapper.ProgressBarStyleChanged += new ChangeEventHandler<ProgressBarStyle>(ProgressBarStyleChanged);
                Bootstrapper.CancelEnabledChanged += new ChangeEventHandler<bool>(CancelEnabledChanged);

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

        public virtual void ShowSuccess(object sender, ChangeEventArgs<string> e)
        {
            MessageBox.Show(
                e.Value,
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

        public virtual void CloseDialog(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler handler = new(CloseDialog);
                this.Invoke(handler, sender, e);
            }
            else
            {
                this.Hide();
            }
        }

        public void PromptShutdown(object? sender, EventArgs e)
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


        public void MessageChanged(object sender, ChangeEventArgs<string> e)
        {
            if (this.InvokeRequired)
            {
                ChangeEventHandler<string> handler = new(MessageChanged);
                this.Invoke(handler, sender, e);
            }
            else
            {
                Message = e.Value;
            }
        }

        public void ProgressBarStyleChanged(object sender, ChangeEventArgs<ProgressBarStyle> e)
        {
            if (this.InvokeRequired)
            {
                ChangeEventHandler<ProgressBarStyle> handler = new(this.ProgressBarStyleChanged);
                this.Invoke(handler, sender, e);
            }
            else
            {
                ProgressStyle = e.Value;
            }
        }

        public void ProgressBarValueChanged(object sender, ChangeEventArgs<int> e)
        {
            if (this.InvokeRequired)
            {
                ChangeEventHandler<int> handler = new(ProgressBarValueChanged);
                this.Invoke(handler, sender, e);
            }
            else
            {
                ProgressValue = e.Value;
            }
        }

        public void CancelEnabledChanged(object sender, ChangeEventArgs<bool> e)
        {
            if (this.InvokeRequired)
            {
                ChangeEventHandler<bool> handler = new(CancelEnabledChanged);
                this.Invoke(handler, sender, e);
            }
            else
            {
                this.CancelEnabled = e.Value;
            }
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

using Bloxstrap.Enums;

namespace Bloxstrap.Dialogs.BootstrapperDialogs
{
    // basically just the modern dialog

    public partial class ProgressDialog : BootstrapperDialogForm
    {
        protected override string _message
        {
            get => labelMessage.Text;
            set => labelMessage.Text = value;
        }

        protected override ProgressBarStyle _progressStyle
        {
            get => ProgressBar.Style;
            set => ProgressBar.Style = value;
        }

        protected override int _progressValue
        {
            get => ProgressBar.Value;
            set => ProgressBar.Value = value;
        }

        protected override bool _cancelEnabled
        {
            get => this.buttonCancel.Enabled;
            set => this.buttonCancel.Enabled = this.buttonCancel.Visible = value;
        }

        public ProgressDialog(Bootstrapper? bootstrapper = null)
        {
            InitializeComponent();

            if (Program.Settings.Theme.GetFinal() == Theme.Dark)
            {
                this.labelMessage.ForeColor = SystemColors.Window;
                this.buttonCancel.Image = Properties.Resources.DarkCancelButton;
                this.panel1.BackColor = Color.FromArgb(35, 37, 39);
                this.BackColor = Color.FromArgb(25, 27, 29);
            }

            Bootstrapper = bootstrapper;

            this.IconBox.BackgroundImage = Program.Settings.BootstrapperIcon.GetBitmap();

            SetupDialog();
        }

        private void ButtonCancel_MouseEnter(object sender, EventArgs e)
        {
            if (Program.Settings.Theme.GetFinal() == Theme.Dark)
            {
                this.buttonCancel.Image = Properties.Resources.DarkCancelButtonHover;
            }
            else
            {
                this.buttonCancel.Image = Properties.Resources.CancelButtonHover;
            }
        }

        private void ButtonCancel_MouseLeave(object sender, EventArgs e)
        {
            if (Program.Settings.Theme.GetFinal() == Theme.Dark)
            {
                this.buttonCancel.Image = Properties.Resources.DarkCancelButton;
            }
            else
            {
                this.buttonCancel.Image = Properties.Resources.CancelButton;
            }
        }

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}

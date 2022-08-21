using Bloxstrap.Helpers;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // basically just the modern dialog

    public partial class ProgressDialog : BootstrapperStyleForm
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

            Bootstrapper = bootstrapper;

            this.IconBox.BackgroundImage = IconManager.GetBitmapResource();

            SetupDialog();
        }

        private void ButtonCancel_MouseEnter(object sender, EventArgs e)
        {
            this.buttonCancel.Image = Properties.Resources.CancelButtonHover;
        }

        private void ButtonCancel_MouseLeave(object sender, EventArgs e)
        {
            this.buttonCancel.Image = Properties.Resources.CancelButton;
        }

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}

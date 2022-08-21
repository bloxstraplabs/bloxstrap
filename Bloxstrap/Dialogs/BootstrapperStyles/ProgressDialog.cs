using Bloxstrap.Helpers;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // basically just the modern dialog

    public partial class ProgressDialog : BootstrapperStyleForm
    {
        public override string Message
        {
            get => labelMessage.Text;
            set => labelMessage.Text = value;
        }

        public override ProgressBarStyle ProgressStyle
        {
            get => ProgressBar.Style;
            set => ProgressBar.Style = value;
        }

        public override int ProgressValue
        {
            get => ProgressBar.Value;
            set => ProgressBar.Value = value;
        }

        public override bool CancelEnabled
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

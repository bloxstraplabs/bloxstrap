using System.Windows.Forms;

using Bloxstrap.UI.Elements.Bootstrapper.Base;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    // windows: https://youtu.be/VpduiruysuM?t=18
    // mac: https://youtu.be/ncHhbcVDRgQ?t=63

    public partial class LegacyDialog2008 : WinFormsDialogBase
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

        protected override int _progressMaximum
        {
            get => ProgressBar.Maximum;
            set => ProgressBar.Maximum = value;
        }

        protected override int _progressValue
        {
            get => ProgressBar.Value;
            set => ProgressBar.Value = value;
        }

        protected override bool _cancelEnabled
        {
            get => this.buttonCancel.Enabled;
            set => this.buttonCancel.Enabled = value;
        }

        public LegacyDialog2008()
        {
            InitializeComponent();

            this.buttonCancel.Text = Resources.Strings.Common_Cancel;

            ScaleWindow();
            SetupDialog();

            this.ProgressBar.RightToLeft = this.RightToLeft;
            this.ProgressBar.RightToLeftLayout = this.RightToLeftLayout;
        }

        private void LegacyDialog2008_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}

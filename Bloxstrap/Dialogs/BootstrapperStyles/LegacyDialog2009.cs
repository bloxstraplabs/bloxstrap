namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // windows: https://youtu.be/VpduiruysuM?t=18
    // mac: https://youtu.be/ncHhbcVDRgQ?t=63

    public partial class LegacyDialog2009 : BootstrapperStyleForm
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
            set => this.buttonCancel.Enabled = value; 
        }

        public LegacyDialog2009(Bootstrapper? bootstrapper = null)
        {
            InitializeComponent();

            Bootstrapper = bootstrapper;

            SetupDialog();
        }

        private void LegacyDialog2009_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}

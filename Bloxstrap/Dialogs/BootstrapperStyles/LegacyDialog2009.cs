namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // windows: https://youtu.be/VpduiruysuM?t=18
    // mac: https://youtu.be/ncHhbcVDRgQ?t=63

    public partial class LegacyDialog2009 : BootstrapperStyleForm
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
            set => this.buttonCancel.Enabled = value; 
        }

        public LegacyDialog2009(Bootstrapper? bootstrapper = null)
        {
            InitializeComponent();

            Bootstrapper = bootstrapper;

            SetupDialog();
        }
    }
}

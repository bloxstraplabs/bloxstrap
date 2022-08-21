using Bloxstrap.Helpers;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // https://youtu.be/3K9oCEMHj2s?t=35

    public partial class LegacyDialog2011 : BootstrapperStyleForm
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

        public LegacyDialog2011(Bootstrapper? bootstrapper = null)
        {
            InitializeComponent();

            Bootstrapper = bootstrapper;
            
            // have to convert icon -> bitmap since winforms scaling is poop
            this.IconBox.Image = IconManager.GetIconResource().ToBitmap();

            SetupDialog();
        }

        private void LegacyDialog2011_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}

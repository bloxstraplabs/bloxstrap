using Bloxstrap.Helpers;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // https://youtu.be/3K9oCEMHj2s?t=35

    public partial class LegacyDialog2011 : BootstrapperStyleForm
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

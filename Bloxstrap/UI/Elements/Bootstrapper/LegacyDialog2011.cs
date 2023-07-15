using System.Windows.Forms;

using Bloxstrap.UI.Elements.Bootstrapper.Base;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    // https://youtu.be/3K9oCEMHj2s?t=35

    public partial class LegacyDialog2011 : WinFormsDialogBase
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

        public LegacyDialog2011()
        {
            InitializeComponent();

            this.IconBox.BackgroundImage = App.Settings.Prop.BootstrapperIcon.GetIcon().ToBitmap();

            ScaleWindow();
            SetupDialog();
        }

        private void LegacyDialog2011_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}

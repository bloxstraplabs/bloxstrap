using System;
using System.Drawing;
using System.Windows.Forms;

using Bloxstrap.Enums;
using Bloxstrap.Extensions;
using Bloxstrap.UI.Elements.Bootstrapper.Base;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    // basically just the modern dialog

    public partial class ProgressDialog : WinFormsDialogBase
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

        public ProgressDialog()
        {
            InitializeComponent();

            if (App.Settings.Prop.Theme.GetFinal() == Theme.Dark)
            {
                this.labelMessage.ForeColor = SystemColors.Window;
                this.buttonCancel.Image = Properties.Resources.DarkCancelButton;
                this.panel1.BackColor = Color.FromArgb(35, 37, 39);
                this.BackColor = Color.FromArgb(25, 27, 29);
            }

            this.IconBox.BackgroundImage = App.Settings.Prop.BootstrapperIcon.GetIcon().GetSized(128, 128).ToBitmap();

            SetupDialog();
        }

        private void ButtonCancel_MouseEnter(object sender, EventArgs e)
        {
            if (App.Settings.Prop.Theme.GetFinal() == Theme.Dark)
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
            if (App.Settings.Prop.Theme.GetFinal() == Theme.Dark)
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

using System.Windows.Forms;
using System.Windows.Shell;

using Bloxstrap.UI.Utility;

namespace Bloxstrap.UI.Elements.Bootstrapper.Base
{
    public class WinFormsDialogBase : Form, IBootstrapperDialog
    {
        public Bloxstrap.Bootstrapper? Bootstrapper { get; set; }

        private bool _isClosing;

        #region UI Elements
        protected virtual string _message { get; set; } = "Please wait...";
        protected virtual ProgressBarStyle _progressStyle { get; set; }
        protected virtual int _progressValue { get; set; }
        protected virtual int _progressMaximum { get; set; }
        protected virtual TaskbarItemProgressState _taskbarProgressState { get; set; }
        protected virtual double _taskbarProgressValue { get; set; }
        protected virtual bool _cancelEnabled { get; set; }

        public string Message
        {
            get => _message;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _message = value);
                else
                    _message = value;
            }
        }

        public ProgressBarStyle ProgressStyle
        {
            get => _progressStyle;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _progressStyle = value);
                else
                    _progressStyle = value;
            }
        }

        public int ProgressMaximum
        {
            get => _progressMaximum;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _progressMaximum = value);
                else
                    _progressMaximum = value;
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _progressValue = value);
                else
                    _progressValue = value;
            }
        }

        public TaskbarItemProgressState TaskbarProgressState
        {
            get => _taskbarProgressState;
            set
            {
                _taskbarProgressState = value;

                if (Handle != IntPtr.Zero)
                    TaskbarProgress.SetProgressState(Handle, value);
            }
        }

        public double TaskbarProgressValue
        {
            get => _taskbarProgressValue;
            set
            {
                _taskbarProgressValue = value;

                if (Handle != IntPtr.Zero)
                    TaskbarProgress.SetProgressValue(Handle, (int)value, App.TaskbarProgressMaximum);
            }
        }

        public bool CancelEnabled
        {
            get => _cancelEnabled;
            set
            {
                if (InvokeRequired)
                    Invoke(() => _cancelEnabled = value);
                else
                    _cancelEnabled = value;
            }
        }
        #endregion

        public void ScaleWindow()
        {
            Size = MinimumSize = MaximumSize = WindowScaling.GetScaledSize(Size);

            foreach (Control control in Controls)
            {
                control.Size = WindowScaling.GetScaledSize(control.Size);
                control.Location = WindowScaling.GetScaledPoint(control.Location);
                control.Padding = WindowScaling.GetScaledPadding(control.Padding);
            }
        }

        public void SetupDialog()
        {
            Text = App.Settings.Prop.BootstrapperTitle;
            Icon = App.Settings.Prop.BootstrapperIcon.GetIcon();

            if (Locale.RightToLeft)
            {
                this.RightToLeft = RightToLeft.Yes;
                this.RightToLeftLayout = true;
            }
        }

        #region WinForms event handlers
        protected void ButtonCancel_Click(object? sender, EventArgs e) => Close();

        protected void Dialog_Load(object sender, EventArgs e)
        {
            TaskbarProgress.SetProgressState(Handle, _taskbarProgressState);
            if (_taskbarProgressState != TaskbarItemProgressState.None && _taskbarProgressState != TaskbarItemProgressState.Indeterminate)
                TaskbarProgress.SetProgressValue(Handle, (int)_taskbarProgressValue, App.TaskbarProgressMaximum);

            OnDialogLoad();
        }

        protected void Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isClosing)
                Bootstrapper?.Cancel();
        }
        #endregion

        #region Virtual Methods
        protected virtual void OnDialogLoad()
        {
            // Intentionally left blank
        }
        #endregion

        #region IBootstrapperDialog Methods
        public void ShowBootstrapper() => ShowDialog();

        public virtual void CloseBootstrapper()
        {
            if (InvokeRequired)
            {
                Invoke(CloseBootstrapper);
            }
            else
            {
                _isClosing = true;
                Close();
            }
        }

        public virtual void ShowSuccess(string message, Action? callback) => BaseFunctions.ShowSuccess(message, callback);
        #endregion
    }
}

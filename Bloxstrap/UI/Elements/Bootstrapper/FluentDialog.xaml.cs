using Bloxstrap.UI.ViewModels.Bootstrapper;

namespace Bloxstrap.UI.Elements.Bootstrapper
{
    /// <summary>
    /// Interaction logic for FluentDialog.xaml
    /// </summary>
    public partial class FluentDialog
    {
        public FluentDialog(bool aero)
            : base()
        {
            InitializeComponent();

            _viewModel = new FluentDialogViewModel(this, aero);
            DataContext = _viewModel;

            // setting this to true for mica results in the window being undraggable
            if (aero)
                AllowsTransparency = true;
        }
    }
}
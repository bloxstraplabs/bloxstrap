using Bloxstrap.UI.Elements.Base;
using Bloxstrap.UI.ViewModels.Editor;

namespace Bloxstrap.UI.Elements.Editor
{
    /// <summary>
    /// Interaction logic for BootstrapperEditorWindow.xaml
    /// </summary>
    public partial class BootstrapperEditorWindow : WpfUiWindow
    {
        public BootstrapperEditorWindow(string name)
        {
            var viewModel = new BootstrapperEditorWindowViewModel();
            viewModel.Name = name;
            viewModel.Title = $"Editing \"{name}\"";
            viewModel.Code = File.ReadAllText(Path.Combine(Paths.CustomThemes, name, "Theme.xml"));

            DataContext = viewModel;
            InitializeComponent();

            UIXML.Text = viewModel.Code;
        }

        private void OnCodeChanged(object sender, EventArgs e)
        {
            BootstrapperEditorWindowViewModel viewModel = (BootstrapperEditorWindowViewModel)DataContext;
            viewModel.Code = UIXML.Text;
            viewModel.OnPropertyChanged(nameof(viewModel.Code));
        }
    }
}

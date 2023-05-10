using System.ComponentModel;
using Bloxstrap.Dialogs;

namespace Bloxstrap.ViewModels
{
    public class HyperionDialogViewModel : FluentDialogViewModel, INotifyPropertyChanged
    {
        public string Version => $"Bloxstrap v{App.Version}";

        public HyperionDialogViewModel(IBootstrapperDialog dialog) : base(dialog)
        {
        }
    }
}

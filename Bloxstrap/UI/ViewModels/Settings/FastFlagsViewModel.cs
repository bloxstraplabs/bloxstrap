using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Wpf.Ui.Mvvm.Contracts;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.UI.Elements.Settings.Pages;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class FastFlagsViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Page _page;

        public ICommand OpenFastFlagEditorCommand => new RelayCommand(OpenFastFlagEditor);

        public bool UseFastFlagManager
        {
            get => App.Settings.Prop.UseFastFlagManager;
            set => App.Settings.Prop.UseFastFlagManager = value;
        }

        public Dictionary<string, List<FFlagPreset>> Presets => App.FastFlags.PresetConfig.Presets;

        public FastFlagsViewModel(Page page) 
        { 
            _page = page;
        }

        private void OpenFastFlagEditor()
        {
            if (Window.GetWindow(_page) is INavigationWindow window)
            {
                if (App.State.Prop.ShowFFlagEditorWarning)
                    window.Navigate(typeof(FastFlagEditorWarningPage));
                else
                    window.Navigate(typeof(FastFlagEditorPage));
            }
        }
    }
}

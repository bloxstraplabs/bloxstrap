using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.Enums.FlagPresets;
using System.Windows;
using Bloxstrap.UI.Elements.Settings.Pages;
using Wpf.Ui.Mvvm.Contracts;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Navigation;
using Bloxstrap.Enums.GBSPresets;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class GBSEditorViewModel : NotifyPropertyChangedViewModel
    {
        public bool ReadOnly
        {
            get => App.GlobalSettings.GetReadOnly();
            set => App.GlobalSettings.SetReadOnly(value);
        }

        public string FramerateCap
        {
            get => App.GlobalSettings.GetPreset("Rendering.FramerateCap")!;
            set => App.GlobalSettings.SetPreset("Rendering.FramerateCap", value);
        }

        public string UITransparency
        {
            get => App.GlobalSettings.GetPreset("UI.Transparency")!;
            set
            {
                App.GlobalSettings.SetPreset("UI.Transparency", value.Length >= 3 ? value[..3] : value); // guhh??

                OnPropertyChanged(nameof(UITransparency));
            }
        }

        public string GraphicsQuality
        {
            get => App.GlobalSettings.GetPreset("Rendering.SavedQualityLevel")!;
            set
            {
                App.GlobalSettings.SetPreset("Rendering.SavedQualityLevel", value);

                OnPropertyChanged(nameof(GraphicsQuality));
            }
        }

        public bool ReducedMotion
        {
            get => App.GlobalSettings.GetPreset("UI.ReducedMotion")?.ToLower() == "true";
            set => App.GlobalSettings.SetPreset("UI.ReducedMotion", value);
        }

        public IReadOnlyDictionary<FontSize, string?> FontSizes => GBSEditor.FontSizes;
        public FontSize SelectedFontSize
        {
            get => FontSizes.FirstOrDefault(x => x.Value == App.GlobalSettings.GetPreset("UI.FontSize")).Key;
            set => App.GlobalSettings.SetPreset("UI.FontSize", FontSizes[value]);
        }

        public string MouseSensitivity
        {
            get => App.GlobalSettings.GetPreset("User.MouseSensitivity")!;
            set => App.GlobalSettings.SetPreset("User.MouseSensitivity", value);
        }

        public string VREnabled
        {
            get => App.GlobalSettings.GetPreset("User.VREnabled")!;
            set => App.GlobalSettings.SetPreset("User.VREnabled", value);
        }
    }
}
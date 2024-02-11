﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Wpf.Ui.Mvvm.Contracts;

using CommunityToolkit.Mvvm.Input;

using Bloxstrap.UI.Elements.Menu.Pages;
using Bloxstrap.Enums.FlagPresets;

namespace Bloxstrap.UI.ViewModels.Menu
{
    public class FastFlagsViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Page _page;

        public FastFlagsViewModel(Page page) 
        { 
            _page = page;
        }

        private void OpenFastFlagEditor()
        {
            if (Window.GetWindow(_page) is INavigationWindow window)
                window.Navigate(typeof(FastFlagEditorPage));
        }

        public ICommand OpenFastFlagEditorCommand => new RelayCommand(OpenFastFlagEditor);

        public Visibility ShowDebugFlags => App.Settings.Prop.PowerTools ? Visibility.Visible : Visibility.Collapsed;

        public bool HttpRequestLogging
        {
            get => App.FastFlags.GetPreset("HTTP.Log") is not null;
            set => App.FastFlags.SetPreset("HTTP.Log", value ? 12 : null);
        }

        public string HttpRequestProxy
        {
            get => App.FastFlags.GetPreset("HTTP.Proxy.Address.1") ?? "";

            set
            {
                App.FastFlags.SetPreset("HTTP.Proxy.Enable", String.IsNullOrEmpty(value) ? null : true);
                App.FastFlags.SetPreset("HTTP.Proxy.Address", String.IsNullOrEmpty(value) ? null : value);
            }
        }

        public string StateOverlayFlags
        {
            get => App.FastFlags.GetPreset("UI.FlagState") ?? "";
            set => App.FastFlags.SetPreset("UI.FlagState", String.IsNullOrEmpty(value) ? null : value);
        }

        public int FramerateLimit
        {
            get => int.TryParse(App.FastFlags.GetPreset("Rendering.Framerate"), out int x) ? x : 60;
            set => App.FastFlags.SetPreset("Rendering.Framerate", value);
        }

        public IReadOnlyDictionary<RenderingMode, string> RenderingModes => FastFlagManager.RenderingModes;

        public RenderingMode SelectedRenderingMode
        {
            get => App.FastFlags.GetPresetEnum(RenderingModes, "Rendering.Mode", "True");
            set
            {
                App.FastFlags.SetPresetEnum("Rendering.Mode", RenderingModes[value], "True");
                App.FastFlags.CheckManualFullscreenPreset();
            }
        }

        public bool FixDisplayScaling
        {
            get => App.FastFlags.GetPreset("Rendering.DisableScaling") == "True";
            set => App.FastFlags.SetPreset("Rendering.DisableScaling", value ? "True" : null);
        }

        public bool AlternateGraphicsSelectorEnabled
        {
            get => App.FastFlags.GetPreset("UI.Menu.GraphicsSlider") == "True";
            set => App.FastFlags.SetPreset("UI.Menu.GraphicsSlider", value ? "True" : null);
        }

        public IReadOnlyDictionary<MaterialVersion, string> MaterialVersions => FastFlagManager.MaterialVersions;

        public MaterialVersion SelectedMaterialVersion
        {
            get
            {
                MaterialVersion oldMaterials = App.FastFlags.GetPresetEnum(MaterialVersions, "Rendering.Materials", FastFlagManager.OldTexturesFlagValue);

                if (oldMaterials != MaterialVersion.Default)
                    return oldMaterials;

                return App.FastFlags.GetPresetEnum(MaterialVersions, "Rendering.Materials", FastFlagManager.NewTexturesFlagValue);
            }

            set => App.FastFlags.SetPresetEnum("Rendering.Materials", MaterialVersions[value], MaterialVersions[value] == "NewTexturePack" ? FastFlagManager.OldTexturesFlagValue : FastFlagManager.NewTexturesFlagValue);
        }

        public IReadOnlyDictionary<InGameMenuVersion, Dictionary<string, string?>> IGMenuVersions => FastFlagManager.IGMenuVersions;

        public InGameMenuVersion SelectedIGMenuVersion
        {
            get
            {
                // yeah this kinda sucks
                foreach (var version in IGMenuVersions)
                {
                    bool flagsMatch = true;

                    foreach (var flag in version.Value)
                    {
                        foreach (var presetFlag in FastFlagManager.PresetFlags.Where(x => x.Key.StartsWith($"UI.Menu.Style.{flag.Key}")))
                        { 
                            if (App.FastFlags.GetValue(presetFlag.Value) != flag.Value)
                                flagsMatch = false;
                        }
                    }

                    if (flagsMatch)
                        return version.Key;
                }

                return IGMenuVersions.First().Key;
            }

            set
            {
                foreach (var flag in IGMenuVersions[value])
                    App.FastFlags.SetPreset($"UI.Menu.Style.{flag.Key}", flag.Value);
            }
        }

        public IReadOnlyDictionary<LightingMode, string> LightingModes => FastFlagManager.LightingModes;

        public LightingMode SelectedLightingMode
        {
            get => App.FastFlags.GetPresetEnum(LightingModes, "Rendering.Lighting", "True");
            set => App.FastFlags.SetPresetEnum("Rendering.Lighting", LightingModes[value], "True");
        }

        public bool GuiHidingEnabled
        {
            get => App.FastFlags.GetPreset("UI.Hide") == "32380007";
            set => App.FastFlags.SetPreset("UI.Hide", value ? "32380007" : null);
        }
    }
}

﻿using System.Windows;
using System.Windows.Interop;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Windows.UI.ViewManagement;

namespace Bloxstrap.UI.Elements.Base
{
    public abstract class WpfUiWindow : UiWindow
    {
        private readonly IThemeService _themeService = new ThemeService();
        private UISettings _settings;

        public WpfUiWindow()
        {
            ApplyTheme();
            _settings = new UISettings();
            _settings.ColorValuesChanged += ColorValuesChanged;
        }

        public void ApplyTheme()
        {
            _themeService.SetTheme(App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);
            _themeService.SetSystemAccent();
            
#if QA_BUILD
            this.BorderBrush = System.Windows.Media.Brushes.Red;
            this.BorderThickness = new Thickness(4);
#endif
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (App.Settings.Prop.WPFSoftwareRender || App.LaunchSettings.NoGPUFlag.Active)
            {
                if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }

            base.OnSourceInitialized(e);
        }

        private async void ColorValuesChanged(UISettings sender, object args)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                ApplyTheme();
            });
        }
    }
}

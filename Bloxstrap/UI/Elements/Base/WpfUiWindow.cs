using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Bloxstrap.UI.Elements.Base
{
    public class WpfUiWindow : UiWindow
    {
        private readonly IThemeService _themeService = new ThemeService();

        public void ApplyTheme()
        {
            _themeService.SetTheme(App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);
            _themeService.SetSystemAccent();
        }
    }
}

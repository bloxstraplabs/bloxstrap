using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Dialogs
{
    internal class LanguageSelectorViewModel
    {
        public event EventHandler? CloseRequestEvent;

        public ICommand SetLocaleCommand => new RelayCommand(SetLocale);

        public static List<string> Languages => Locale.SupportedLocales.Values.ToList();

        public string SelectedLanguage { get; set; } = Locale.SupportedLocales[App.Settings.Prop.Locale];

        private void SetLocale()
        {
            App.Settings.Prop.Locale = Locale.GetIdentifierFromName(SelectedLanguage);
            Locale.Set();

            CloseRequestEvent?.Invoke(this, new());
        }
    }
}

using Bloxstrap.UI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public class FastFlagEditorWarningBase : UiPage
    {
        private bool _initialLoad = false;

        protected FastFlagEditorWarningBase(Type nextPageType)
        {
            Loaded += Page_Loaded;

            var vm = new FastFlagEditorWarningViewModel(this, nextPageType, ContinueCallback);
            DataContext = vm;
            vm.StartCountdown();
        }

        protected virtual void ContinueCallback()
        {
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // refresh datacontext on page load to reset timer

            if (!_initialLoad)
            {
                _initialLoad = true;
                return;
            }

            ((FastFlagEditorWarningViewModel)DataContext).StartCountdown();
        }
    }
}

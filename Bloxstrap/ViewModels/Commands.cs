using Bloxstrap.Helpers;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bloxstrap.ViewModels
{
    public static class Commands
    {
        public static ICommand OpenWebpageCommand => new RelayCommand<string>(OpenWebpage);

        private static void OpenWebpage(string? location)
        {
            if (location is null)
                return;

            Utilities.OpenWebsite(location);
        }
    }
}

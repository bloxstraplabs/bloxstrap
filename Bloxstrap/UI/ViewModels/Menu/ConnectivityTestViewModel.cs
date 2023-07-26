using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Menu

{
    internal class ConnectivityTestViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand RunAgainCommand => new RelayCommand(RunAgain);

        public string Output { get; private set; } = "";

        public ConnectivityTestViewModel()
        {
            Task.Run(() => RunTest());
        }

        private void RunAgain() => Task.Run(() => RunTest());

        private void WriteToOutput(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            Output += $"[{timestamp}] {message}\n";
            OnPropertyChanged(nameof(Output));
        }

        private async void RunTest()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Output = "";

            try
            {
                string url = "https://www.google.com";
                WriteToOutput($"Connecting to {url}...");
                var result = await App.HttpClient.GetAsync(url);
                WriteToOutput($"Connection complete! (HTTP {(int)result.StatusCode})");
            }
            catch (Exception ex)
            {
                WriteToOutput($"Connection failed!");
                WriteToOutput(ex.ToString());
            }

            WriteToOutput("");

            try
            {
                string url = "https://clientsettingscdn.roblox.com";
                WriteToOutput($"Connecting to {url}...");
                var result = await App.HttpClient.GetAsync(url);
                string response = await result.Content.ReadAsStringAsync();
                WriteToOutput($"Connection complete! (HTTP {(int)result.StatusCode})");
                WriteToOutput(response);
            }
            catch (Exception ex)
            {
                WriteToOutput($"Connection failed!");
                WriteToOutput(ex.ToString());
            }

            WriteToOutput("");

            try
            {
                string url = "http://s3.amazonaws.com/setup.roblox.com/version";
                WriteToOutput($"Connecting to {url}...");
                var result = await App.HttpClient.GetAsync(url);
                string response = await result.Content.ReadAsStringAsync();
                WriteToOutput($"Connection complete! (HTTP {(int)result.StatusCode})");
                WriteToOutput(response);
            }
            catch (Exception ex)
            {
                WriteToOutput($"Connection failed!");
                WriteToOutput(ex.ToString());
            }
        }
    }
}

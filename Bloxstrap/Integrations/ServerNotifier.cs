using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

using Bloxstrap.Helpers;
using Bloxstrap.Properties;

namespace Bloxstrap.Integrations
{
    public class ServerNotifier
    {
        private readonly GameActivityWatcher _activityWatcher;

        public ServerNotifier(GameActivityWatcher activityWatcher)
        {
            _activityWatcher = activityWatcher;
            _activityWatcher.OnGameJoin += (_, _) => Task.Run(() => Notify());
        }

        public async void Notify()
        {
            string machineAddress = _activityWatcher.ActivityMachineAddress;
            string message = "";

            App.Logger.WriteLine($"[ServerNotifier::Notify] Getting server information for {machineAddress}");

            // basically nobody has a free public access geolocation api that's accurate,
            // the ones that do require an api key which isn't suitable for a client-side application like this
            // so, hopefully this is reliable enough?
            string locationCity = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/city");
            string locationRegion = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/region");
            string locationCountry = await App.HttpClient.GetStringAsync($"https://ipinfo.io/{machineAddress}/country");

            locationCity = locationCity.ReplaceLineEndings("");
            locationRegion = locationRegion.ReplaceLineEndings("");
            locationCountry = locationCountry.ReplaceLineEndings("");

            if (String.IsNullOrEmpty(locationCountry))
                message = "Location: N/A";
            else if (locationCity == locationRegion)
                message = $"Location: {locationRegion}, {locationCountry}\n";
            else
                message = $"Location: {locationCity}, {locationRegion}, {locationCountry}\n";

            // UDMUX protected servers don't respond to ICMP packets and so the ping fails
            // we could probably get around this by doing a UDP latency test but ehhhhhhhh
            if (_activityWatcher.ActivityMachineUDMUX)
            {
                message += "Latency: N/A (Server is UDMUX protected)";
            }
            else
            {
                PingReply ping = await new Ping().SendPingAsync(machineAddress);

                if (ping.Status == IPStatus.Success)
                    message += $"Latency: ~{ping.RoundtripTime}ms";
                else
                    message += $"Latency: N/A (Code {ping.Status})";
            }

            App.Logger.WriteLine($"[ServerNotifier::Notify] {message.ReplaceLineEndings("\\n")}");

            NotifyIcon notification = new()
            {
                Icon = Resources.IconBloxstrap,
                Text = "Bloxstrap",
                Visible = true,
                BalloonTipTitle = "Connected to server",
                BalloonTipText = message
            };

            notification.ShowBalloonTip(10);
            await Task.Delay(10000);
            notification.Dispose();
        }
    }
}

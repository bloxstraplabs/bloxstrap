using System.Text;
using System.Web;
using Microsoft.Win32;

namespace Bloxstrap.Helpers
{
    public class Protocol
    {
        // map uri keys to command line args
        private static readonly IReadOnlyDictionary<string, string> UriKeyArgMap = new Dictionary<string, string>()
        {
			// excluding roblox-player, browsertrackerid and channel
            { "launchmode", "--" },
            { "gameinfo", "-t " },
            { "placelauncherurl", "-j "},
            // { "launchtime", "--launchtime=" }, we'll set this when launching the game client
            { "robloxLocale", "--rloc " },
            { "gameLocale", "--gloc " },
        };

        public static string Parse(string protocol)
        {
            string[] keyvalPair;
            string key;
            string val;
            StringBuilder commandLine = new();

            foreach (var parameter in protocol.Split('+'))
            {
                if (!parameter.Contains(':'))
                    continue;

                keyvalPair = parameter.Split(':');
                key = keyvalPair[0];
                val = keyvalPair[1];

                if (!UriKeyArgMap.ContainsKey(key))
                    continue;

                if (key == "placelauncherurl")
                    val = HttpUtility.UrlDecode(val).Replace("browserTrackerId", "lol");

                commandLine.Append(UriKeyArgMap[key] + val + " ");
            }

            return commandLine.ToString();
        }

        public static void Register(string key, string name, string handler)
        {
            string handlerArgs = $"\"{handler}\" %1";
            RegistryKey uriKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{key}");
            RegistryKey uriIconKey = uriKey.CreateSubKey("DefaultIcon");
            RegistryKey uriCommandKey = uriKey.CreateSubKey(@"shell\open\command");

            if (uriKey.GetValue("") is null)
            {
                uriKey.SetValue("", $"URL: {name} Protocol");
                uriKey.SetValue("URL Protocol", "");
            }

            if ((string?)uriCommandKey.GetValue("") != handlerArgs)
            {
                uriIconKey.SetValue("", handler);
                uriCommandKey.SetValue("", handlerArgs);
            }

            uriKey.Close();
            uriIconKey.Close();
            uriCommandKey.Close();
        }

        public static void Unregister(string key)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKey($@"Software\Classes\{key}");
            }
            catch (Exception) { }
        }
    }
}

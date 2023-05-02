using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json;

namespace Bloxstrap
{
    internal class DeployHelper
    {
        public static readonly string ChannelURL = "https://raw.githubusercontent.com/bluepilledgreat/Roblox-DeployHistory-Tracker/main/ChannelsActive.json";
        private static HttpClient HttpClient = new HttpClient();

        public static async Task<List<string>> GetChannels()
        {
            App.Logger.WriteLine($"[DeployHelper::GetChannels] Trying to get currently active channels from {ChannelURL}");

            var Response = await HttpClient.GetAsync(ChannelURL);
            var JSON = await Response.Content.ReadAsStringAsync();

            var Channels = JsonConvert.DeserializeObject<List<string>>(JSON);

            return Channels;
        }
    }
}

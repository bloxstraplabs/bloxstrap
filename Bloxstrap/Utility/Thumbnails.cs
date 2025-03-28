using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Utility
{
    internal static class Thumbnails
    {
        // TODO: remove requests from list once they're finished or failed
        /// <remarks>
        /// Returned array may contain null values
        /// </remarks>
        public static async Task<string?[]> GetThumbnailUrlsAsync(List<ThumbnailRequest> requests, CancellationToken token)
        {
            const string LOG_IDENT = "Thumbnails::GetThumbnailUrlsAsync";
            const int RETRIES = 5;
            const int RETRY_TIME_INCREMENT = 500; // ms

            string?[] urls = new string?[requests.Count];

            // assign unique request ids to each request
            for (int i = 0; i < requests.Count; i++)
                requests[i].RequestId = i.ToString();

            var payload = new StringContent(JsonSerializer.Serialize(requests));

            ThumbnailResponse[] response = null!;

            for (int i = 1; i <= RETRIES; i++)
            {
                var json = await App.HttpClient.PostFromJsonWithRetriesAsync<ThumbnailBatchResponse>("https://thumbnails.roblox.com/v1/batch", payload, 3, token);
                if (json == null)
                    throw new InvalidHTTPResponseException("Deserialised ThumbnailBatchResponse is null");

                response = json.Data;

                bool finished = response.All(x => x.State != "Pending");
                if (finished)
                    break;

                if (i == RETRIES)
                    App.Logger.WriteLine(LOG_IDENT, "Ran out of retries");
                else
                    await Task.Delay(RETRY_TIME_INCREMENT * i, token);
            }

            foreach (var item in response)
            {
                if (item.State == "Pending")
                    App.Logger.WriteLine(LOG_IDENT, $"{item.TargetId} is still pending");
                else if (item.State == "Error")
                    App.Logger.WriteLine(LOG_IDENT, $"{item.TargetId} got error code {item.ErrorCode} ({item.ErrorMessage})");
                else if (item.State != "Completed")
                    App.Logger.WriteLine(LOG_IDENT, $"{item.TargetId} got \"{item.State}\"");

                urls[int.Parse(item.RequestId)] = item.ImageUrl;
            }

            return urls;
        }

        public static async Task<string?> GetThumbnailUrlAsync(ThumbnailRequest request, CancellationToken token)
        {
            const string LOG_IDENT = "Thumbnails::GetThumbnailUrlAsync";
            const int RETRIES = 5;
            const int RETRY_TIME_INCREMENT = 500; // ms

            request.RequestId = "0";

            var payload = new StringContent(JsonSerializer.Serialize(new ThumbnailRequest[] { request }));

            ThumbnailResponse response = null!;

            for (int i = 1; i <= RETRIES; i++)
            {
                var json = await App.HttpClient.PostFromJsonWithRetriesAsync<ThumbnailBatchResponse>("https://thumbnails.roblox.com/v1/batch", payload, 3, token);
                if (json == null)
                    throw new InvalidHTTPResponseException("Deserialised ThumbnailBatchResponse is null");

                response = json.Data[0];

                if (response.State != "Pending")
                    break;

                if (i == RETRIES)
                    App.Logger.WriteLine(LOG_IDENT, "Ran out of retries");
                else
                    await Task.Delay(RETRY_TIME_INCREMENT * i, token);
            }

            if (response.State == "Pending")
                App.Logger.WriteLine(LOG_IDENT, $"{response.TargetId} is still pending");
            else if (response.State == "Error")
                App.Logger.WriteLine(LOG_IDENT, $"{response.TargetId} got error code {response.ErrorCode} ({response.ErrorMessage})");
            else if (response.State != "Completed")
                App.Logger.WriteLine(LOG_IDENT, $"{response.TargetId} got \"{response.State}\"");

            return response.ImageUrl;
        }
    }
}

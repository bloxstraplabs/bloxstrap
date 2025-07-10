using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Extensions
{
    internal static class HttpClientEx
    {
        public static async Task<HttpResponseMessage> GetWithRetriesAsync(this HttpClient client, string url, int retries, CancellationToken token)
        {
            HttpResponseMessage response = null!;

            for (int i = 1; i <= retries; i++)
            {
                try
                {
                    response = await client.GetAsync(url, token);
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("HttpClientEx::GetWithRetriesAsync", ex);

                    if (i == retries)
                        throw;
                }
            }

            return response;
        }

        public static async Task<HttpResponseMessage> PostWithRetriesAsync(this HttpClient client, string url, HttpContent? content, int retries, CancellationToken token)
        {
            HttpResponseMessage response = null!;

            for (int i = 1; i <= retries; i++)
            {
                try
                {
                    response = await client.PostAsync(url, content, token);
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("HttpClientEx::PostWithRetriesAsync", ex);

                    if (i == retries)
                        throw;
                }
            }

            return response;
        }

        public static async Task<T?> GetFromJsonWithRetriesAsync<T>(this HttpClient client, string url, int retries, CancellationToken token) where T : class
        {
            HttpResponseMessage response = await GetWithRetriesAsync(client, url, retries, token);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(token);
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: token);
        }

        public static async Task<T?> PostFromJsonWithRetriesAsync<T>(this HttpClient client, string url, HttpContent? content, int retries, CancellationToken token) where T : class
        {
            HttpResponseMessage response = await PostWithRetriesAsync(client, url, content, retries, token);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(token);
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: token);
        }
    }
}

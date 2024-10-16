﻿using Serilog;
using TMOScraper.Properties;

namespace TMOScraper.Utils
{
    public static class Downloader
    {
        private static readonly HttpClient client;
        static Downloader() 
        {
            client = new HttpClient(new SocketsHttpHandler() { MaxConnectionsPerServer = 5 });
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:108.0) Gecko/20100101 Firefox/108.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        } 
        public static async Task DownloadChapter(string folderPath, List<string> imgUrls, CancellationToken token)
        {
            string url, filename;
            var tasks = new List<Task>();
            client.DefaultRequestHeaders.Referrer = new Uri(Settings.Default.Domain);

            for (int i = 0; i < imgUrls.Count; i++)
            {
                url = imgUrls[i];
                filename = $"{i:D3}." + url.Split('.').Last();
                tasks.Add(DownloadFile(new Uri(url), Path.Combine(folderPath, filename), filename, token));
            }

            await Task.WhenAll(tasks);
        }
        private static async Task DownloadFile(Uri uri, string path, string filename, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(path, FileMode.Create);
                await responseStream.CopyToAsync(fileStream, token);
                Log.Verbose($"Downloaded file {filename}.");
            }
            else
            {
                Log.Error($"Failed to download {filename}. Status code : {response.StatusCode}");
            }
            
        }
    }
}

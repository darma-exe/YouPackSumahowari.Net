using PuppeteerSharp;

namespace YouPackSumahowari.Net.Helpers;

public static class PuppeteerHelper
{
    /// <summary>
    /// Downloads the Playwright browser if it is not already downloaded.
    /// </summary>
    public static async Task DownloadIfNeededAsync()
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
    }
}
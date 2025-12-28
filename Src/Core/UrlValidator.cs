using System.Net.Http;

namespace FinanceNotifier.Core;

public interface IUrlValidator
{
    Task<bool> ValidateAsync(Dictionary<string, string> urls);
}
public class UrlValidator : IUrlValidator
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(15)
    };
    public async Task<bool> ValidateAsync(Dictionary<string, string> urls)
    {
        if (urls == null || urls.Count == 0)
            return false;

        foreach (KeyValuePair<string, string> url in urls)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url.Value) ||
                    !(url.Value.StartsWith("http://") || url.Value.StartsWith("https://")))
                {
                    throw new HighlightedException($"Invalid URL format");
                }

                // Use GET to follow redirects and check content
                using var response = await _httpClient.GetAsync(url.Value);

                // Get the FINAL URL after any redirects
                string finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url.Value;

                // Look for "404" in the final URL path
                if (finalUrl.Contains("/404") || finalUrl.Contains("/notfound", StringComparison.OrdinalIgnoreCase))
                {
                    throw new HighlightedException($"URL redirects to a 404 page");
                    // return false;
                }

                // Verify status code
                if (!response.IsSuccessStatusCode)
                {
                    throw new HighlightedException($"URL returned error: {(int)response.StatusCode} {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HighlightedException($"HTTP error for {url.Value}: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                throw new HighlightedException($"Timeout accessing URL");
            }
            catch (Exception ex)
            {
                throw new HighlightedException($"[{ex.Message}] {url.Value}");
            }
        }
        return true;
    }
}
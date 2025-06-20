using System.Text.Json;

public class DataScraperFormatter
{
    public required HttpClient client;
    public async Task<List<string>> Scrape(List<string> urls)
    {
        using HttpClient client = new();
        List<string> returnData = new();

        foreach (string url in urls)
        {
             try
                {
                    // TODO use HtmlAgilityPack for HTML parsing
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var dataJson = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(dataJson);
                    var prettified = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                    returnData.Add(prettified);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("Something went wrong with " + url + ". " + ex.Message);
                    throw;
                }
        }

        return returnData;
    }
}
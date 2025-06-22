using System.Drawing;
using Pastel;

using System.Text.Json;

using HtmlAgilityPack;

namespace FinanceNotifier.Src;

public class DataScraperFormatter(Dictionary<string, string> urls)
{
    private readonly Dictionary<string, string> _urls = urls;

    public async Task<Dictionary<string, string>> Scrape()
    {
        // using HttpClient client = new();
        Dictionary<string, string> returnData = new();

        foreach (KeyValuePair<string, string> url in _urls)
        {
            switch (url.Key)
            {
                case "PGIM-Press-Release":
                    ScrapePGIMPressReleasePage(url.Value);
                    break;
                default:
                    throw new Exception("Could not identify the urls");
            }
        }

        return returnData;
    }

    public static KeyValuePair<string, string> ScrapePGIMPressReleasePage(string url)
    {
        try
        {
            // Go to the page
            var web = new HtmlWeb();
            var htmlDoc = web.Load(url);

            // Look for anchors with a 'cmp-cta__link-wrapper' which should have "publishDate"
            var anchors = htmlDoc.DocumentNode.SelectNodes("//a[contains(@class, 'cmp-cta__link-wrapper')]");
            if (anchors == null || anchors.Count == 0)
            {
                throw new Exception("Anchors are null".Pastel(Color.OrangeRed));
            }


            // get its header and populate header into key
            // get article and populate article into value
            // push into common object

            foreach (var node in anchors)
            {
                // check type and date (published within the last week)
                if (node.NodeType == HtmlNodeType.Element && IsPublishedWithinLastWeek(node))
                {
                    // click on that one
                    Console.WriteLine(node.OuterHtml);

                }
            }

            // var response = await client.GetAsync(url);
            // response.EnsureSuccessStatusCode();
            // var dataJson = await response.Content.ReadAsStringAsync();
            // var parsedDoc = JsonDocument.Parse(dataJson);
            // var prettified = JsonSerializer.Serialize(parsedDoc.RootElement, new JsonSerializerOptions { WriteIndented = true });
            // returnData.Add("header", prettified);

            // should return a key:value pair - header:content
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Something went wrong with " + url + ". " + ex.Message);
            throw;
        }
    }

    public static bool IsPublishedWithinLastWeek(HtmlNode node)
    {
        // Extract 'data-cmp-data-layer' attribute
        // Decode HTML entities
        // Parse the decoded string using System.Text.Json
        // Extract the publishDate field
        return true;
    }
}
using System.Drawing;
using Pastel;

using System.Text.Json;

using HtmlAgilityPack;

namespace FinanceNotifier.Src;

public class DataScraperFormatter(Dictionary<string, string> urls)
{
    private readonly Dictionary<string, string> _urls = urls;

    public async Task<List<ArticleData>> Scrape()
    {
        List<ArticleData> scrapedArticles = new();

        foreach (KeyValuePair<string, string> url in _urls)
        {
            switch (url.Key)
            {
                case "PGIM-Press-Release":
                    ArticleData article = ScrapePGIMPressReleasePage(url.Value);
                    scrapedArticles.Add(article);
                    break;
                default:
                    throw new Exception("Could not identify the urls");
            }
        }

        return scrapedArticles;
    }

    public static ArticleData ScrapePGIMPressReleasePage(string url)
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
                throw new HighlightedException("Anchors are null");
            }


            // get its header and populate header into key
            // get article and populate article into value
            // push into common object

            foreach (var node in anchors)
            {
                // check type and date (published within the last week)
                if (node.NodeType == HtmlNodeType.Element)
                {
                    DateTime? datePublished = GetPublishedDate(node);
                    if (datePublished != null && IsPublishedWithinLastWeek(datePublished.Value))
                    {
                        // click on that one
                        string href = node.GetAttributeValue("href", "");
                        Console.WriteLine("href: ", href);
                    }
                }
            }

            var pgimArticle = new ArticleData();
            return pgimArticle;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Something went wrong with {url}: {ex.Message}");
            throw;
        }
    }

    public static DateTime? GetPublishedDate(HtmlNode node)
    {
        // Extract 'data-cmp-data-layer' attribute
        string dataCmpDataLayerAttribute = node.Attributes["data-cmp-data-layer"].Value
        ?? throw new HighlightedException("Could not find 'data-cmp-data-layer' attribute");

        // Decode HTML entities
        string decodedData = System.Net.WebUtility.HtmlDecode(dataCmpDataLayerAttribute);

        try
        {
            // Parse the decoded string using System.Text.Json
            var parsedDoc = JsonDocument.Parse(decodedData);
            // Extract the publishDate field
            var root = parsedDoc.RootElement;
            foreach (var property in root.EnumerateObject())
            {
                var entry = property.Value;

                string publishDateString = entry.GetProperty("publishDate").GetString()
                ?? throw new HighlightedException("Could not get publishDate");

                if (!DateTime.TryParse(publishDateString, out DateTime publishDate))
                {
                    throw new HighlightedException($"Failed to parse publishDate: {publishDateString}");
                }
                return publishDate;
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Failed to parse JSON: " + ex.Message);
        }

        return null;
    }

    public static bool IsPublishedWithinLastWeek(DateTime publishDate)
    {
        return (DateTime.Today - publishDate).TotalDays < 7;
    }
}
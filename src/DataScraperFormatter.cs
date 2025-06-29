using System.Text.Json;

using Microsoft.Playwright;

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
                    List<ArticleData> articles = await ScrapePGIMPressReleasePage(url.Value);
                    scrapedArticles.AddRange(articles);
                    break;
                default:
                    throw new Exception("Could not identify the urls");
            }
        }

        return scrapedArticles;
    }

    public async Task<List<ArticleData>> ScrapePGIMPressReleasePage(string url)
    {
        IPage page = await GetNewPlaywrightPage();
        string baseUrl = "https://www.pgim.com";
        try
        {

            // Go to the page
            await page.GotoAsync(url);

            // Wait for the content to load
            await page.WaitForSelectorAsync(".cmp-search-list__item-group", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Attached,
            });

            // Get list items
            var listItems = await page.QuerySelectorAllAsync("li.cmp-searchresult-item");
            if (listItems == null || listItems.Count == 0)
            {
                throw new HighlightedException("No articles found");
            }

            List<ArticleData> recentPgimArticles = new();
            // Loop over list items to identify recent articles
            foreach (var li in listItems)
            {
                var anchor = await li.QuerySelectorAsync("a.cmp-searchresult-link");
                // Get article's publish date
                DateTime? datePublished = await GetPublishedDate(li);
                if (anchor != null && datePublished != null && IsPublishedWithinLastWeek(datePublished.Value))
                {
                    // Click on a recent article
                    string href = await anchor.GetAttributeAsync("href") ?? string.Empty;
                    ArticleData article = new();

                    string header = await anchor.GetAttributeAsync("title") ?? string.Empty;
                    string splitHeader = header.Split(" - ")[0];
                    article.Header = splitHeader;

                    article.PublishDate = datePublished.Value;
                    article.Url = $"{baseUrl}{href}";
                    string articleContent = await GetPgimArticleContent(article.Url);
                    article.Content = articleContent;
                    recentPgimArticles.Add(article);
                }
            }
            return recentPgimArticles;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Something went wrong with {url}: {ex.Message}");
            throw;
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async Task<DateTime?> GetPublishedDate(IElementHandle node)
    {
        try
        {
            IElementHandle datePublishedElement = await node.QuerySelectorAsync("span.cmp-searchresult-date")
        ?? throw new HighlightedException("Could not find 'cmp-searchresult-date' span");
            string datePublishedString = await datePublishedElement.InnerTextAsync();

            if (!DateTime.TryParse(datePublishedString, out DateTime publishDate))
            {
                throw new HighlightedException($"Failed to parse publishDate: {datePublishedString}");
            }
            return publishDate;
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Failed to parse JSON: " + ex.Message);
        }

        return null;
    }

    public bool IsPublishedWithinLastWeek(DateTime publishDate)
    {
        return (DateTime.Today - publishDate).TotalDays < 7;
    }

    public async Task<string> GetPgimArticleContent(string articleUrl)
    {
        IPage page = await GetNewPlaywrightPage();
        await page.GotoAsync(articleUrl);
        string content = "";
        await page.CloseAsync();
        return content;
    }

    public async Task<IPage> GetNewPlaywrightPage()
    {
        // Initialize Playwright
        var playwright = await Playwright.CreateAsync();
        // Initialize a browser and a new page
        var browser = await playwright.Chromium.LaunchAsync(
        new BrowserTypeLaunchOptions { Headless = true });

        return await browser.NewPageAsync();
    }
}
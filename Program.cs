using System.Drawing;

using Pastel;

using DotNetEnv;

using Hangfire;

namespace FinanceNotifier;

using FinanceNotifier.Src;

public class Program
{
    public static void Main(string[] args)
    {
        string? dbConn = LoadEnv();
        GlobalConfiguration.Configuration
                          .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                          .UseColouredConsoleLogProvider()
                          .UseSimpleAssemblyNameTypeSerializer()
                          .UseRecommendedSerializerSettings()
                          .UseSqlServerStorage(dbConn);

        // Print starting log
        Start();

        // Each week on Sunday at 6AM run a job:
        // RecurringJob.AddOrUpdate("scrape", () => scraperFormatter.Scrape(), "0 6 * * SUN");
        BackgroundJob.Enqueue(() => RunRecurringJob());

        // keep server running until it is manually stopped
        using var server = new BackgroundJobServer();
        Console.ReadLine();
    }

    public static async Task RunRecurringJob()
    {
        // 1) pass scrapeUrls into DataScraperFormatter
        // await articles
        DataScraperFormatter scraperFormatter = new(ScraperUrls.FinanceUrls);
        Console.WriteLine("Scraping articles...".Pastel(Color.Blue));
        List<ArticleData> articles = await scraperFormatter.Scrape();
        if (articles != null)
        {
            // 2) pass the articles into GeminiFlashSummarizer
            // get summaries concurrently
            Console.WriteLine("Data received. Summarizing...".Pastel(Color.Blue));
            GeminiFlashSummarizer geminiFlashSummarizer = new();
            var summarizationTasks = articles
                .Select(article => new
                {
                    Article = article,
                    SummaryTask = geminiFlashSummarizer.Summarize(article.Content)
                })
                .ToList();
            await Task.WhenAll(summarizationTasks.Select(t => t.SummaryTask));
            foreach (var item in summarizationTasks)
            {
                item.Article.Summary = item.SummaryTask.Result;
            }

            foreach (var article in articles)
            {
                Console.WriteLine("------------------");
                Console.WriteLine($"Header: {article.Header}");
                Console.WriteLine($"Content: {article.Content.Length}");
                Console.WriteLine($"Summary: {article.Summary.Length}");
                Console.WriteLine($"URL: {article.Url}");
                Console.WriteLine($"Publish Date: {article.PublishDate}");
                Console.WriteLine("------------------");
            }
        }

        // 3) pass into emailSender
    }

    public static string? LoadEnv()
    {
        Env.Load();
        string? dbConn = Environment.GetEnvironmentVariable("FINANCE_NOTIFIER_CONN_STRING");
        if (dbConn == "")
        {
            throw new HighlightedException("Failed to connect to SQLEXPRESS server.");
        }

        return dbConn;
    }

    public static void Start()
    {
        Console.WriteLine("===========================".Pastel(Color.DarkBlue));
        Console.WriteLine("Hangfire Server started.\nPress Enter to exit...".Pastel(Color.DarkBlue));
        Console.WriteLine("===========================".Pastel(Color.DarkBlue));
    }
}
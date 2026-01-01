using System.Drawing;

using Pastel;

using DotNetEnv;

using Hangfire;

namespace FinanceScraper.Src.Scheduler;

using System.Diagnostics;

using FinanceScraper.Core;

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

        // // Each week on Sunday at 6AM run a job:
        // // RecurringJob.AddOrUpdate("scrape", () => scraperFormatter.Scrape(), "0 6 * * SUN");
        // BackgroundJob.Enqueue(() => RunRecurringJob());

        // // keep server running until it is manually stopped
        // using var server = new BackgroundJobServer();
        // Console.ReadLine();

        // Run the job synchronously instead of as a background job
        Task.Run(async () =>
        {
            try
            {
                await RunRecurringJob();
                Console.WriteLine("Job completed successfully!".Pastel(Color.DarkBlue));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Job failed: {ex.Message}".Pastel(Color.Red));
            }
            finally
            {
                // Signal to exit
                Environment.Exit(0);
            }
        }).Wait(); // Wait for the task to complete
    }

    public static async Task RunRecurringJob()
    {
        // 1) pass scrapeUrls into DataScraperFormatter
        // await articles
        DataScraperFormatter scraperFormatter = new(ScraperUrls.FinanceUrls);
        Console.WriteLine("Scraping articles...".Pastel(Color.Blue));
        List<ArticleData> articles = await scraperFormatter.Scrape();
        Console.WriteLine($"Articles count: {(articles == null ? "null" : articles.Count.ToString())}".Pastel(Color.Blue));
        if (articles != null)
        {
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
        }
    }

    public static string? LoadEnv()
    {
        string? dbConn;

        // In debug mode
        if (Debugger.IsAttached)
        {
            string envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                dbConn = Environment.GetEnvironmentVariable("FINANCE_Scraper_CONN_STRING");

                if (!string.IsNullOrEmpty(dbConn))
                {
                    Console.WriteLine("Loaded from .env file".Pastel(Color.Yellow));
                    return dbConn;
                }
            }
        }

        Env.Load();
        dbConn = Environment.GetEnvironmentVariable("FINANCE_Scraper_CONN_STRING") ?? throw new HighlightedException("Failed to connect to SQLEXPRESS server.");

        return dbConn;
    }

    public static void Start()
    {
        Console.WriteLine("===========================".Pastel(Color.DarkBlue));
        Console.WriteLine("Hangfire Server started.\nPress Enter to exit...".Pastel(Color.DarkBlue));
        Console.WriteLine("===========================".Pastel(Color.DarkBlue));
    }
}
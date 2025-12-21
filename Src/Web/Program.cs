using System.Drawing;

using Pastel;

using DotNetEnv;

namespace FinanceNotifier.Web;

using System.Diagnostics;

using FinanceNotifier.Core;

public class Program
{
    public static void Main(string[] args)
    {
        // validate environment variables
        // enable cors
        // where do controllers come in? idfk
        string? dbConn = LoadEnv();


        // Print starting log
        Start();

        Task.Run(async () =>
        {
            try
            {
                await RunMainWorkflowAsync();
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

    public static async Task RunMainWorkflowAsync()
    {

        // before anything else, I have to verify that that the url is still valid
        // perfrom a health check, if you're getting 404, return some with an error message

        // 1) pass scrapeUrls into DataScraperFormatter
        // await articles
        DataScraperFormatter scraperFormatter = new(ScraperUrls.FinanceUrls);
        Console.WriteLine("Scraping articles...".Pastel(Color.Blue));
        List<ArticleData> articles = await scraperFormatter.Scrape();
        Console.WriteLine($"Articles count: {(articles == null ? "null" : articles.Count.ToString())}".Pastel(Color.Blue));
        if (articles == null || articles?.Count == 0)
        {
            Console.WriteLine("No recent articles found.");
            return;
        }
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
                dbConn = Environment.GetEnvironmentVariable("FINANCE_NOTIFIER_CONN_STRING");

                if (!string.IsNullOrEmpty(dbConn))
                {
                    Console.WriteLine("Loaded from .env file".Pastel(Color.Yellow));
                    return dbConn;
                }
            }
        }

        Env.Load();
        dbConn = Environment.GetEnvironmentVariable("FINANCE_NOTIFIER_CONN_STRING") ?? throw new HighlightedException("Failed to connect to SQLEXPRESS server.");

        return dbConn;
    }

    public static void Start()
    {
        Console.WriteLine("===========================".Pastel(Color.DarkBlue));
        Console.WriteLine("Hangfire Server started.\nPress Enter to exit...".Pastel(Color.DarkBlue));
        Console.WriteLine("===========================".Pastel(Color.DarkBlue));
    }
}
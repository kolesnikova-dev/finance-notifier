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
        BackgroundJob.Enqueue(() => Start());

        // Each week on Sunday at 6AM run a job:
        // RecurringJob.AddOrUpdate("scrape", () => scraperFormatter.Scrape(), "0 6 * * SUN");
        BackgroundJob.Enqueue(() => RunRecurringJob());

        // keep server running until it is manually stopped
        using var server = new BackgroundJobServer();
        Console.ReadLine();
    }

    public static async Task RunRecurringJob()
    {
        // 1) pass the httpClient and scrapeUrls into dataScraperFormatter
        // get information
        DataScraperFormatter scraperFormatter = new(ScraperUrls.FinanceUrls);
        Dictionary<string, string> data = await scraperFormatter.Scrape();

        Console.WriteLine("Data received: ");
        Console.WriteLine(data);
        // 2) pass the httpClient and the formatted data into GeminiFlashSummarizer
        // get information

        // 3) pass into emailSender
    }

    private static string? LoadEnv()
    {
        Env.Load();
        string? dbConn = Environment.GetEnvironmentVariable("FINANCE_NOTIFIER_CONN_STRING");
        if (dbConn == "")
        {
            throw new Exception("Failed to connect to SQLEXPRESS server.".Pastel(Color.OrangeRed));
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
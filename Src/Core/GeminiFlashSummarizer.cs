using GenerativeAI;

namespace FinanceNotifier.Core;

public class GeminiFlashSummarizer
{
    private readonly string _apiKey;
    private readonly GoogleAi _googleAI;
    public GeminiFlashSummarizer()
    {
        _apiKey = Environment.GetEnvironmentVariable("GOOGLE_GEMINI_FLASH_API_KEY")
                      ?? throw new HighlightedException("Google API key is empty.");
        _googleAI = new GoogleAi(_apiKey);

    }

    public async Task<string> Summarize(string articleContent)
    {
        var model = _googleAI.CreateGenerativeModel("models/gemini-2.5-flash");
        // calculate 50% of length of the article
        // tell gemini to summarize to that length
        int length = articleContent.Length / 2;
        // Define and pass the prompt
        string prompt = "You are an expert summarizer." +
        $"Summarize content of this article to the length of {length} characters total." +
        " If the article contains technical language, simplify it for a beginner-level audience without changing the core meaning." +
        "Recap of task: summarize to the provided length, if the content is too specialized - simplify." +
        $"Do not exceed {length} characters." +
        "The article starts after the word 'Article:'. " +
        $"Article: {articleContent}.";
        try
        {
            var googleResponse = await model.GenerateContentAsync(prompt);
            return googleResponse.Text() ?? throw new HighlightedException("Did not get a response from Gemini Flash");
        }
        catch (Exception ex)
        {
            throw new HighlightedException(ex.Message);
        }
    }
}
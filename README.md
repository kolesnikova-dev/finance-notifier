# FinanceNotifier

FinanceNotifier is a C#-based background service that scrapes important financial news, summarizes it using AI, and delivers it to my inbox weekly. 
The goal is to keep users up to date on key financial developments — without requiring them to actively monitor news sources.

## Features
- Scheduled weekly scraping job (every Sunday at 6AM EST)

- Concurrent web scraping using HttpClient and HtmlAgilityPack

- AI-generated article summaries (summarizied by Gemini Flash)

- Email delivery via SmtpClient

## Built with clean architecture and asynchronous C# practices

📌 Tech Stack
C# (.NET)

- `Hangfire` for reliable background scheduling

- HtmlAgilityPack for HTML parsing

- HttpClient for data fetching

- SmtpClient for email sending

- Gemini Flash integration


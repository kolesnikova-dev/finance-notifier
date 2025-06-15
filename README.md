# FinanceNotifier

FinanceNotifier is a C#-based background service that scrapes important financial news, summarizes it using AI, and delivers it to my inbox weekly. 
The goal is to keep users up to date on key financial developments — without requiring them to actively monitor news sources.

## Features
- Scheduled weekly scraping job (every Sunday at 6AM EST)

- Concurrent web scraping using HttpClient and HtmlAgilityPack

- AI-generated article summaries (summarizied by Gemini Flash)

- Email delivery via MailKit

## Built with clean architecture and asynchronous C# practices

📌 Tech Stack
C# (.NET)

- `Hangfire` for reliable background scheduling

- HtmlAgilityPack for HTML parsing

- HttpClient for data fetching

- MailKit for email sending

- Gemini Flash integration


## Architectural Choices

Typically, I prefer built-in solutions, however, after conducting research I made following decisions:

- Picked Handfire over System.Timers for retry support
- Picked MailKit instead of SmtpClient as SmtpClient is becoming obsolete

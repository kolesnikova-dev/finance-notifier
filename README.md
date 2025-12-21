# FinanceNotifier

FinanceNotifier is a C#-based background service that scrapes important financial news, summarizes it using AI, and delivers it to my inbox weekly. The project has been refactored into a clean, modular architecture with a separate API layer for future frontend integration.

## Architecture
The application is now structured as a modular monolith with clear separation of concerns:

### Core Module
- Contains domain models, business logic, and shared services

- Defines interfaces and contracts for the entire application

- Includes scraping, AI summarization, and email services

### Scheduler Module
- Background service using Hangfire for reliable job scheduling

- Runs the weekly scraping job (every Sunday at 6AM EST)

- Handles job retries, monitoring, and failure recovery

Note: Currently wired to use Hangfire, but designed for flexibility

### Web API Module
- REST API built with ASP.NET Core

- Provides endpoints for frontend consumption

- Will serve as the backend for a React frontend application

## API Endpoints
```
GET /api/articles/{timeperiod}
```
Returns summarized financial articles for the specified time period.

Parameters:

- timeperiod: Weekly, Monthly, or Quarterly

## Features
- Scheduled weekly scraping job (every Sunday at 6AM EST)

- Concurrent web scraping using `HttpClient` and `HtmlAgilityPack`

- AI-generated article summaries (summarizied by `Gemini Flash`)

- Email delivery via `MailKit`

## Future Development
- React Frontend: Separate client application to display articles

## Built with clean architecture and asynchronous C# practices

📌 Tech Stack
C# (.NET)

- `Hangfire` for reliable background scheduling

- `Playwright` for scraping SPAs with dynamic content

- `HtmlAgilityPack` for HTML parsing

- `HttpClient` for data fetching

- `MailKit` for email sending

- `Gemini Flash` integration


## Architectural Choices

Typically, I prefer built-in solutions, however, after conducting research I made following decisions:

- Picked `Hangfire` over `System.Timers` for retry support
- Picked `MailKit` instead of `SmtpClient` as it is becoming obsolete

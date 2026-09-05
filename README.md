# FinanceTracker API

A personal finance tracking REST API built with ASP.NET Core.

## Tech Stack
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core + SQLite
- JWT Bearer authentication (HMAC-SHA256)
- PBKDF2 password hashing with per-user salt
- OpenAPI + Scalar reference

## Features
- Registration & login; default categories are seeded for new users
- JWT auth with expiration; protected endpoints return 401 for anonymous calls
- Per-user data ownership: categories and transactions are fully isolated between accounts
- CRUD for categories and transactions with ownership validation on every input
- Reports: per-category summary and income/expense totals with date filtering

## Warning
- The secrets in appsettings.json are left for ease of use in the training project. In production, use user-secrets / environment variables

## Getting Started
```bash
dotnet ef database update
dotnet run --project FinanceTracker.Api

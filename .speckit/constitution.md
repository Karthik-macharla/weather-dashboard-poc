# SpecKit Constitution
# Project: Weather Dashboard POC
# Stack: Angular 14 + .NET 8 + Azure

---

## 1. Project Overview

This project is a Weather Dashboard application.
- Frontend: Angular 14 (TypeScript)
- Backend: .NET 8 Web API (C#)
- Database: None (external API only)
- Cloud: Microsoft Azure
- CI/CD: GitHub Actions

---

## 2. Frontend Standards (Angular 14)

- Use Angular 14 with TypeScript strict mode
- Use Angular modules (NOT standalone components — we are on Angular 14)
- Use Angular Reactive Forms for any form inputs
- Use Angular HttpClient for all HTTP calls — never use fetch() or axios
- Use Angular Services for all business logic — keep components lean
- Use RxJS Observables for async operations — always unsubscribe using takeUntil or async pipe
- Use environment.ts and environment.prod.ts for all API URLs — never hardcode URLs
- Component file naming: kebab-case (e.g. weather-card.component.ts)
- Each component must have: .ts, .html, .scss files
- No inline styles — always use .scss files
- No console.log statements in production code
- All public methods must have JSDoc comments

### Angular Folder Structure
```
src/
  app/
    core/           → singleton services, interceptors
    shared/         → reusable components, pipes, directives
    features/
      weather/      → weather feature module
        components/ → smart + dumb components
        services/   → weather.service.ts
        models/     → weather.model.ts (interfaces)
```

---

## 3. Backend Standards (.NET 8)

- Use .NET 8 Minimal API or Controller-based Web API (prefer Controllers for clarity)
- Use three-layer architecture: Controller → Service → (external API call)
- Use IHttpClientFactory for all outbound HTTP calls — never use new HttpClient()
- Use appsettings.json for configuration — API keys must come from Azure Key Vault, not hardcoded
- Use dependency injection for all services
- All controllers must have XML documentation comments
- Return consistent response format: { data, message, success }
- Handle exceptions globally using middleware — no try/catch in controllers
- Enable CORS for the Angular frontend URL only
- Use async/await throughout — no blocking calls

### .NET Folder Structure
```
WeatherDashboard.API/
  Controllers/     → WeatherController.cs
  Services/        → IWeatherService.cs, WeatherService.cs
  Models/          → WeatherResponse.cs, ApiSettings.cs
  Middleware/       → ExceptionHandlingMiddleware.cs
  Program.cs
```

---

## 4. Testing Standards

- Frontend: Use Jasmine + Karma (default Angular 14 testing)
- Backend: Use xUnit for unit tests
- Minimum test coverage: 70%
- Every service method must have at least one unit test
- Every API controller endpoint must have at least one unit test
- Use mocks/stubs for external API calls in tests — never call real APIs in tests

---

## 5. Azure Deployment Targets

- Angular frontend → Azure Static Web Apps (Free tier)
- .NET 8 API → Azure App Service (Free/Basic tier)
- API keys → Azure Key Vault
- Monitoring → Azure Application Insights (connect to both frontend and backend)
- All secrets must be stored in GitHub Secrets — never committed to the repo

---

## 6. GitHub & CI/CD Standards

- Branch naming: feature/<task-name>, fix/<issue-name>
- All changes must go through a Pull Request — no direct commits to main
- PR must pass all GitHub Actions checks before merging
- Commit messages: use conventional commits format
  (e.g. feat: add weather card component, fix: handle empty city input)
- GitHub Actions must run on every PR: build, test, lint
- GitHub Actions must deploy on every merge to main

---

## 7. Code Quality Rules

- No unused imports
- No commented-out code blocks
- No magic numbers — use named constants
- All API response models must be typed (no 'any' in TypeScript)
- ESLint must pass with zero errors (Angular default config)
- .NET code must have zero compiler warnings

---

## 8. Security Rules

- Never commit API keys, connection strings, or secrets
- Always validate user input on both frontend (Angular) and backend (.NET)
- Use HTTPS only — no HTTP endpoints in production
- CORS must only allow the specific frontend URL, not wildcard (*)

---

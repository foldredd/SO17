# SO17 — Sales Operations Workspace

SO17 is a full-stack sales-management application for maintaining customers and products, creating orders, and tracking sales activity. It provides a Ukrainian-first React interface, a secure ASP.NET Core API, and SQL Server storage.

## Highlights

- Manage customers, products, stock, prices, and order status in one workspace.
- Create orders with stock validation and atomic database updates.
- Authenticate with secure HTTP-only cookies and role-aware API endpoints.
- Use Ukrainian by default or switch the interface to English.
- Enter phone numbers through a unified country-code control with country flags.
- Display monetary values in US dollars.
- Run local setup, migrations, application startup, shutdown, and verification through PowerShell scripts.

## Technology stack

| Area | Technology |
| --- | --- |
| Frontend | React, Vite, JavaScript, CSS, Vitest, ESLint |
| Backend | ASP.NET Core, C#, Entity Framework Core, ASP.NET Core Identity |
| Database | Microsoft SQL Server / SQL Server Express |
| Tests | xUnit integration tests, Vitest component tests |
| API documentation | OpenAPI / Swagger UI |

## Project structure

```text
SO17/
├── backend/
│   ├── SO17.Api/              # HTTP API, controllers, middleware, configuration
│   ├── SO17.Application/      # Use cases, DTOs, validation, interfaces
│   ├── SO17.Domain/           # Entities and domain rules
│   └── SO17.Infrastructure/   # EF Core, SQL Server, Identity, repositories
├── frontend/
│   ├── src/                   # React UI, API client, pages, localisation
│   └── public/flags/          # Country and language flag assets
├── tests/SO17.Tests/          # Backend integration tests
├── scripts/                   # Local setup, startup, shutdown, and test scripts
├── docs/                      # Database script and technical documentation
├── SO17.sln                   # Backend solution for Visual Studio
├── Start-SO17.cmd             # Double-click launcher for Windows
└── Stop-SO17.cmd              # Double-click shutdown helper
```

## Prerequisites

Install the following tools on Windows before the first run:

- **.NET SDK 10** — verify with `dotnet --version`.
- **SQL Server Express 2025** (or compatible SQL Server) with an instance named `SQLEXPRESS`.
- **Node.js 22.12+** and **pnpm 11+** — verify with `node --version` and `pnpm --version`.
- **Visual Studio 2022** with the *ASP.NET and web development* workload, if you want to edit and debug the backend in an IDE.

The development database used by the included configuration is `SO17_Refactored` on `localhost\SQLEXPRESS`.

## Quick start

Open PowerShell in the project directory and run:

```powershell
cd "<path-to-SO17>"
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Setup-Dev.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Start-Dev.ps1
```

The setup script restores NuGet packages, installs frontend dependencies, and creates a local development configuration when required. The start script applies pending Entity Framework migrations, starts both applications, and opens the frontend.

When startup succeeds, open these addresses:

| Service | Address |
| --- | --- |
| Web application | <http://localhost:5173> |
| API liveness check | <http://localhost:5080/health/live> |
| API readiness check (includes SQL Server) | <http://localhost:5080/health/ready> |
| OpenAPI document | <http://localhost:5080/openapi/v1.json> |

You can also start the project by double-clicking [Start-SO17.cmd](Start-SO17.cmd). To stop both local processes, run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Stop-Dev.ps1
```

or double-click [Stop-SO17.cmd](Stop-SO17.cmd).

## Database and migrations

The application uses Entity Framework Core migrations. `Start-Dev.ps1` executes them automatically. To apply migrations manually:

```powershell
dotnet run --project .\backend\SO17.Api -- --migrate
```

To connect in SQL Server Management Studio, use:

| Setting | Value |
| --- | --- |
| Server name | `localhost\SQLEXPRESS` |
| Authentication | Windows Authentication |
| Database | `SO17_Refactored` |

The database is created by the migration when it does not already exist. The reference SQL schema is available in [docs/database.sql](docs/database.sql).

## Configuration and secrets

Development defaults are stored in `backend/SO17.Api/appsettings.Development.json`. Machine-specific values belong in the ignored file `backend/SO17.Api/appsettings.Local.json`; use `appsettings.Local.example.json` as a template.

Do not commit passwords, connection strings containing credentials, JWT keys, API tokens, or production certificate material. For production, provide configuration through environment variables or a secret-management service and set a trusted SQL Server certificate instead of `TrustServerCertificate=true`.

## API overview

The API is versioned under `/api` and uses JSON. Protected endpoints require an authenticated session and CSRF protection for state-changing requests.

| Area | Endpoint group | Purpose |
| --- | --- | --- |
| Authentication | `/api/auth` | Sign in, sign out, session, registration |
| Customers | `/api/customers` | Create, read, update, and delete customer records |
| Products | `/api/items` | Maintain products, price, and stock |
| Orders | `/api/orders` | Create and manage orders and their lines |
| Service status | `/health/live`, `/health/ready` | Verify API availability and SQL Server readiness |

Open the API contract at <http://localhost:5080/openapi/v1.json> while the API is running to inspect the complete endpoint and schema definition.

## Frontend behaviour

- The default interface language is **Ukrainian**; **English** is available from the selector beside the profile menu.
- The language selector uses Ukrainian and UK flag images.
- Customer phone input is a single visual control: select a country code on the left, then enter the local number on the right.
- Available default country codes are Ukraine `+380`, United Kingdom `+44`, United States `+1`, Poland `+48`, and Germany `+49`.
- Amounts are formatted as USD with the `$` symbol.

## Architecture

The backend follows a clean, layered design:

```text
SO17.Api → SO17.Application → SO17.Domain
     │              ↑
     └──── SO17.Infrastructure
```

- **Domain** defines entities and business invariants without framework dependencies.
- **Application** contains use cases, DTOs, validation, and repository contracts.
- **Infrastructure** implements persistence and identity using EF Core and SQL Server.
- **API** exposes HTTP endpoints, composes dependencies, and owns transport-level concerns.

This separation keeps business logic testable and prevents controllers from directly depending on database implementation details.

## Security measures

- Passwords are handled by ASP.NET Core Identity rather than stored directly.
- Session credentials use secure, HTTP-only cookies.
- State-changing requests require anti-forgery / CSRF validation.
- Request validation, centralized exception handling, and safe problem-details responses are enabled.
- Authentication endpoints have rate limits.
- Entity concurrency tokens protect stock and order updates from conflicting writes.
- Local secrets are excluded from Git through `.gitignore`.

## Testing and quality checks

Run the complete local verification suite:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test.ps1
```

It performs:

1. .NET restore and build.
2. Backend integration tests.
3. Frontend ESLint checks.
4. Frontend Vitest tests.
5. Frontend production build.
6. Backend and frontend production dependency audits.

For focused commands:

```powershell
dotnet test .\tests\SO17.Tests\SO17.Tests.csproj
pnpm --dir .\frontend lint
pnpm --dir .\frontend test --run
pnpm --dir .\frontend build
```

## Editing the code

Open [SO17.sln](SO17.sln) in Visual Studio to work with the backend and tests. The React application is in [frontend](frontend), which is most convenient to edit in Visual Studio Code:

```powershell
code .\frontend
```

Keep the API and frontend development servers running while editing. Vite updates browser changes automatically; ASP.NET Core reloads supported source changes during development.

## Documentation

The detailed technical documentation, including corrected issues, applied practices, architecture, security decisions, and launch instructions, is available in [SO17 technical documentation](docs/SO17_Technical_Documentation.docx).

## Production checklist

Before deployment:

- Set `ASPNETCORE_ENVIRONMENT=Production`.
- Move every secret to a secure configuration provider.
- Configure HTTPS, a public CORS origin, and secure cookie settings for the real domain.
- Use a trusted SQL Server certificate and a least-privilege database account.
- Persist ASP.NET Core data-protection keys outside the application directory.
- Enable backups, monitoring, structured logs, and alerting.
- Run the full verification suite before each release.

## License

This repository does not currently declare a license. Add one before distributing the source publicly.

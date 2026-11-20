# Clean Architecture Web API template

.NET 10 Minimal API solution with Domain, Application, Infrastructure, Api, and Shared layers. Implements CQRS with [Mediator](https://github.com/martinothamar/Mediator), EF Core (SQL Server), FluentValidation, Serilog, OpenTelemetry, and optimistic concurrency via `RowVersion`.

## Install the template

```powershell
dotnet new install "C:\work\git\source code\project-templates\dotnet-templates\CleanArchiecture.WebApi.NetCore"
```

## Create a new solution

```powershell
dotnet new ant-ca-webapi -n AcmePortal -o "C:\work\playaround\Templates\AcmePortal"
```

`-n` replaces `CompanyName` in project names, namespaces, and the SQL catalog / connection string placeholder `TemplateDatabase` (dots in the name are removed for the database and `DbContext` type).

## Run SQL Server (optional)

```powershell
docker compose up -d
```

Copy `.env.example` to `.env` and adjust `MSSQL_SA_PASSWORD` if needed.

## Run the API

```powershell
cd path\to\AcmePortal
dotnet restore
dotnet run --project src\AcmePortal.Api
```

Local development uses `appsettings.Development.json` (Docker SQL Server). Prefer **User Secrets** instead of editing committed files:

```powershell
dotnet user-secrets init --project src\<Prefix>.Api
dotnet user-secrets set "ConnectionStrings:TemplateDatabase" "Server=localhost,1433;Database=<Db>;..." --project src\<Prefix>.Api
```

### SQL Server connection string (production)

| Practice | Development | Production |
|----------|-------------|------------|
| **Secrets** | User Secrets or `appsettings.Development.json` only | Environment variable `ConnectionStrings__TemplateDatabase`, Key Vault, or platform secret store — never commit passwords |
| **Encryption** | `Encrypt=True` | `Encrypt=True` (required for Azure SQL) |
| **Trust server cert** | `TrustServerCertificate=True` for local/Docker only | `TrustServerCertificate=False` — validate TLS |
| **Account** | `sa` acceptable locally | Dedicated SQL login or [Entra ID auth](https://learn.microsoft.com/sql/connect/ado-net/sql/azure-active-directory-authentication) (`Authentication=Active Directory Managed Identity`) — not `sa` |
| **Pooling** | Defaults usually fine | Set `Max Pool Size` / `Min Pool Size` to match load; use `Application Name` for monitoring |
| **Timeouts** | `Connect Timeout=30` | Same; tune command timeout in EF if needed |
| **MARS** | `MultipleActiveResultSets=False` for EF Core unless you need MARS | Same |

Example production shape (password supplied at deploy time):

```text
Server=tcp:myserver.database.windows.net,1433;Database=MyApp;User Id=myapp_api;Password=***;Encrypt=True;TrustServerCertificate=False;Connect Timeout=30;Max Pool Size=200;Application Name=MyApp.Api;
```

EF Core uses **`AddDbContextPool`** (default pool size **128**, configurable via `Database:DbContextPoolSize`) and **transient fault retry** (`EnableRetryOnFailure`) in `Infrastructure/DependencyInjection.cs`. Disable app-level **response compression** (`ResponseCompression:Enabled`) when your reverse proxy or CDN already compresses JSON.

In **Development**, pending EF Core migrations are applied automatically on startup.

## Health endpoints

| Path | Purpose |
|------|---------|
| `/health` | All checks |
| `/health/live` | Liveness (no dependencies) |
| `/health/ready` | Readiness (`SELECT 1` against SQL Server; tagged `ready`) |

## Sample UAM API

Minimal APIs are registered automatically via `IEndpoint` (one class per use case). See [Automatically Register Minimal APIs](https://milanjovanovic.tech/blog/automatically-register-minimal-apis-in-aspnetcore).

| Method | Path | Endpoint class |
|--------|------|----------------|
| `POST` | `/api/v1/uam/staff` | `Endpoints/Uam/CreateStaffEndpoint.cs` |
| `GET` | `/api/v1/uam/users?page=1&pageSize=50` | `Endpoints/Uam/ListUsersEndpoint.cs` (paginated; `pageSize` max 100) |
| `PATCH` | `/api/v1/uam/users/{id}/status` | `Endpoints/Uam/UpdateUserStatusEndpoint.cs` |

Add a new use case by implementing `IEndpoint` under `Endpoints/<Feature>/`; it is picked up by `AddEndpoints` in `Program.cs`.

## Transactional outbox

The template uses a simple [transactional outbox](https://milanjovanovic.tech/blog/implementing-the-outbox-pattern): command handlers enqueue `INotification` messages via `IOutbox` before `SaveChangesAsync`, so business data and outbox rows commit in one transaction. `OutboxProcessorBackgroundService` polls `outbox_messages`, deserializes each message, and publishes it through Mediator (at-least-once delivery—handlers should be idempotent).

Configure polling in `appsettings.json` under `Outbox` (`PollingIntervalSeconds`, `BatchSize`, `LockDurationSeconds`, `RetryDelaySeconds`, `MaxRetryAttempts`). The worker claims rows with SQL Server `UPDLOCK`/`READPAST` so multiple API instances do not process the same message concurrently. Failed messages are retried until `MaxRetryAttempts`, then dead-lettered (`ProcessedOnUtc` set with `Error`). Sample flow: `CreateStaffCommand` enqueues `StaffCreatedEvent`; the background worker delivers it to `StaffCreatedEventHandler` (handlers should still be idempotent for at-least-once delivery).

When you add a new outbox notification type, register it in `Infrastructure/Outbox/OutboxMessageTypeRegistry.cs` and add the type to `Infrastructure/Outbox/OutboxJsonContext.cs` for source-generated JSON serialization.

## EF Core migrations

```powershell
dotnet tool install --global dotnet-ef --version 10.0.3

dotnet ef migrations add YourMigrationName `
  --project src\AcmePortal.Infrastructure `
  --startup-project src\AcmePortal.Api `
  --context AcmePortalDbContext `
  --output-dir Persistence/Migrations
```

## Uninstall

```powershell
dotnet new uninstall "C:\work\git\source code\project-templates\dotnet-templates\CleanArchiecture.WebApi.NetCore"
```

```powershell
dotnet new list ant-ca-webapi
```
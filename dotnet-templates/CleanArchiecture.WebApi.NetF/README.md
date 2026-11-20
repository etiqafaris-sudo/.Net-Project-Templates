# Clean Architecture Web API template (.NET Framework 4.8)

ASP.NET Web API 2 on IIS with Domain, Application, Infrastructure, Api, and Shared layers. Implements CQRS with [Mediator](https://github.com/martinothamar/Mediator), EF6 (SQL Server), FluentValidation, Serilog, transactional outbox, and optimistic concurrency via `RowVersion`.

Pair with [CleanArchiecture.WebApi.NetCore](../CleanArchiecture.WebApi.NetCore/README.md) for the .NET 10 Minimal API variant.

## Prerequisites

* .NET Framework 4.8 Developer Pack
* Visual Studio 2022 (or Build Tools) with **ASP.NET and web development** workload
* IIS Express (included with VS) or IIS
* [EF6 migrations](https://learn.microsoft.com/ef/ef6/modeling/code-first/migrations/) (`Update-Database` in Package Manager Console)

## Install the template

```powershell
dotnet new install "C:\work\git\source code\project-templates\dotnet-templates\CleanArchiecture.WebApi.NetF"
```

## Create a new solution

```powershell
dotnet new ant-ca-webapi-netf -n AcmePortal -o "C:\work\playaround\Templates\AcmePortal"
```

`-n` replaces `CompanyName` in project names, namespaces, and the SQL catalog / connection string placeholder `TemplateDatabase`.

## Run SQL Server (optional)

```powershell
docker compose up -d
```

Copy `.env.example` to `.env` and adjust `MSSQL_SA_PASSWORD` if needed.

Docker here is **SQL Server only**; the API is not containerized on .NET Framework.

## Run the API

Set **CompanyName.Api** as the startup project and set `connectionStrings` → `TemplateDatabase` in `Web.config` (local Docker defaults are already in the template).

### Option A: IIS Express (recommended)

1. Open `CompanyName.slnx` in Visual Studio 2022.
2. Select the **IIS Express** launch profile.
3. Press F5.

### Option B: Console self-host (no IIS)

1. Build: `dotnet build src\CompanyName.Api\CompanyName.Api.csproj`
2. Run: `src\CompanyName.Api\bin\CompanyName.Api.exe`  
   Or use the **SelfHost** launch profile (listens on `http://localhost:5080` via OWIN).

Do **not** set `ASPNETCORE_URLS` to `http://+:5080` unless you have reserved the URL. Self-host uses **localhost only** so it does not need `netsh urlacl`. If you need a wildcard binding, run an elevated PowerShell once: `scripts\Add-HttpUrlAcl.ps1`.

In **Debug**, pending EF6 migrations are applied automatically on startup.

### Connection string (production)

Keep secrets in environment variables or your secret store — never commit passwords. Use `Encrypt=True` and avoid `TrustServerCertificate=True` outside local development.

## Health endpoints

| Path | Purpose |
|------|---------|
| `/health` | All checks |
| `/health/live` | Liveness |
| `/health/ready` | Readiness (`SELECT 1` against SQL Server; tagged `ready`) |

## Sample UAM API

Thin Web API controllers are registered in DI and use attribute routing (one class per use case).

| Method | Path | Controller |
|--------|------|------------|
| `POST` | `/api/v1/uam/staff` | `Controllers/Uam/UamController.cs` |
| `GET` | `/api/v1/uam/users?page=1&pageSize=50` | `Controllers/Uam/UamController.cs` |
| `PATCH` | `/api/v1/uam/users/{id}/status` | `Controllers/Uam/UamController.cs` |

## Transactional outbox

Same pattern as the NetCore template: handlers enqueue notifications via `IOutbox` before `SaveChangesAsync`; `OutboxProcessorHostedService` polls `outbox_messages` and publishes through Mediator.

Configure polling under `appSettings` keys `Outbox:*` in `Web.config` (mirrored into `IConfiguration` at startup).

When you add a new outbox notification type, register it in `Infrastructure/Outbox/OutboxMessageTypeRegistry.cs` and add the type to `OutboxJsonContext.cs`.

## EF6 migrations

Package Manager Console (default project: **CompanyName.Infrastructure**):

```powershell
Enable-Migrations -MigrationsDirectory Persistence\Migrations -ContextTypeName CompanyName.Infrastructure.Persistence.TemplateDbContext
Add-Migration YourMigrationName -MigrationsDirectory Persistence\Migrations
Update-Database
```

Or apply at runtime in Development (already wired in `Global.asax.cs`).

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md).

## Uninstall

```powershell
dotnet new uninstall "C:\work\git\source code\project-templates\dotnet-templates\CleanArchiecture.WebApi.NetF"
```

```powershell
dotnet new list ant-ca-webapi-netf
```
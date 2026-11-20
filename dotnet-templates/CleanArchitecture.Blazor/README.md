# Clean Architecture Blazor template

Blazor Server solution with Domain, Application, Infrastructure, and Web layers.

## Install the template

```powershell
dotnet new install "C:\work\playaround\Templates\CleanArchitecture.Blazor"
```

## Create a new solution

```powershell
dotnet new ant-ca-blazor -n SampleApps -o "C:\work\playaround\Templates\SampleApps"
```

`-n` sets the project prefix (replaces `CompanyName` in names, namespaces, and file names). Prefer a single identifier without dots (for example `AcmePortal`); dotted names like `Acme.Portal` work for projects but produce awkward assembly names.

The **SQL database name**, primary `Db_Name` in `appsettings.json`, and EF Core context types use the same prefix with **dots removed** (`Acme.Portal` → database `AcmePortal`, context `AcmePortalDbContext`). Other characters in `-n` are not normalized automatically.

## Run

```powershell
cd "C:\work\playaround\Templates\AcmePortal"
dotnet restore
dotnet run --project src\AcmePortal.Web
```

Update the connection string in `src\<Prefix>.Web\appsettings.json` if needed (server, credentials). The `Database=` value and primary `Db_Name` are set from your normalized prefix when the project is created (see [Database migrations](#database-migrations-ef-core)).

## Structured logging (Serilog and Seq)

The Web project uses Serilog with console and rolling file sinks (`logs/info`, `logs/error`). Optional **Seq** is enabled in `appsettings.json` under `Serilog:Seq` (`ServerUrl`: `http://localhost:5341`). Run Seq locally when you want search and filtering (not included in `docker-compose.yml`):

```powershell
docker run --rm -e ACCEPT_EULA=Y -p 5341:5341 -p 8081:80 datalust/seq:latest
```

Open Seq at `http://localhost:8081` (or the ingestion port UI your install exposes). Useful filters:

- `CorrelationId = '<id from response header X-Correlation-Id>'`
- `@MessageTemplate like '%LoginQuery%'`
- `Errors is not null` (failed Mediator `Result` handlers)

HTTP requests get a correlation ID via middleware; Mediator commands and queries log start/complete at Information, with failures including an `Errors` scope.

## Database migrations (EF Core)

This template ships with EF Core migrations in `src\<Prefix>.Infrastructure\Persistence\Migrations\Application` for `<NormalizedPrefix>DbContext` (for example `AcmePortalDbContext` when `-n Acme.Portal`). After `dotnet new`, `<Prefix>` is the name you passed to `-n`. In this template repository the placeholders are `CompanyName` (prefix), `TemplateDatabase` (SQL catalog / `Db_Name`), and `TemplateDbContext` (EF context type).

Install the EF CLI once (match the EF Core version in the Infrastructure project, currently 10.0.3):

```powershell
dotnet tool install --global dotnet-ef --version 10.0.3
```

From the solution root, use these parameters on every command:

| Parameter | Example |
|-----------|---------|
| `--project` | `src\AcmePortal.Infrastructure` |
| `--startup-project` | `src\AcmePortal.Web` |
| `--context` | `AcmePortalDbContext` (normalized prefix + `DbContext`) |

### List migrations (including pending)

Migrations that exist in code but are not applied to the database yet are marked `(Pending)`:

```powershell
dotnet ef migrations list `
  --project src\AcmePortal.Infrastructure `
  --startup-project src\AcmePortal.Web `
  --context AcmePortalDbContext
```

Example output:

```text
20260915214932_InitialApplication (Pending)
20260917063057_UpdateApplication (Pending)
```

Applied migrations appear without `(Pending)`.

### Apply pending migrations

Apply all pending migrations to the database configured in `appsettings.json`:

```powershell
dotnet ef database update `
  --project src\AcmePortal.Infrastructure `
  --startup-project src\AcmePortal.Web `
  --context AcmePortalDbContext
```

To apply up to a specific migration, pass the migration name as the last argument instead of omitting it.

### Add a migration after model changes

When you change domain entities or EF configuration in a generated solution, add a new migration:

```powershell
dotnet ef migrations add YourMigrationName `
  --project src\AcmePortal.Infrastructure `
  --startup-project src\AcmePortal.Web `
  --context AcmePortalDbContext `
  --output-dir Persistence/Migrations/Application
```

### Development startup

In **Development**, the Web project also calls `Database.Migrate()` on startup, so pending migrations may be applied automatically when you `dotnet run`. For other environments, run `dotnet ef database update` explicitly (or apply migrations in your deployment pipeline).

## Uninstall

```powershell
dotnet new uninstall "C:\work\playaround\Templates\CleanArchitecture.Blazor"
```

## List installed templates

```powershell
dotnet new list ant-ca-blazor
```


## Scaffold database tables to code

Requires [dotnet-ef](#database-migrations-ef-core). Example uses the template repo prefix `CompanyName`; replace with your `<Prefix>` in generated solutions.

```shell
dotnet ef dbcontext scaffold "Server=localhost,1433;Database=Testing;User Id=sa;Password=P@ssw0rd;Encrypt=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --project src\CompanyName.Infrastructure --startup-project src\CompanyName.Web --output-dir Persistence\Entities --context-dir Persistence --context TestingDbContext --namespace CompanyName.Infrastructure.Persistence.Entities --context-namespace CompanyName.Infrastructure.Persistence --no-onconfiguring --force
```
# .NET Project Templates

Reusable [`dotnet new`](https://learn.microsoft.com/dotnet/core/tools/dotnet-new) project templates for Clean Architecture backends and Blazor apps. Source: [github.com/etiqafaris-sudo/.Net-Project-Templates](https://github.com/etiqafaris-sudo/.Net-Project-Templates).

Shared design goals and layer rules are described in [ARCHITECTURE.md](./ARCHITECTURE.md).

## Repository layout

| Path | Template short name | Description |
|------|---------------------|-------------|
| [dotnet-templates/CleanArchiecture.WebApi.NetCore](./dotnet-templates/CleanArchiecture.WebApi.NetCore/) | `ant-ca-webapi` | .NET 10 Minimal API — CQRS (Mediator), EF Core, FluentValidation, Serilog, OpenTelemetry |
| [dotnet-templates/CleanArchiecture.WebApi.NetF](./dotnet-templates/CleanArchiecture.WebApi.NetF/) | `ant-ca-webapi-netf` | ASP.NET Web API 2 on .NET Framework 4.8 / IIS — EF6, transactional outbox |
| [dotnet-templates/CleanArchitecture.Blazor](./dotnet-templates/CleanArchitecture.Blazor/) | `ant-ca-blazor` | Blazor Server — Domain, Application, Infrastructure, Web |

Each folder has its own **README** with runbooks, health checks, and sample APIs.

## Clone

```powershell
git clone https://github.com/etiqafaris-sudo/.Net-Project-Templates.git
cd .Net-Project-Templates
```

To point an existing local repo at GitHub:

```powershell
git remote add origin https://github.com/etiqafaris-sudo/.Net-Project-Templates.git
git branch -M main
git push -u origin main
```

## Install templates

From the repository root (adjust the drive/path if you cloned elsewhere):

```powershell
$root = (Get-Location).Path

dotnet new install "$root\dotnet-templates\CleanArchiecture.WebApi.NetCore"
dotnet new install "$root\dotnet-templates\CleanArchiecture.WebApi.NetF"
dotnet new install "$root\dotnet-templates\CleanArchitecture.Blazor"
```

Update after pulling changes:

```powershell
dotnet new install "$root\dotnet-templates\CleanArchiecture.WebApi.NetCore" --force
```

## Create a new solution

Replace `AcmePortal` and the output path with your names:

```powershell
dotnet new ant-ca-webapi      -n AcmePortal -o C:\work\AcmePortal
dotnet new ant-ca-webapi-netf -n AcmePortal -o C:\work\AcmePortalNetF
dotnet new ant-ca-blazor      -n AcmePortal -o C:\work\AcmePortalBlazor
```

The `-n` value replaces the `CompanyName` placeholder in projects, namespaces, and the SQL catalog name (`TemplateDatabase`); dots in the name are removed for database and `DbContext` types.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (NetCore and Blazor templates)
- .NET Framework 4.8 Developer Pack and Visual Studio 2022 with the ASP.NET workload (NetF template)
- Docker (optional) for local SQL Server via each template’s `docker-compose.yml`

## Contributing

Open issues or pull requests on [GitHub](https://github.com/etiqafaris-sudo/.Net-Project-Templates/issues).
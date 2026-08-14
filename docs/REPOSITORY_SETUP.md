# BIMSS Repository Setup

## Suggested repository name

`bi-buklod-bimss`

Alternative: `bimss`

## Create the solution

Example .NET CLI commands:

```powershell
mkdir bi-buklod-bimss
cd bi-buklod-bimss

dotnet new sln -n Bimss

dotnet new mvc -n Bimss.Web -o src/Bimss.Web -f net10.0
dotnet new webapi -n Bimss.Api -o src/Bimss.Api -f net10.0 --use-controllers
dotnet new classlib -n Bimss.Application -o src/Bimss.Application -f net10.0
dotnet new classlib -n Bimss.Domain -o src/Bimss.Domain -f net10.0
dotnet new classlib -n Bimss.Infrastructure -o src/Bimss.Infrastructure -f net10.0
dotnet new classlib -n Bimss.Contracts -o src/Bimss.Contracts -f net10.0

dotnet new xunit -n Bimss.UnitTests -o tests/Bimss.UnitTests -f net10.0
dotnet new xunit -n Bimss.IntegrationTests -o tests/Bimss.IntegrationTests -f net10.0
dotnet new xunit -n Bimss.ArchitectureTests -o tests/Bimss.ArchitectureTests -f net10.0

dotnet sln add src/Bimss.Web/Bimss.Web.csproj
dotnet sln add src/Bimss.Api/Bimss.Api.csproj
dotnet sln add src/Bimss.Application/Bimss.Application.csproj
dotnet sln add src/Bimss.Domain/Bimss.Domain.csproj
dotnet sln add src/Bimss.Infrastructure/Bimss.Infrastructure.csproj
dotnet sln add src/Bimss.Contracts/Bimss.Contracts.csproj
dotnet sln add tests/Bimss.UnitTests/Bimss.UnitTests.csproj
dotnet sln add tests/Bimss.IntegrationTests/Bimss.IntegrationTests.csproj
dotnet sln add tests/Bimss.ArchitectureTests/Bimss.ArchitectureTests.csproj
```

## Project references

```powershell
dotnet add src/Bimss.Application/Bimss.Application.csproj reference src/Bimss.Domain/Bimss.Domain.csproj

dotnet add src/Bimss.Infrastructure/Bimss.Infrastructure.csproj reference src/Bimss.Domain/Bimss.Domain.csproj
dotnet add src/Bimss.Infrastructure/Bimss.Infrastructure.csproj reference src/Bimss.Application/Bimss.Application.csproj

dotnet add src/Bimss.Web/Bimss.Web.csproj reference src/Bimss.Application/Bimss.Application.csproj
dotnet add src/Bimss.Web/Bimss.Web.csproj reference src/Bimss.Infrastructure/Bimss.Infrastructure.csproj

dotnet add src/Bimss.Api/Bimss.Api.csproj reference src/Bimss.Application/Bimss.Application.csproj
dotnet add src/Bimss.Api/Bimss.Api.csproj reference src/Bimss.Infrastructure/Bimss.Infrastructure.csproj
dotnet add src/Bimss.Api/Bimss.Api.csproj reference src/Bimss.Contracts/Bimss.Contracts.csproj
```

## EF Core SQL Server

Add current compatible 10.0.x packages to `Bimss.Infrastructure`:

```powershell
dotnet add src/Bimss.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.*
dotnet add src/Bimss.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 10.0.*
```

Pin actual package versions in source control rather than leaving wildcards in the final project.

### EF Core tooling (dotnet-ef)

A local, repo-pinned `dotnet-ef` tool is checked in via `.config/dotnet-tools.json`,
matching the `Microsoft.EntityFrameworkCore` package version. This avoids relying
on whatever `dotnet-ef` version (if any) happens to be installed globally on a
contributor's machine. Restore it once per clone:

```powershell
dotnet tool restore
```

`Bimss.Web` is the designated EF Core startup project (it references
`Microsoft.EntityFrameworkCore.Design` for this purpose); `BimssDbContext` itself
lives in `Bimss.Infrastructure`. Add/apply migrations with:

```powershell
dotnet tool run dotnet-ef migrations add <Name> --project src/Bimss.Infrastructure --startup-project src/Bimss.Web --output-dir Persistence/Migrations
dotnet tool run dotnet-ef database update --project src/Bimss.Infrastructure --startup-project src/Bimss.Web
```

## Initial branches

Recommended:

```text
main
develop (optional; use only if your team actually needs it)
feature/*
fix/*
chore/*
```

A simpler GitHub flow with short-lived feature branches directly into `main` is also valid.

## First repository protection

- Require pull requests to `main`
- Require build/test checks
- Require at least one reviewer when more developers join
- Block force pushes to protected branches
- Enable Dependabot alerts/updates as appropriate
- Enable CodeQL/default code scanning where available
- Store environment secrets in GitHub/IIS/approved secret stores, not appsettings committed to Git

## Environment files

Real `appsettings.json` and `appsettings.Development.json` files are **never
committed**, for either `Bimss.Web` or `Bimss.Api`. They are expected to hold
connection strings, API tokens, and other secrets as the application grows,
so both are git-ignored outright rather than trusted to stay secret-free.

Commit only safe example templates instead:

```text
appsettings.json.example
appsettings.Development.json.example
```

### Local developer setup

For both `src/Bimss.Web` and `src/Bimss.Api`, copy each `*.example` file to
its real name in the same folder, then edit the real file directly with your
local values, including connection strings and any other local secrets:

```powershell
Copy-Item src/Bimss.Web/appsettings.json.example src/Bimss.Web/appsettings.json
Copy-Item src/Bimss.Web/appsettings.Development.json.example src/Bimss.Web/appsettings.Development.json
```

That's the whole setup — no `dotnet user-secrets` step is required. The real
files are git-ignored (see above), so anything written into them, secrets
included, never reaches git regardless of where they're edited.

### Common local connection error: SSL certificate chain

```text
A connection was successfully established with the server, but then an error
occurred during the login process. (provider: SSL Provider, error: 0 - The
certificate chain was issued by an authority that is not trusted.)
```

`Microsoft.Data.SqlClient` 3.x+ defaults to `Encrypt=True` and validates the
server's TLS certificate. A local SQL Server (LocalDB, Developer Edition, a
Docker container) typically presents a self-signed certificate, which fails
that validation. Add `TrustServerCertificate=True` to the local
`ConnectionStrings:Bimss` value in your real, git-ignored
`appsettings.Development.json`:

```text
Server=localhost;Database=Bimss;Trusted_Connection=True;TrustServerCertificate=True;
```

Local-dev only — a real production connection string should have a properly
trusted certificate instead of bypassing validation.

### Deploying to IIS

The same pattern carries over to deployment: the published build never
includes a real `appsettings*.json`, since those files are git-ignored and
therefore never part of the repository or the build output pulled from it.
Place the real, environment-specific file directly on the server as part of
the deployment step:

- Maintain a real `appsettings.Production.json` (or `appsettings.json`) for
  each server, containing that environment's actual connection strings and
  secrets, and copy it into the deployed site's folder alongside the
  published app. ASP.NET Core picks up `appsettings.{Environment}.json`
  automatically based on `ASPNETCORE_ENVIRONMENT`.
- Never commit that file, generate it from a committed template with real
  values filled in via a script, or store it anywhere `git status` would see
  it inside the repository working tree.

Never add a connection string, API token, or credential to a committed
`appsettings*.json` or `*.example` file — `.example` files stay placeholder
templates only. When a feature introduces a new config section (e.g. a
connection string once EF Core is wired up), add the *key name* with an
empty/placeholder value to the `.example` template so other developers know
what to fill in, never the real value.

## Do not commit

```text
real membership Excel exports
database backups
production connection strings
uploaded member documents
loan exports
election exports containing voter/member data
secrets/tokens
```

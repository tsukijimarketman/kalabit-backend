# Development Commands

Run from the repository root in PowerShell.

## Docker

```
docker compose up -d postgres mailpit
docker compose ps
docker compose logs -f postgres
```

## Tests

Run all tests:

```
dotnet test NotificationPlatform.sln
```

Run template renderer tests:

```
dotnet test tests\NotificationPlatform.UnitTests\NotificationPlatform.UnitTests.csproj --filter FullyQualifiedName~TemplateRendererTests
```

## Coverage

Run tests and generate the coverage report:

```
.\scripts\test-coverage.ps1
```

Report location:

```
TestResults\CoverageReport\index.html
```

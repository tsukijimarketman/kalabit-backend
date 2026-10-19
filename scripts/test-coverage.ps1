$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot 'NotificationPlatform.sln'
$resultsRoot = Join-Path $repoRoot 'TestResults'
$runResultsPath = Join-Path $resultsRoot (Get-Date -Format 'yyyyMMdd-HHmmss')
$reportPath = Join-Path $resultsRoot 'CoverageReport'

dotnet test $solutionPath `
    --collect 'XPlat Code Coverage' `
    --results-directory $runResultsPath

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$coverageFile = Get-ChildItem -LiteralPath $runResultsPath `
    -Filter 'coverage.cobertura.xml' `
    -Recurse -File |
    Select-Object -First 1

if ($null -eq $coverageFile) {
    throw "Coverage XML was not created under $runResultsPath"
}

dotnet tool run reportgenerator -- `
    "-reports:$($coverageFile.FullName)" `
    "-targetdir:$reportPath" `
    '-reporttypes:Html'

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Coverage report: $reportPath\index.html"

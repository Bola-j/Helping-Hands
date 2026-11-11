param(
    [string]$EnvironmentFile = (Join-Path $PSScriptRoot "../.env"),
    [switch]$Migrate,
    [switch]$SeedDemo
)
$ErrorActionPreference = "Stop"
if (-not (Test-Path -LiteralPath $EnvironmentFile -PathType Leaf)) {
    throw "Missing .env file. Copy .env.example to .env and edit it first."
}
$original = @{}
try {
    foreach ($line in Get-Content -LiteralPath $EnvironmentFile) {
        $line = $line.Trim()
        if (-not $line -or $line.StartsWith('#')) { continue }
        if ($line -notmatch '^([A-Za-z_][A-Za-z0-9_]*)=(.*)$') {
            throw "Invalid .env line; expected an unquoted KEY=VALUE pair."
        }
        $key = $Matches[1]
        $value = $Matches[2]
        if (-not $original.ContainsKey($key)) {
            $original[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
        }
        [Environment]::SetEnvironmentVariable($key, $value, 'Process')
    }
    $project = Join-Path $PSScriptRoot '../src/HelpingHands.Api/HelpingHands.Api.csproj'
    if ($Migrate -and $SeedDemo) { throw "Choose either -Migrate or -SeedDemo." }
    if ($Migrate) {
        & dotnet ef database update --project $project --startup-project $project
    } elseif ($SeedDemo) {
        & dotnet run --project $project --no-launch-profile -- --seed-demo
    } else {
        & dotnet run --project $project --no-launch-profile
    }
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    foreach ($key in $original.Keys) {
        [Environment]::SetEnvironmentVariable($key, $original[$key], 'Process')
    }
}

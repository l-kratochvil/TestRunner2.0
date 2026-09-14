# Spustí TestRunner.WebApp z příkazové řádky.
# Použití:
#   .\tools\scripts\run-webapp.ps1            # profil http (http://localhost:5078)
#   .\tools\scripts\run-webapp.ps1 https      # profil https
#   .\tools\scripts\run-webapp.ps1 -Watch     # dotnet watch (hot reload)
param(
    [string]$Profile = "http",
    [switch]$Watch
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$project = Join-Path $repoRoot "TestRunner.WebApp"

if ($Watch) {
    dotnet watch --project $project --launch-profile $Profile
} else {
    dotnet run --project $project --launch-profile $Profile
}

# Spustí TestRunner.WebApp v režimu watch (hot reload).
# Použití:
#   .\tools\scripts\run-webapp-watch.ps1          # profil http (http://localhost:5078)
#   .\tools\scripts\run-webapp-watch.ps1 https    # profil https
param(
    [string]$Profile = "http"
)

& (Join-Path $PSScriptRoot "run-webapp.ps1") -Profile $Profile -Watch

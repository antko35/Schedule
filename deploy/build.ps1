# Builds all service images and packs everything into deploy\bundle\ -
# a self-contained folder you can copy to another computer (that only needs Docker).
#
# Usage:
#   .\build.ps1                 # build images + create offline bundle (images.tar)
#   .\build.ps1 -WithFrontend   # also build the Electron desktop app installer
#   .\build.ps1 -NoImageExport  # skip docker save (target machine will build from source)

param(
    [switch]$WithFrontend,
    [switch]$NoImageExport
)

$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot

$bundle = Join-Path $PSScriptRoot "bundle"
$images = @(
    "schedule/user-service:local",
    "schedule/schedule-service:local",
    "schedule/user-management-service:local"
)

Write-Host "==> Building service images..." -ForegroundColor Cyan
docker compose build
if ($LASTEXITCODE -ne 0) { throw "docker compose build failed" }

Write-Host "==> Preparing bundle folder: $bundle" -ForegroundColor Cyan
if (Test-Path $bundle) { Remove-Item $bundle -Recurse -Force }
New-Item -ItemType Directory -Path $bundle | Out-Null

Copy-Item "docker-compose.yml" $bundle
Copy-Item "run.ps1"            $bundle
Copy-Item "README.md"          $bundle -ErrorAction SilentlyContinue
Copy-Item "keycloak"           (Join-Path $bundle "keycloak") -Recurse

if (-not $NoImageExport) {
    Write-Host "==> Exporting images to images.tar (this can take a few minutes)..." -ForegroundColor Cyan
    docker save -o (Join-Path $bundle "images.tar") $images
    if ($LASTEXITCODE -ne 0) { throw "docker save failed" }
}

if ($WithFrontend) {
    Write-Host "==> Building Electron desktop app..." -ForegroundColor Cyan
    Push-Location (Join-Path $PSScriptRoot "..\Front\electron-vue")
    npm install
    npm run build
    Pop-Location
    $dist = Join-Path $PSScriptRoot "..\Front\electron-vue\release"
    if (-not (Test-Path $dist)) { $dist = Join-Path $PSScriptRoot "..\Front\electron-vue\dist" }
    if (Test-Path $dist) {
        Copy-Item $dist (Join-Path $bundle "desktop-app") -Recurse
        Write-Host "    Desktop build copied to bundle\desktop-app" -ForegroundColor Green
    } else {
        Write-Warning "Electron output folder not found - check electron-builder config."
    }
}

Write-Host ""
Write-Host "Done. Copy the entire '$bundle' folder to the target machine and run run.ps1 there." -ForegroundColor Green

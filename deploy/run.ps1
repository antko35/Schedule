# Starts the whole solution on this machine. Requires only Docker Desktop.
#
# Usage:
#   .\run.ps1            # load images.tar if present, then start everything
#   .\run.ps1 -Build     # build images from source instead of using images.tar
#   .\run.ps1 -Down      # stop and remove all containers
#   .\run.ps1 -Logs      # follow logs

param(
    [switch]$Build,
    [switch]$Down,
    [switch]$Logs
)

$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot

if ($Down) {
    docker compose down
    return
}

if ($Logs) {
    docker compose logs -f
    return
}

$tar = Join-Path $PSScriptRoot "images.tar"
if ((Test-Path $tar) -and (-not $Build)) {
    Write-Host "==> Loading prebuilt images from images.tar..." -ForegroundColor Cyan
    docker load -i $tar
}

Write-Host "==> Starting all services..." -ForegroundColor Cyan
if ($Build) {
    docker compose up -d --build
} else {
    docker compose up -d
}

Write-Host ""
Write-Host "All services are starting. Endpoints:" -ForegroundColor Green
Write-Host "  UserService API            http://localhost:5001/swagger"
Write-Host "  ScheduleService API        http://localhost:5002  (Swagger at /)"
Write-Host "  UserManagementService API  http://localhost:5003/swagger"
Write-Host "  Keycloak admin             http://localhost:8080  (admin/admin)"
Write-Host "  RabbitMQ UI                http://localhost:15672 (guest/guest)"
Write-Host "  pgAdmin                    http://localhost:5050"
Write-Host "  Mongo Express (schedule)   http://localhost:8082"
Write-Host "  Mongo Express (management) http://localhost:8081"
Write-Host ""
Write-Host "Follow logs:  .\run.ps1 -Logs    Stop:  .\run.ps1 -Down" -ForegroundColor DarkGray

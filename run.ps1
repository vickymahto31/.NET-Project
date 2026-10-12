$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet"
$env:PATH = "$env:USERPROFILE\.dotnet;$env:PATH"

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " WhisperSpace - Anonymous Student Platform (.NET Core)" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Starting web server on http://localhost:5000 ..." -ForegroundColor Green
dotnet run --urls=http://localhost:5000

@echo off
set "DOTNET_ROOT=%USERPROFILE%\.dotnet"
set "PATH=%USERPROFILE%\.dotnet;%PATH%"
echo ========================================================
echo  WhisperSpace - Anonymous Student Platform (.NET Core)
echo ========================================================
echo Starting web server on http://localhost:5000 ...
dotnet run --urls=http://localhost:5000
pause

@echo off
setlocal

set CONFIG=Release
if "%1%" == "debug" set CONFIG=Debug

echo.
echo Building for configuration: %CONFIG%
echo.

dotnet build StreamDeckSimHub.Plugin\StreamDeckSimHub.Plugin.csproj -c %CONFIG%
dotnet publish StreamDeckSimHub.Plugin\StreamDeckSimHub.Plugin.csproj -c %CONFIG%
dotnet build StreamDeckSimHub.Installer\StreamDeckSimHub.Installer.csproj -c %CONFIG%
dotnet publish StreamDeckSimHub.Installer\StreamDeckSimHub.Installer.csproj -c %CONFIG%

for /f "tokens=*" %%a in ('dir /b /od StreamDeckSimHub.Installer\bin\%Config%\net8.0-windows\win-x64\publish\*.exe') do set newest=%%a
copy StreamDeckSimHub.Installer\bin\%Config%\net8.0-windows\win-x64\publish\%newest% build\


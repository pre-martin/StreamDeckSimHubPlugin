@echo off
setlocal

rem Script to build the plugin and the instaler.
rem
rem Requires Node with StreamDeck CLI installed. See "doc/Release.adoc"

set CONFIG=Release
if "%1%" == "debug" set CONFIG=Debug

echo.
echo Building for configuration: %CONFIG%
echo.

dotnet build StreamDeckSimHub.Plugin\StreamDeckSimHub.Plugin.csproj -c %CONFIG%
dotnet publish StreamDeckSimHub.Plugin\StreamDeckSimHub.Plugin.csproj -c %CONFIG%
if %errorlevel% neq 0 (
    echo Failed to build/publish StreamDeckSimHub.Plugin
    exit /b %errorlevel%
)

dotnet build StreamDeckSimHub.Installer\StreamDeckSimHub.Installer.csproj -c %CONFIG%
if %errorlevel% neq 0 (
    echo Failed to build StreamDeckSimHub.Installer
    exit /b %errorlevel%
)
for /f "tokens=*" %%a in ('dir /b /od StreamDeckSimHub.Installer\bin\%Config%\*.exe') do set newest=%%a
copy StreamDeckSimHub.Installer\bin\%Config%\%newest% build\



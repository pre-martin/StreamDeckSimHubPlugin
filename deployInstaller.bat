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

cd StreamDeckSimHub.Installer
"C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\amd64\MSBuild.exe" /p:Configuration=%CONFIG% /p:Platform="Any CPU"
cd ..
for /f "tokens=*" %%a in ('dir /b /od StreamDeckSimHub.Installer\bin\%Config%\*.exe') do set newest=%%a
copy StreamDeckSimHub.Installer\bin\%Config%\%newest% build\

rem Build for Installer with .NET 8.0
rem dotnet build StreamDeckSimHub.Installer\StreamDeckSimHub.Installer.csproj -c %CONFIG%
rem dotnet publish StreamDeckSimHub.Installer\StreamDeckSimHub.Installer.csproj -c %CONFIG%

rem for /f "tokens=*" %%a in ('dir /b /od StreamDeckSimHub.Installer\bin\%Config%\net8.0-windows\win-x64\publish\*.exe') do set newest=%%a
rem copy StreamDeckSimHub.Installer\bin\%Config%\net8.0-windows\win-x64\publish\%newest% build\


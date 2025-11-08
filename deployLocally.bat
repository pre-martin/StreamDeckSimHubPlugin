@echo off
setlocal

rem Script to build deploy locally.
rem Also allows to deploy with the same "version" in manifest.json as already installed.
rem
rem Requires Node with StreamDeck CLI installed. See "doc/Release.adoc"

set CONFIG=Release
if "%1%" == "debug" set CONFIG=Debug

echo.
echo Building for configuration: %CONFIG%
echo.


dotnet build StreamDeckSimHub.Plugin\StreamDeckSimHub.Plugin.csproj -c %CONFIG%
dotnet publish StreamDeckSimHub.Plugin\StreamDeckSimHub.Plugin.csproj -c %CONFIG%
if %errorlevel% neq 0 exit /b 1



taskkill /im StreamDeck.exe /t /f
timeout 1 > nul

cd build
xcopy net.planetrenner.simhub.sdPlugin "%AppData%\Elgato\StreamDeck\Plugins\net.planetrenner.simhub.sdPlugin\" /e /y /q
cd ..

start /d "%ProgramFiles%\Elgato\StreamDeck" StreamDeck.exe

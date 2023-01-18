@echo off
setlocal

set CONFIG=Release
if "%1%" == "debug" set CONFIG=Debug

echo.
echo Building for configuration: %CONFIG%
echo.

dotnet publish StreamDeckSimHub.Plugin\StreamDeckSimHub.Plugin.csproj -c %CONFIG% -r win-x64

if exist build rmdir /s /q build
if exist build goto :endFailure
mkdir build
xcopy StreamDeckSimHub.Plugin\bin\%CONFIG%\net6.0\win-x64\publish build\net.planetrenner.simhub.sdPlugin /e /i

cd build
..\..\DistributionTool.exe -b -i net.planetrenner.simhub.sdPlugin -o .
goto end

:endFailure
exit /b 1

:end

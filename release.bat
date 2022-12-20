@echo off
setlocal

dotnet publish StreamDeckSimHub.Plugin\StreamDeckSimHub.Plugin.csproj -c Release -r win-x64 --self-contained

if exist build rmdir /s /q build
if exist build goto :endFailure
mkdir build
xcopy StreamDeckSimHub.Plugin\bin\Release\net6.0\win-x64\publish build\net.planetrenner.simhub.sdPlugin /e /i

cd build
..\..\DistributionTool.exe -b -i net.planetrenner.simhub.sdPlugin -o .
goto end

:endFailure
exit /b 1

:end

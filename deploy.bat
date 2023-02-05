@echo off
setlocal

rem Script to deploy locally.
rem Also allows to deploy with the same "version" in manifest.json as already installed.

taskkill /im StreamDeck.exe /t /f
timeout 1 > nul

cd build
xcopy net.planetrenner.simhub.sdPlugin "%AppData%\Elgato\StreamDeck\Plugins\net.planetrenner.simhub.sdPlugin\" /e /y
cd ..

start /d "%ProgramFiles%\Elgato\StreamDeck" StreamDeck.exe

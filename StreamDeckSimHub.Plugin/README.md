# Stream Deck SimHub Plugin - Binary Part

## About

This is the binary part of the plugin for [Stream Deck](https://www.elgato.com/stream-deck). It offers Stream Deck actions, which are updating their state from [SimHub](https://www.simhubdash.com/).

This means, that actions can be bound to SimHub properties. If the value of a bound SimHub property changes, the button in Stream Deck will reflect this property change.

This plugin depends on the [SimHub Property Server plugin](https://github.com/pre-martin/SimHubPropertyServer), which has to be installed in SimHub.


## Building

For the "Debug" build:

```
dotnet publish -o ..\net.planetrenner.simhub.sdPlugin\plugin\
```

For the "Release" build

```
dotnet publish -o ..\net.planetrenner.simhub.sdPlugin\plugin\ -c Release
```

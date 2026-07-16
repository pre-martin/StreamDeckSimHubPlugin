# Tote TCP-Verbindung zum SimHub Property Server

## Problem

Die TCP-Verbindung zwischen diesem Plugin (`SimHubConnection.cs`) und dem SimHub Property Server
kann in einen "toten" Zustand geraten, ohne dass der Client das erkennt.

**Beobachtung:** Im Server-Log erscheint `"We will disconnect the client"`, aber im Client-Log
ist danach nichts zu sehen – kein Reconnect, keine Fehlermeldung.

## Ursache

### Hauptproblem: `ReadLineAsync()` ohne Timeout

Der Client wartet in `ReadFromServer()` (`SimHubConnection.cs`) dauerhaft auf Daten:

```csharp
while ((line = await reader.ReadLineAsync()) != null)
```

Das Subscribe-Verfahren erlaubt es, dass nach dem initialen Subscribe über lange Zeiträume
nichts vom Server gesendet wird – ein einfacher Timeout auf `ReadLineAsync()` scheidet daher aus.

### Das eigentliche Szenario

Eine TCP-Verbindung kann auf zwei Arten "tot" werden:

- **Szenario A (sauberer Close):** Server sendet FIN/RST → `ReadLineAsync()` gibt `null` zurück
  oder wirft `IOException` → Client reconnectet. **Funktioniert korrekt.**

- **Szenario B (half-open / Netzwerkunterbrechung):** Die TCP-Verbindung erscheint noch offen
  (kein FIN/RST), aber Pakete kommen nicht mehr durch. Beispiele: SimHub-Absturz ohne sauberes
  Socket-Close, Firewall verwirft Pakete still, Netzwerkkarte resettet.

  In diesem Szenario:
  - Der **Server** bemerkt den Fehler sofort beim nächsten **Schreibversuch** (Exception)
  - Der **Client** wartet in `ReadLineAsync()` **ewig**, da ein Read erst zurückkommt, wenn
    Daten ankommen oder die Verbindung sauber geschlossen wird
  - Kein Log, kein Reconnect auf Client-Seite

### Nebenproblem: `_tcpClient.Close()` wird verzögert aufgerufen

In `Client.cs:Disconnect()` (Server-Projekt) wird `_tcpClient.Close()` erst **nach** dem
Unsubscribe aller Subscriptions aufgerufen. Falls `_subscriptionManager.Unsubscribe()` blockiert
oder langsam ist, verzögert sich der eigentliche Socket-Close – und damit der Moment, an dem
Szenario A auf Client-Seite greift.

## Gewählter Lösungsansatz: Applikations-Level Heartbeat (Ping)

TCP Keep-Alive (OS-Ebene) wäre eine Alternative, die nur Client-Änderungen erfordert, aber
OS-abhängige Timings hat. Der Heartbeat-Mechanismus ist robuster und vollständig kontrollierbar.

### Protokollerweiterung

Der Server sendet periodisch alle **30 Sekunden** bei Inaktivität eine `ping`-Zeile.
Der Client antwortet **nicht** mit `pong`. Stattdessen reicht es, dass der Client überhaupt
**irgendeine** Zeile empfängt – denn jeder empfangene `ping` beweist, dass die Verbindung lebt.
Der Client trackt den Timestamp der letzten empfangenen Nachricht und reconnectet, wenn zu lange
nichts kam.

Der Client aktiviert den Heartbeat nur, wenn der Server im Connect-String eine Version **>= v1.6.0**
meldet. Bei älteren Server-Versionen (oder fehlendem Versions-String) bleibt der Heartbeat
deaktiviert.

### Änderungen Server (`SimHubPropertyServer`)

- `Client.cs`: Neues Feld `_lastSentTicks` trackt den Timestamp der zuletzt gesendeten Nachricht.
- `Client.cs`: `SendString()` aktualisiert `_lastSentTicks` nach jedem erfolgreichen Senden.
- `Client.cs`: `PingLoopAsync()`-Task startet parallel zu `Start()`, prüft alle 5 s ob seit
  mehr als 30 s keine Nachricht gesendet wurde, und schickt dann `ping\r\n`.
  Wird beim Beenden von `Start()` sauber via `CancellationToken` gestoppt.

### Änderungen Client (`SimHubConnection.cs`)

- `ConnectAsync()`: Parst die Version aus dem Connect-String (`"SimHub Property Server v1.6.0"`)
  via `IsHeartbeatSupported()` und setzt das Flag `_heartbeatEnabled`.
- `ReadFromServer()`: Aktualisiert `_lastReceivedTicks` bei jeder empfangenen Zeile.
  Ignoriert `ping`-Zeilen (kein `ParseProperty`-Aufruf, nur `Trace`-Log).
  Startet optional `WatchdogAsync()` (nur wenn `_heartbeatEnabled`), der im `finally`-Block
  sauber gestoppt wird.
- `WatchdogAsync()`: Prüft alle 10 s ob `_lastReceivedTicks` älter als 75 s ist – schließt
  dann `_tcpClient`, was `ReadLineAsync()` mit einer `IOException` abbricht → normaler
  Reconnect-Pfad.

### Timing

| Parameter | Wert | Begründung |
|---|---|---|
| Server Ping-Intervall | 30 s | Kurz genug um tote Verbindungen schnell zu erkennen |
| Server Ping-Prüfzyklus | 5 s | Interne Prüffrequenz des `PingLoopAsync`-Tasks |
| Client Watchdog-Prüfzyklus | 10 s | Interne Prüffrequenz des `WatchdogAsync`-Tasks |
| Client Timeout | 75 s | 2,5 × Ping-Intervall, Puffer für Latenzen |
| Mindest-Serverversion | v1.6.0 | Ab dieser Version sendet der Server `ping` |

## Betroffene Dateien

| Datei | Projekt | Änderung |
|---|---|---|
| `PropertyServer.Plugin/PropertyServer/Comm/Client.cs` | SimHubPropertyServer | Ping-Task senden |
| `StreamDeckSimHub.Plugin/SimHub/SimHubConnection.cs` | StreamDeckSimHub.Plugin | Ping empfangen, Timeout-Erkennung |

## Status

- [x] Analyse abgeschlossen
- [x] Implementierung Server (`Client.cs`)
- [x] Implementierung Client (`SimHubConnection.cs`)
- [ ] Test: Szenario A (sauberer Close weiterhin funktioniert)
- [ ] Test: Szenario B (SimHub-Kill ohne sauberes Close → Reconnect nach ~75s)

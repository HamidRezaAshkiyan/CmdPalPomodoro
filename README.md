# Pomodoro for Command Palette

A [PowerToys Command Palette](https://learn.microsoft.com/windows/powertoys/command-palette/overview) extension that pins a live Pomodoro countdown to the Dock.

- Live `mm:ss` countdown on the Dock
- Start, pause, resume, skip, and reset
- Configurable focus, short break, long break, and how many focus sessions happen before a long break
- When a session ends, the timer stops and a reminder asks you to start the break or the next focus. Nothing starts on its own.

## Requirements

- Windows 11
- PowerToys with Command Palette enabled
- .NET 10 SDK to build

## Install

Command Palette discovers extensions from installed MSIX packages.

1. Download `CmdPalPomodoro-x64.msix` (or ARM64) from the GitHub release.
2. Install `build\CmdPalPomodoro.cer` into **Current User → Trusted People** if Windows asks you to trust the developer certificate. The certificate password for the matching `.pfx` is `CmdPalPomodoro`. It is a public development certificate for sideloading, not a production trust root.
3. Install the MSIX.
4. Open Command Palette and run **Reload Command Palette extensions**.
5. Turn on the Dock, open Dock edit mode, and add the **Pomodoro** band.

The WinGet package id is `HamidRezaAshkiyan.CmdPalPomodoro`.

## Use

Open **Pomodoro** from Command Palette, or click the Dock band.

| Action | What it does |
| --- | --- |
| Start focus | Starts a focus session |
| Pause / Resume | Pauses the running session |
| Skip | Ends the current session and waits |
| Start short/long break | Starts the rest timer after a focus session |
| Skip break | Skips the offered rest and waits for the next focus |
| Reset | Stops and returns to ready. Keeps the finished-session count |
| Reset cycle | Stops and clears the finished-session count |
| Settings | Focus length, breaks, and the long-break interval |

Defaults are 25 / 5 / 15 minutes and a long break every 4 focus sessions. Settings are stored in `%LocalAppData%\CmdPalPomodoro\settings.json`. A running session keeps its remaining time until you reset it.

## Build

```powershell
dotnet build CmdPalPomodoro.sln -c Debug -p:Platform=x64 -p:EnableMsixTooling=false
dotnet build CmdPalPomodoro.sln -c Release -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true
```

MSIX packaging uses the `Microsoft.Windows.SDK.BuildTools` package, so a separate Windows SDK install is not required. Unpackaged F5 does not register the extension. In Visual Studio, use **Build → Deploy**.

## Publish

See [docs/store-submission.md](docs/store-submission.md) for the Microsoft Store checklist. WinGet installer scripts live in `installer/`.

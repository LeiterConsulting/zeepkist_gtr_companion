# Zeepkist GTR Companion

Zeepkist GTR Companion is an independent community project that connects
Zeepkist to a local Windows companion hub and, after explicit pairing, to
companion devices on the same local network.

This repository contains the public Zeepkist plugin, protocol, and
documentation. The official iOS companion app is a separate proprietary paid
product distributed through the Apple App Store, and its source code is not
part of this repository. Any future official Android app will follow the same
separate paid-product boundary.

## Project status

The project is in early development. There is not yet a public plugin release.

The first planned version will:

- observe supported ZeepSDK events without changing gameplay;
- send live events to a same-user Windows hub through local IPC;
- optionally identify the local Zeepkist player by Steam ID after consent;
- let the Windows hub advertise to explicitly paired companion devices;
- stream current level, run state, timing, split, and telemetry events;
- keep game authentication credentials on the PC; and
- complement the public GTR record history available through
  `https://graphql.zeepki.st`.

## How it fits together

```text
GTR GraphQL API ────────────────────────┐
                                       ├── Windows Companion Hub ──► Paired apps
Zeepkist ──► Companion plugin ──local───┘
```

The GTR GraphQL API supplies durable information such as players, levels,
personal bests, records, and world records. This plugin supplies live,
session-only information directly from the running game.

See [Architecture](docs/architecture.md) and
[Companion protocol](docs/protocol.md) for the current design. Companion client
developers can use the public [API v1 contracts](contracts/v1/README.md),
[pairing flow](docs/pairing.md), and [iOS integration guide](docs/ios-integration.md).

## Building

Prerequisites:

- Windows 10 or later for game-side runtime testing;
- Visual Studio 2022, Rider, or VS Code with the .NET 8 SDK;
- Zeepkist with BepInEx 5 and ZeepSDK 2.6.1 installed by Modkist Revamped; and
- PowerShell 5.1 or later.

Clone the repository and open `Zeepkist.GTR.Companion.sln`, or build from a
terminal:

```powershell
.\scripts\dev.ps1 -Configuration Debug
```

To build and copy the plugin directly into Zeepkist's BepInEx plugins
directory:

```powershell
.\scripts\dev.ps1 -Install -ZeepkistPath "C:\Program Files (x86)\Steam\steamapps\common\Zeepkist"
```

The script also accepts `ZEEPKIST_PATH` as an environment variable. Pass
`-Package -Configuration Release` to create a sideloadable zip beneath
`artifacts\`.

The Windows hub is built to:

```text
src\Zeepkist.GTR.Companion.Hub\bin\Debug\net8.0-windows\
```

More information is available in the
[development guide](docs/development.md).

## Installation

Installation packages will be published after the protocol and pairing flow
are ready for public use. Modkist distribution is planned, but no development
build should currently be treated as a release.

## Related projects

- [Zeepkist GTR](https://github.com/zeepkist/gtr)
- [ZeepSDK](https://github.com/zeepkist/ZeepSDK)
- [Modkist Revamped](https://github.com/zeepkist/ModkistRevamped)

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) and the
[public repository policy](PUBLIC_REPOSITORY.md) before opening a pull request.

## License

A source license will be selected before the first public release. Until then,
no license is granted beyond the rights provided by applicable law.

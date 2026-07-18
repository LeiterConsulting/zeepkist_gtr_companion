# Zeepkist GTR Companion

Zeepkist GTR Companion is an independent community project that connects
Zeepkist to a companion device on the same local network.

This repository contains the public Zeepkist plugin and its public
documentation. The companion app is distributed separately and its source code
is not part of this repository.

## Project status

The project is in early development. There is not yet a public plugin release.

The first planned version will:

- identify the local Zeepkist player by Steam ID;
- advertise the running game to paired companion devices on the local network;
- stream current level, run state, timing, split, and telemetry events;
- keep game authentication credentials on the PC; and
- complement the public GTR record history available through
  `https://graphql.zeepki.st`.

## How it fits together

```text
GTR GraphQL API ──────────────────────────┐
                                         ├── Companion app
Zeepkist → Companion plugin → Local LAN ──┘
```

The GTR GraphQL API supplies durable information such as players, levels,
personal bests, records, and world records. This plugin supplies live,
session-only information directly from the running game.

See [Architecture](docs/architecture.md) and
[Companion protocol](docs/protocol.md) for the current design.

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

To build and copy the plugin into Modkist's sideload directory:

```powershell
.\scripts\dev.ps1 -Install -ZeepkistPath "C:\Program Files (x86)\Steam\steamapps\common\Zeepkist"
```

The script also accepts `ZEEPKIST_PATH` as an environment variable. Pass
`-Package -Configuration Release` to create a sideloadable zip beneath
`artifacts\`.

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

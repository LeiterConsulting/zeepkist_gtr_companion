# Development Guide

## Requirements

- Windows 10 or later
- Git
- Visual Studio 2022, Rider, or VS Code with the .NET 8 SDK
- A Zeepkist installation for runtime verification
- Modkist Revamped with BepInEx 5 and ZeepSDK 2.6.1 installed

The plugin targets .NET Framework 4.7.2 to match the current Zeepkist mod
ecosystem. The repository supplies the reference assemblies and public NuGet
feeds required to compile it with the .NET 8 SDK.

## First checkout on the PC

```powershell
git clone https://github.com/LeiterConsulting/zeepkist_gtr_companion.git
cd zeepkist_gtr_companion
dotnet restore .\Zeepkist.GTR.Companion.sln
dotnet build .\Zeepkist.GTR.Companion.sln -c Debug --no-restore
```

Open `Zeepkist.GTR.Companion.sln` in your preferred IDE. The initial plugin
does not depend on private game DLLs checked into the repository; compile-time
game references come from the `Zeepkist.GameLibs` package.

## Build

From the repository root:

```powershell
.\scripts\dev.ps1 -Configuration Debug
```

The assembly is written to:

```text
src\Zeepkist.GTR.Companion\bin\Debug\net472\com.leiterconsulting.zeepkist.gtrcompanion.dll
```

## Install a development build

Set the game directory once:

```powershell
[Environment]::SetEnvironmentVariable(
    "ZEEPKIST_PATH",
    "C:\Program Files (x86)\Steam\steamapps\common\Zeepkist",
    "User"
)
```

Start a new terminal, then build and install:

```powershell
.\scripts\dev.ps1 -Install
```

You can also pass `-ZeepkistPath` directly. The script copies the plugin to:

```text
<Zeepkist>\BepInEx\plugins\Sideloaded\Plugins\ZeepkistGTRCompanion\
```

Launch Zeepkist and confirm `BepInEx\LogOutput.log` contains:

```text
Zeepkist GTR Companion 0.1.0-alpha loaded.
```

The initial scaffold only verifies that the plugin loads. LAN discovery,
pairing, and event transport will be added incrementally.

## Create a Modkist sideload package

```powershell
.\scripts\dev.ps1 -Configuration Release -Package
```

The resulting zip is placed beneath `artifacts\` and can be selected through
Modkist Revamped's sideload interface. Published Modkist/mod.io metadata will
be added when the plugin is ready for public preview.

Do not commit compiled DLLs, BepInEx configuration, captured sessions, logs,
game files, or the private companion app.

## Public repository check

Run this before staging or publishing changes:

```powershell
.\scripts\check-public-boundary.ps1
```

On Git Bash, WSL, Linux, or macOS:

```sh
./scripts/check-public-boundary.sh
```

Also inspect the complete staged diff. Ignore rules and automated checks
cannot detect every form of private or licensed material.

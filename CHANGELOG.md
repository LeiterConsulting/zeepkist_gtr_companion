# Changelog

All notable public changes to this project will be documented in this file.

The project intends to follow Semantic Versioning after the first public
release.

## Unreleased

- Created the public plugin repository.
- Added the initial BepInEx 5 and ZeepSDK 2.6.1 plugin scaffold.
- Added a Visual Studio solution and reproducible NuGet configuration.
- Added Windows build, Modkist sideload, and packaging commands.
- Documented the proposed local companion architecture and protocol.
- Added safeguards that keep the private companion app and internal working
  material outside the repository.
- Added versioned shared protocol contracts and reconnect snapshots.
- Added a bounded background named-pipe transport that keeps I/O off the game
  thread and drops events while no hub is connected.
- Added read-only ZeepSDK events for levels, runs, checkpoints, finishes,
  crashes, resets, camera changes, and wheel damage.
- Added an opt-in Windows tray hub restricted to the current Windows user.
- Added protocol and end-to-end local pipe tests.
- Changed the loader-facing plugin version to BepInEx-compatible numeric
  semantic versioning while retaining alpha status as assembly metadata.
- Added public OpenAPI and AsyncAPI v1 contracts for companion discovery,
  six-digit device enrollment, and the authenticated live feed.
- Documented Bonjour discovery, certificate pinning, device-token storage,
  revocation, and the iOS integration flow.

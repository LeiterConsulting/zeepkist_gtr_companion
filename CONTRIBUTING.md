# Contributing

Thanks for helping improve Zeepkist GTR Companion.

## Scope

Contributions to this repository should concern the public Zeepkist plugin,
the companion protocol, packaging, or public documentation. The companion
app's source code is maintained separately.

Before contributing, read [PUBLIC_REPOSITORY.md](PUBLIC_REPOSITORY.md).

## Development workflow

1. Create a focused branch.
2. Restore and build the plugin:

   ```powershell
   .\scripts\dev.ps1 -Configuration Release -Package
   ```

3. Run the repository boundary check:

   ```powershell
   .\scripts\check-public-boundary.ps1
   ```

4. Describe externally visible behavior and protocol changes in the pull
   request.

## Compatibility

- Keep the network protocol versioned.
- Treat unknown message fields as forward-compatible additions.
- Avoid exposing game, Steam, or GTR authentication credentials.
- Prefer ZeepSDK APIs over direct game patches when an appropriate API exists.
- Document any new local-network ports, service names, or permissions.

## Pull requests

Keep changes small enough to review. A pull request should explain:

- what user-facing problem it solves;
- how it was verified;
- whether it changes the companion protocol; and
- whether it introduces or redistributes third-party code or assets.

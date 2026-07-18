# Zeepkist GTR Companion API v1

Status: public preview contract. Compatibility is not guaranteed until the
first public release.

This directory is the canonical machine-readable handoff for companion client
developers:

- [`openapi.yaml`](openapi.yaml) describes discovery status and device
  enrollment over HTTPS.
- [`asyncapi.yaml`](asyncapi.yaml) describes the authenticated live WebSocket
  feed.
- [`../../docs/pairing.md`](../../docs/pairing.md) defines the complete
  enrollment flow and security requirements.
- [`../../docs/ios-integration.md`](../../docs/ios-integration.md) contains the
  Apple-platform implementation checklist.

## Compatibility rules

- `/v1` in HTTP and WebSocket paths is the API major version.
- `protocolVersion` is the live-event major version.
- Clients must ignore unknown JSON fields.
- Clients may ignore unknown event types.
- Existing fields will not change meaning within a major version.
- New optional fields and new event types may be added within a major version.

## Current implementation status

The in-game plugin and Windows hub already use the event envelope and event
names described by `asyncapi.yaml` over same-user local IPC.

Bonjour advertisement, HTTPS enrollment, certificate pinning, device
credentials, and the LAN WebSocket server are contract-first and not yet
enabled in the Windows hub.

## Validate locally

With Node.js 24 or later:

```powershell
npx --yes @redocly/cli@2.39.0 lint contracts/v1/openapi.yaml
npx --yes @asyncapi/cli@6.0.2 validate contracts/v1/asyncapi.yaml
```

CI runs both validators for every push and pull request.

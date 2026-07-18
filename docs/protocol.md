# Companion Protocol

Status: pre-release implementation. No compatibility guarantee applies before
the first public release.

The machine-readable HTTP enrollment and WebSocket feed definitions are in
[`contracts/v1`](../contracts/v1/).

## Transport layers

### Plugin to Windows hub

The implemented transport is a newline-delimited JSON stream over the
same-user Windows named pipe:

```text
LeiterConsulting.Zeepkist.GtrCompanion.v1
```

The pipe server is disabled until the player enables live companion data in
the Windows hub. The plugin never waits for the pipe on the Unity game thread.

### Windows hub to paired devices

LAN transport is not implemented yet. The proposed DNS-SD service type is:

```text
_zeepkist-companion._tcp
```

A service advertisement may include non-sensitive metadata such as protocol
and hub versions. It must not include authentication material, player identity,
or a Steam authentication ticket. The planned event transport is an
authenticated, encrypted WebSocket connection approved through the Windows
hub.

Every message uses a common envelope:

```json
{
  "protocolVersion": 1,
  "type": "run.checkpoint",
  "sessionId": "9b4833308ec846b1ae86dc1ad2c9019f",
  "sequence": 42,
  "timestamp": "2026-07-18T18:00:00.000Z",
  "payload": {
    "timeSeconds": 12.345,
    "checkpointNumber": 2
  }
}
```

Fields:

- `protocolVersion`: major protocol version.
- `type`: namespaced message type.
- `sessionId`: random identifier for one game/plugin lifetime.
- `sequence`: monotonically increasing session-local sequence number.
- `timestamp`: UTC production time.
- `payload`: message-specific content.

Receivers should ignore unknown payload fields. Unknown message types may be
ignored and logged.

## Implemented local messages

### Session

- `session.hello`
- `session.snapshot`
- `session.ended`

### Level

- `level.loaded`

### Run

- `run.spawned`
- `run.started`
- `run.checkpoint`
- `run.finished`
- `run.crashed`
- `run.reset`
- `run.ended`

### Vehicle and view

- `camera.changed`
- `vehicle.wheelBroken`

## Telemetry guidance

Continuous telemetry is intentionally not implemented in the first slice.
When added, it will be sampled at a bounded rate suitable for a companion
display and will use a lower-priority queue. Event messages such as checkpoints
and finishes will not depend on telemetry delivery.

A reconnect begins with `session.hello` followed by `session.snapshot`, so a
client does not need to reconstruct state from an earlier connection.

## Privacy and authentication

The implemented protocol does not carry a Steam ID or other player identity.
Future identity fields require a separate, explicit player opt-in. The
protocol must never carry:

- Steam session or Web API tickets;
- GTR access or refresh tokens;
- Steam credentials;
- device advertising identifiers; or
- unrelated personal or machine data.

The detailed LAN pairing exchange and complete message schemas will be
specified before the first public preview.

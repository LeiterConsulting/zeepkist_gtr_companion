# Companion Protocol

Status: design draft. No compatibility guarantee applies before version 1.

## Discovery

The proposed DNS-SD service type is:

```text
_zeepkist-companion._tcp
```

A service advertisement may include non-sensitive metadata such as protocol
version and plugin version. It must not include authentication material or a
Steam authentication ticket.

## Connection

The proposed transport is WebSocket. A client connects only after the player
approves a short-lived pairing request in Zeepkist.

Every message uses a common envelope:

```json
{
  "protocolVersion": 1,
  "type": "run.telemetry",
  "sequence": 42,
  "timestamp": "2026-07-18T18:00:00.000Z",
  "payload": {}
}
```

Fields:

- `protocolVersion`: major protocol version.
- `type`: namespaced message type.
- `sequence`: monotonically increasing connection-local sequence number.
- `timestamp`: UTC production time.
- `payload`: message-specific content.

Receivers should ignore unknown payload fields. Unknown message types may be
ignored and logged.

## Planned messages

### Session

- `session.hello`
- `session.snapshot`
- `session.ended`
- `session.error`

### Player and level

- `player.changed`
- `level.loaded`
- `level.unloaded`

### Run

- `run.started`
- `run.telemetry`
- `run.split`
- `run.finished`
- `run.reset`

## Telemetry guidance

Telemetry should be sampled at a bounded rate suitable for a companion
display. Event messages such as splits and finishes are delivered separately
and must not depend on a telemetry sample arriving.

A reconnect begins with `session.hello` followed by `session.snapshot`, so a
client does not need to reconstruct state from an earlier connection.

## Privacy and authentication

The protocol may carry the local player's Steam ID because it is needed to map
the live session to public GTR records. It must not carry:

- Steam session or Web API tickets;
- GTR access or refresh tokens;
- Steam credentials;
- device advertising identifiers; or
- unrelated personal or machine data.

The detailed pairing exchange and message schemas will be specified before
the first public preview.

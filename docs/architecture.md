# Architecture

Zeepkist GTR Companion separates the latency-sensitive game process from
integration work and separates durable public record data from live gameplay
data.

## Components

### GTR GraphQL API

The public GTR service at `https://graphql.zeepki.st` is the source for durable
data including users, Steam IDs, levels, records, personal bests, world
records, points, and available ghost media.

The companion plugin does not call GTR, proxy GTR credentials, or submit
records.

### Zeepkist companion plugin

The public plugin runs inside Zeepkist through BepInEx and ZeepSDK. It observes
documented game events, maintains a small reconnect snapshot, and publishes a
versioned stream to the Windows hub through a same-user named pipe.

The plugin performs no LAN discovery, HTTP requests, file writes, lighting
control, or device integration. Event serialization and pipe writes run on a
bounded background worker. If the hub is absent or cannot keep up, events are
dropped rather than delaying the game.

### Windows companion hub

The public Windows hub is the boundary between Zeepkist and every external
integration. Its responsibilities are:

- presenting setup, privacy, connection, and diagnostic state;
- accepting the local plugin connection only after explicit opt-in;
- querying public GTR data without moving credentials into the game process;
- performing future LAN discovery, pairing, authentication, and encryption;
- routing live events to approved integrations; and
- isolating integration failures from Zeepkist.

The current implementation accepts local IPC only. LAN discovery, mobile
pairing, GTR queries, Stream Deck, lighting, and web integrations are not yet
enabled.

### Mobile companion apps

The official paid iOS app is a separate proprietary App Store product. Any
future official paid Android app follows the same boundary. Mobile apps
combine public GTR history with the live stream exposed by an explicitly
enabled and paired Windows hub.

## Data flow

```text
                          Internet
                             │
                    GTR GraphQL API
                             │
                             ▼
Zeepkist ──► Companion plugin ──local IPC──► Windows hub ──paired LAN──► Apps
   │                 │
   └── ZeepSDK events┘
```

GTR data remains useful when the game is not running. Live events are
available only while the game, plugin, and hub are running.

## Discovery and transport

The implemented game-to-hub transport is a Windows named pipe restricted to
the current user. The hub does not start the pipe server until the player
enables local companion data.

The planned hub-to-device discovery mechanism is DNS Service
Discovery/Bonjour. After pairing, event delivery will use an authenticated,
encrypted WebSocket connection. Multicast or broadcast traffic is reserved
for standard service discovery rather than the telemetry stream.

## Security boundaries

- Local capture is disabled until the player opts in through the Windows hub.
- Named-pipe access is restricted to the same Windows user.
- LAN connections will require an explicit pairing step.
- Pairing secrets are short-lived and scoped to the local hub instance.
- Steam authentication tickets and GTR access or refresh tokens are never sent
  to companion devices.
- The plugin does not open a LAN or Internet-facing listener.
- The protocol currently carries no player identity.
- Clients must tolerate disconnects and must not treat live telemetry as an
  authoritative GTR record.

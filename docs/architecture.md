# Architecture

Zeepkist GTR Companion separates durable public record data from live local
gameplay data.

## Components

### GTR GraphQL API

The public GTR service at `https://graphql.zeepki.st` is the source for durable
data including users, Steam IDs, levels, records, personal bests, world
records, points, and available ghost media.

The companion plugin does not proxy GTR credentials or record submissions.

### Zeepkist companion plugin

The public plugin runs inside Zeepkist through BepInEx and ZeepSDK. Its
responsibilities are limited to:

- reading the local player's public Steam ID;
- observing the current game and run state;
- advertising a companion service on the local network;
- pairing with an explicitly approved device; and
- sending a versioned stream of session events.

### Companion app

The companion app is a separate product and is not developed in this
repository. It combines GTR history with the live LAN event stream.

## Data flow

```text
                         Internet
                            │
                   GTR GraphQL API
                            │
                            ▼
Zeepkist ──► Companion plugin ──LAN──► Companion app
   │                 │
   └── ZeepSDK events┘
```

GTR data remains useful when the game is not running. Live events are
available only while the game and companion plugin are running on a reachable
local network.

## Discovery and transport

The intended discovery mechanism is DNS Service Discovery/Bonjour. Discovery
announces only enough information to locate the plugin.

After pairing, event delivery uses a direct WebSocket connection. Multicast or
broadcast traffic is reserved for standard service discovery rather than the
telemetry stream.

## Security boundaries

- Connections require an explicit pairing step.
- Pairing secrets are short-lived and scoped to the local plugin instance.
- Steam authentication tickets and GTR access or refresh tokens are never sent
  to companion devices.
- The plugin does not open an Internet-facing listener.
- Clients must tolerate disconnects and must not treat live telemetry as an
  authoritative GTR record.

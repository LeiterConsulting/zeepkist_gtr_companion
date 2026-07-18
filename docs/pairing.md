# Companion Device Pairing

Status: public preview design. The Windows LAN server is not implemented yet.

## Product flow

The normal experience is:

1. The player opens the Windows hub and selects **Pair a device**.
2. The hub displays a QR code and a six-digit fallback code.
3. The player opens the iOS app and selects **Connect to Zeepkist**.
4. The app discovers the hub automatically. Scanning the QR code selects the
   correct hub and pins its certificate. Manual entry selects a discovered hub
   and asks for the six-digit code.
5. The app exchanges the code for a device-scoped credential.
6. The hub shows the approved device and the app opens the live feed.

No IP address, port, certificate installation, account, or router setup is
part of the user flow.

## Discovery

The Windows hub advertises:

```text
_zeepkist-companion._tcp.local.
```

The service port is the HTTPS and secure WebSocket port. TXT records are
limited to:

| Key | Meaning |
| --- | --- |
| `api` | HTTP API major version, currently `1` |
| `protocol` | Live event protocol major version, currently `1` |
| `hub` | Random hub installation UUID |
| `name` | User-visible hub name |
| `tls` | Lowercase SHA-256 certificate fingerprint |

Discovery must not contain a Steam ID, Windows username, machine name, pairing
code, device token, level, run state, or other player data.

## Pairing code

The hub generates the code with a cryptographically secure random number
generator over the full range `000000` through `999999`. It is treated as a
six-character string so leading zeroes are retained.

Each code:

- exists only after the Windows user selects **Pair a device**;
- expires after five minutes;
- may be used successfully once;
- is invalidated after five incorrect attempts;
- is never written to logs or analytics;
- is compared in constant time where practical; and
- is erased when pairing is cancelled or the hub exits.

Enrollment attempts are additionally rate-limited across the hub. A rate
limited response uses HTTP `429` and includes `Retry-After`.

## Enrollment exchange

The iOS app sends:

```http
POST /v1/pairings
Content-Type: application/json
```

```json
{
  "code": "042731",
  "deviceName": "Chris's iPhone",
  "platform": "ios",
  "appVersion": "1.0.0"
}
```

On success, the hub consumes the code and returns:

- a random device UUID;
- a device token containing at least 256 random bits;
- the secure WebSocket feed URL;
- the certificate fingerprint; and
- the granted `feed:read` scope.

The response uses `Cache-Control: no-store`. The raw device token is returned
once, is never logged, is stored with Windows DPAPI by the hub, and is stored
in iOS Keychain by the app. It remains valid until the Windows user revokes
that device.

The six-digit code is not a feed password and cannot be used again.

## TLS and QR bootstrap

The hub uses a per-installation TLS certificate and publishes its SHA-256
fingerprint. The iOS app pins that certificate for enrollment and feed
connections.

The preferred QR code contains the selected endpoint, certificate fingerprint,
and one-time pairing code:

```text
zeepkist-companion://pair?v=1&host=zeepkist-companion.local&port=24817&fp=<sha256>&code=042731
```

The QR contents are sensitive for the five-minute pairing window and must not
be stored in analytics, crash reports, screenshots, or logs.

Manual code entry uses the Bonjour-selected hub and its advertised
fingerprint. The app must show the selected hub name before sending the code.

## Feed authentication

The app opens the URL returned by enrollment with:

```http
Authorization: Bearer <deviceToken>
```

Only the `feed:read` scope exists in v1. The WebSocket is read-only for
gameplay data. Clients may send protocol-level ping/pong frames, but v1
contains no gameplay commands.

After every connection or reconnection, the hub sends `session.hello` followed
by `session.snapshot`. The app does not need earlier events to reconstruct the
current state.

## Revocation and reset

The Windows hub provides:

- **Forget** beside each enrolled device;
- **Forget every device**; and
- **Reset companion security**, which replaces the certificate and invalidates
  every device token.

Revocation takes effect for new requests immediately and closes any active
feed connection for that device.

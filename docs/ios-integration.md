# iOS Integration Guide

This guide accompanies the machine-readable contracts in
[`contracts/v1`](../contracts/v1/).

## App capabilities

The app needs local network access and Bonjour browsing. Add user-facing
`NSLocalNetworkUsageDescription` text and declare this service in
`NSBonjourServices`:

```xml
<key>NSBonjourServices</key>
<array>
    <string>_zeepkist-companion._tcp</string>
</array>
```

Browse only for the declared service rather than browsing all Bonjour service
types. The app should request local-network access from a visible,
user-initiated **Connect to Zeepkist** screen.

## Recommended Apple APIs

- Use `NWBrowser` with a Bonjour descriptor for discovery.
- Use `URLSession` for `GET /v1/status` and `POST /v1/pairings`.
- Use `URLSessionWebSocketTask` for the `wss://` live feed.
- Use a `URLSessionDelegate` authentication challenge to validate the pinned
  hub certificate fingerprint.
- Store the device token in Keychain with a device-only accessibility class.

## Enrollment

1. Browse for `_zeepkist-companion._tcp`.
2. Resolve the service selected by QR code or by the user.
3. Read and validate `api`, `protocol`, `hub`, `name`, and `tls` TXT fields.
4. Pin the TLS certificate to `tls`.
5. Call `GET /v1/status`.
6. Preserve the pairing code as a six-character string.
7. Call `POST /v1/pairings`.
8. Confirm the returned fingerprint matches the pinned fingerprint.
9. Store `deviceId`, `deviceToken`, `feedUrl`, and the fingerprint in Keychain.
10. Clear the pairing code and enrollment response from memory as soon as
    practical.

Do not print request or response bodies for the pairing endpoint in production
logs.

## Live feed

Create a `URLRequest` for `feedUrl` and set:

```http
Authorization: Bearer <deviceToken>
```

Expect UTF-8 JSON text messages. Decode the common envelope first, then route
on `type`. Unknown fields and unknown message types are forward-compatible and
must not disconnect the app.

Treat `sequence` as session-local:

- a gap means an event may have been lost;
- a lower sequence with a new `sessionId` means a new game session;
- reconnecting begins with `session.hello` and `session.snapshot`; and
- live events are display data, not authoritative GTR records.

## Connection behavior

- Enable `waitsForConnectivity` for local-network permission and Wi-Fi
  transitions.
- Retry discovery when the initial local-network request is denied while the
  permission prompt is unresolved.
- Use bounded exponential backoff for feed reconnects.
- Stop aggressive retries while the app is backgrounded.
- Present plain-language states: **Looking for Zeepkist**, **Enter the code
  shown on your PC**, **Connected**, and **Connection lost**.

## Privacy

The v1 preview feed contains no Steam ID or other player identity. If identity
is added later, it will require a separate capability and explicit Windows
opt-in. Never request or store Steam, GTR, or Windows credentials.

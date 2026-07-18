using System;
using System.Text.Json;
using LeiterConsulting.Zeepkist.GtrCompanion.Protocol;
using Xunit;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Protocol.Tests;

public sealed class ProtocolJsonTests
{
    [Fact]
    public void Serialize_ProducesVersionedEnvelopeWithoutPlayerIdentity()
    {
        var json = ProtocolJson.Serialize(
            CompanionEventTypes.SessionHello,
            "session-123",
            7,
            new DateTime(2026, 7, 18, 18, 0, 0, DateTimeKind.Utc),
            new SessionHelloPayload
            {
                PluginVersion = "0.1.0",
                ContainsPlayerIdentity = false
            });

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(ProtocolConstants.Version, root.GetProperty("protocolVersion").GetInt32());
        Assert.Equal(CompanionEventTypes.SessionHello, root.GetProperty("type").GetString());
        Assert.Equal("session-123", root.GetProperty("sessionId").GetString());
        Assert.Equal(7, root.GetProperty("sequence").GetInt64());
        Assert.Equal("2026-07-18T18:00:00.0000000Z", root.GetProperty("timestamp").GetString());
        Assert.False(root.GetProperty("payload").GetProperty("containsPlayerIdentity").GetBoolean());
        Assert.DoesNotContain("steam", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_SnapshotOmitsUnknownLevelHashes()
    {
        var json = ProtocolJson.Serialize(
            CompanionEventTypes.SessionSnapshot,
            "session-123",
            1,
            DateTime.UtcNow,
            new SessionSnapshotPayload());

        using var document = JsonDocument.Parse(json);
        var payload = document.RootElement.GetProperty("payload");

        Assert.False(payload.TryGetProperty("levelHash", out _));
        Assert.False(payload.TryGetProperty("levelHashV2", out _));
        Assert.Equal(RunStates.Unknown, payload.GetProperty("runState").GetString());
    }
}

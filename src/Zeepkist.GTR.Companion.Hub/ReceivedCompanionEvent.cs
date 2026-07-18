using System;
using System.Text.Json;
using LeiterConsulting.Zeepkist.GtrCompanion.Protocol;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Hub;

internal sealed record ReceivedCompanionEvent(
    string Type,
    string SessionId,
    long Sequence,
    DateTimeOffset Timestamp)
{
    public static bool TryParse(string json, out ReceivedCompanionEvent? companionEvent)
    {
        companionEvent = null;

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (!root.TryGetProperty("protocolVersion", out var protocolVersionElement)
                || protocolVersionElement.GetInt32() != ProtocolConstants.Version
                || !root.TryGetProperty("type", out var typeElement)
                || !root.TryGetProperty("sessionId", out var sessionIdElement)
                || !root.TryGetProperty("sequence", out var sequenceElement)
                || !root.TryGetProperty("timestamp", out var timestampElement))
            {
                return false;
            }

            var type = typeElement.GetString();
            var sessionId = sessionIdElement.GetString();
            var timestampText = timestampElement.GetString();
            if (string.IsNullOrWhiteSpace(type)
                || string.IsNullOrWhiteSpace(sessionId)
                || !DateTimeOffset.TryParse(timestampText, out var timestamp))
            {
                return false;
            }

            companionEvent = new ReceivedCompanionEvent(
                type,
                sessionId,
                sequenceElement.GetInt64(),
                timestamp);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

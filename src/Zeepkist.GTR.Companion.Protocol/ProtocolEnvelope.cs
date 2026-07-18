using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Protocol;

[DataContract]
public sealed class ProtocolEnvelope<TPayload>
{
    [DataMember(Name = "protocolVersion", Order = 1)]
    public int ProtocolVersion { get; set; }

    [DataMember(Name = "type", Order = 2)]
    public string Type { get; set; } = string.Empty;

    [DataMember(Name = "sessionId", Order = 3)]
    public string SessionId { get; set; } = string.Empty;

    [DataMember(Name = "sequence", Order = 4)]
    public long Sequence { get; set; }

    [DataMember(Name = "timestamp", Order = 5)]
    public string Timestamp { get; set; } = string.Empty;

    [DataMember(Name = "payload", Order = 6)]
    public TPayload? Payload { get; set; }
}

public static class ProtocolJson
{
    public static string Serialize<TPayload>(
        string type,
        string sessionId,
        long sequence,
        DateTime timestampUtc,
        TPayload payload)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("A message type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("A session ID is required.", nameof(sessionId));
        }

        var envelope = new ProtocolEnvelope<TPayload>
        {
            ProtocolVersion = ProtocolConstants.Version,
            Type = type,
            SessionId = sessionId,
            Sequence = sequence,
            Timestamp = timestampUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            Payload = payload
        };

        var serializer = new DataContractJsonSerializer(typeof(ProtocolEnvelope<TPayload>));
        using var stream = new MemoryStream();
        serializer.WriteObject(stream, envelope);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}

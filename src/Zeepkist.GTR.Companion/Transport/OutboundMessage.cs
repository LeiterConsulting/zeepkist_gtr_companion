using System;
using LeiterConsulting.Zeepkist.GtrCompanion.Protocol;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Transport;

internal interface IOutboundMessage
{
    string Serialize(string sessionId, long sequence);
}

internal sealed class OutboundMessage<TPayload> : IOutboundMessage
{
    private readonly string _type;
    private readonly TPayload _payload;
    private readonly DateTime _timestampUtc;

    public OutboundMessage(string type, TPayload payload)
    {
        _type = type;
        _payload = payload;
        _timestampUtc = DateTime.UtcNow;
    }

    public string Serialize(string sessionId, long sequence)
    {
        return ProtocolJson.Serialize(_type, sessionId, sequence, _timestampUtc, _payload);
    }
}

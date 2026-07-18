using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using LeiterConsulting.Zeepkist.GtrCompanion.Protocol;
using Xunit;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Hub.Tests;

public sealed class CompanionPipeServerTests
{
    [Fact]
    public async Task Server_ReceivesVersionedEventFromSameUserPipe()
    {
        var pipeName = $"{ProtocolConstants.PipeName}.Tests.{Guid.NewGuid():N}";
        using var server = new CompanionPipeServer(pipeName);
        var received = new TaskCompletionSource<ReceivedCompanionEvent>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        server.EventReceived += companionEvent => received.TrySetResult(companionEvent);
        server.Start();

        using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.Out,
            PipeOptions.Asynchronous);
        await client.ConnectAsync(2000);

        using var writer = new StreamWriter(client, new UTF8Encoding(false))
        {
            AutoFlush = true,
            NewLine = "\n"
        };

        var json = ProtocolJson.Serialize(
            CompanionEventTypes.RunStarted,
            "session-123",
            12,
            DateTime.UtcNow,
            EmptyPayload.Instance);
        await writer.WriteLineAsync(json);

        var companionEvent = await received.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(CompanionEventTypes.RunStarted, companionEvent.Type);
        Assert.Equal("session-123", companionEvent.SessionId);
        Assert.Equal(12, companionEvent.Sequence);
    }

    [Fact]
    public void Parser_RejectsUnsupportedProtocolVersion()
    {
        const string json =
            """{"protocolVersion":99,"type":"session.hello","sessionId":"x","sequence":1,"timestamp":"2026-07-18T18:00:00Z","payload":{}}""";

        Assert.False(ReceivedCompanionEvent.TryParse(json, out _));
    }
}

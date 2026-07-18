using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using BepInEx.Logging;
using LeiterConsulting.Zeepkist.GtrCompanion.Protocol;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Transport;

internal sealed class LocalPipeClient : IDisposable
{
    private const int MaximumQueuedEvents = 256;
    private const int ConnectTimeoutMilliseconds = 250;
    private const int ReconnectDelayMilliseconds = 1000;

    private readonly ManualLogSource _logger;
    private readonly string _pluginVersion;
    private readonly Func<SessionSnapshotPayload> _snapshotFactory;
    private readonly string _sessionId = Guid.NewGuid().ToString("N");
    private readonly ConcurrentQueue<IOutboundMessage> _queue = new();
    private readonly AutoResetEvent _queueSignal = new(false);
    private readonly CancellationTokenSource _cancellation = new();
    private Thread? _worker;
    private long _sequence;
    private int _queuedCount;
    private int _connected;
    private int _disposed;

    public LocalPipeClient(
        ManualLogSource logger,
        string pluginVersion,
        Func<SessionSnapshotPayload> snapshotFactory)
    {
        _logger = logger;
        _pluginVersion = pluginVersion;
        _snapshotFactory = snapshotFactory;
    }

    public void Start()
    {
        if (_worker != null)
        {
            return;
        }

        _worker = new Thread(Run)
        {
            IsBackground = true,
            Name = "Zeepkist GTR Companion IPC"
        };
        _worker.Start();
    }

    public bool TryEnqueue<TPayload>(string type, TPayload payload)
    {
        if (Volatile.Read(ref _disposed) != 0 || Volatile.Read(ref _connected) == 0)
        {
            return false;
        }

        var newCount = Interlocked.Increment(ref _queuedCount);
        if (newCount > MaximumQueuedEvents)
        {
            Interlocked.Decrement(ref _queuedCount);
            return false;
        }

        _queue.Enqueue(new OutboundMessage<TPayload>(type, payload));
        _queueSignal.Set();
        return true;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _cancellation.Cancel();
        _queueSignal.Set();

        if (_worker?.IsAlive == true)
        {
            _worker.Join(500);
        }

        _queueSignal.Dispose();
        _cancellation.Dispose();
    }

    private void Run()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeClientStream(
                    ".",
                    ProtocolConstants.PipeName,
                    PipeDirection.Out,
                    PipeOptions.Asynchronous);

                pipe.Connect(ConnectTimeoutMilliseconds);

                using var writer = new StreamWriter(pipe, new UTF8Encoding(false))
                {
                    AutoFlush = true,
                    NewLine = "\n"
                };

                Interlocked.Exchange(ref _connected, 1);
                _logger.LogInfo("Connected to the local Zeepkist Companion Hub.");

                WriteImmediate(
                    writer,
                    CompanionEventTypes.SessionHello,
                    new SessionHelloPayload
                    {
                        PluginVersion = _pluginVersion,
                        ContainsPlayerIdentity = false
                    });
                WriteImmediate(writer, CompanionEventTypes.SessionSnapshot, _snapshotFactory());
                DrainConnection(writer);
            }
            catch (TimeoutException)
            {
                WaitBeforeReconnect();
            }
            catch (IOException)
            {
                WaitBeforeReconnect();
            }
            catch (Exception exception)
            {
                _logger.LogWarning($"Local companion transport stopped after an unexpected error: {exception.Message}");
                WaitBeforeReconnect();
            }
            finally
            {
                if (Interlocked.Exchange(ref _connected, 0) != 0)
                {
                    _logger.LogInfo("Disconnected from the local Zeepkist Companion Hub.");
                }

                ClearQueue();
            }
        }
    }

    private void DrainConnection(StreamWriter writer)
    {
        while (!_cancellation.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var message))
            {
                Interlocked.Decrement(ref _queuedCount);
                writer.WriteLine(message.Serialize(_sessionId, NextSequence()));
                continue;
            }

            _queueSignal.WaitOne(250);
        }
    }

    private void WriteImmediate<TPayload>(StreamWriter writer, string type, TPayload payload)
    {
        writer.WriteLine(
            ProtocolJson.Serialize(type, _sessionId, NextSequence(), DateTime.UtcNow, payload));
    }

    private long NextSequence()
    {
        return Interlocked.Increment(ref _sequence);
    }

    private void ClearQueue()
    {
        while (_queue.TryDequeue(out _))
        {
            Interlocked.Decrement(ref _queuedCount);
        }
    }

    private void WaitBeforeReconnect()
    {
        if (!_cancellation.IsCancellationRequested)
        {
            _cancellation.Token.WaitHandle.WaitOne(ReconnectDelayMilliseconds);
        }
    }
}

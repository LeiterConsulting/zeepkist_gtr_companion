using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeiterConsulting.Zeepkist.GtrCompanion.Protocol;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Hub;

internal enum CompanionConnectionState
{
    Disabled,
    WaitingForGame,
    Connected,
    Faulted
}

internal sealed class CompanionPipeServer : IDisposable
{
    private readonly object _gate = new();
    private readonly string _pipeName;
    private CancellationTokenSource? _cancellation;
    private NamedPipeServerStream? _activePipe;
    private Task? _worker;
    private bool _disposed;

    public event Action<CompanionConnectionState>? ConnectionStateChanged;
    public event Action<ReceivedCompanionEvent>? EventReceived;

    public CompanionPipeServer(string pipeName = ProtocolConstants.PipeName)
    {
        _pipeName = pipeName;
    }

    public void Start()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_worker != null)
            {
                return;
            }

            _cancellation = new CancellationTokenSource();
            _worker = Task.Run(() => ListenAsync(_cancellation.Token));
        }
    }

    public void Stop()
    {
        CancellationTokenSource? cancellation;
        NamedPipeServerStream? activePipe;
        Task? worker;

        lock (_gate)
        {
            cancellation = _cancellation;
            activePipe = _activePipe;
            worker = _worker;
            _cancellation = null;
            _activePipe = null;
            _worker = null;
        }

        cancellation?.Cancel();
        activePipe?.Dispose();

        if (worker != null)
        {
            try
            {
                worker.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException)
            {
            }
        }

        cancellation?.Dispose();
        ConnectionStateChanged?.Invoke(CompanionConnectionState.Disabled);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        Stop();
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        ConnectionStateChanged?.Invoke(CompanionConnectionState.WaitingForGame);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

                lock (_gate)
                {
                    _activePipe = pipe;
                }

                await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                ConnectionStateChanged?.Invoke(CompanionConnectionState.Connected);

                using var reader = new StreamReader(
                    pipe,
                    new UTF8Encoding(false, true),
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 4096,
                    leaveOpen: true);

                while (pipe.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                    if (line == null)
                    {
                        break;
                    }

                    if (Encoding.UTF8.GetByteCount(line) > ProtocolConstants.MaximumMessageBytes)
                    {
                        continue;
                    }

                    if (ReceivedCompanionEvent.TryParse(line, out var companionEvent)
                        && companionEvent != null)
                    {
                        EventReceived?.Invoke(companionEvent);
                    }
                }

                ConnectionStateChanged?.Invoke(CompanionConnectionState.WaitingForGame);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (IOException) when (!cancellationToken.IsCancellationRequested)
            {
                ConnectionStateChanged?.Invoke(CompanionConnectionState.WaitingForGame);
            }
            catch
            {
                ConnectionStateChanged?.Invoke(CompanionConnectionState.Faulted);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            finally
            {
                lock (_gate)
                {
                    _activePipe = null;
                }
            }
        }
    }
}

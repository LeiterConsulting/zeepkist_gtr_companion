using LeiterConsulting.Zeepkist.GtrCompanion.Protocol;

namespace LeiterConsulting.Zeepkist.GtrCompanion;

internal sealed class CompanionState
{
    private readonly object _gate = new();
    private string? _levelHash;
    private string? _levelHashV2;
    private string _runState = RunStates.Unknown;
    private int _checkpointCount;
    private float? _lastTimeSeconds;
    private string _cameraMode = CameraModes.Unknown;
    private int _brokenWheelCount;

    public void LevelLoaded(string? levelHash, string? levelHashV2)
    {
        lock (_gate)
        {
            _levelHash = levelHash;
            _levelHashV2 = levelHashV2;
            _runState = RunStates.Ready;
            _checkpointCount = 0;
            _lastTimeSeconds = null;
            _brokenWheelCount = 0;
        }
    }

    public void PlayerSpawned()
    {
        lock (_gate)
        {
            _runState = RunStates.Ready;
        }
    }

    public void RunStarted()
    {
        lock (_gate)
        {
            _runState = RunStates.Running;
            _checkpointCount = 0;
            _lastTimeSeconds = null;
            _brokenWheelCount = 0;
        }
    }

    public int CheckpointPassed(float timeSeconds)
    {
        lock (_gate)
        {
            _lastTimeSeconds = timeSeconds;
            return ++_checkpointCount;
        }
    }

    public void RunFinished(float timeSeconds)
    {
        lock (_gate)
        {
            _runState = RunStates.Finished;
            _lastTimeSeconds = timeSeconds;
        }
    }

    public void RunCrashed()
    {
        lock (_gate)
        {
            _runState = RunStates.Crashed;
        }
    }

    public void RunReset()
    {
        lock (_gate)
        {
            _runState = RunStates.Ready;
            _checkpointCount = 0;
            _lastTimeSeconds = null;
            _brokenWheelCount = 0;
        }
    }

    public void RunEnded()
    {
        lock (_gate)
        {
            _runState = RunStates.Ended;
        }
    }

    public void CameraChanged(string cameraMode)
    {
        lock (_gate)
        {
            _cameraMode = cameraMode;
        }
    }

    public int WheelBroken()
    {
        lock (_gate)
        {
            return ++_brokenWheelCount;
        }
    }

    public SessionSnapshotPayload CreateSnapshot()
    {
        lock (_gate)
        {
            return new SessionSnapshotPayload
            {
                LevelHash = _levelHash,
                LevelHashV2 = _levelHashV2,
                RunState = _runState,
                CheckpointCount = _checkpointCount,
                LastTimeSeconds = _lastTimeSeconds,
                CameraMode = _cameraMode,
                BrokenWheelCount = _brokenWheelCount
            };
        }
    }
}

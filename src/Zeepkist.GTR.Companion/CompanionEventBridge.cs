using System;
using BepInEx.Logging;
using LeiterConsulting.Zeepkist.GtrCompanion.Protocol;
using LeiterConsulting.Zeepkist.GtrCompanion.Transport;
using ZeepSDK.Level;
using ZeepSDK.Racing;

namespace LeiterConsulting.Zeepkist.GtrCompanion;

internal sealed class CompanionEventBridge : IDisposable
{
    private readonly CompanionState _state = new();
    private readonly LocalPipeClient _pipeClient;
    private bool _started;

    public CompanionEventBridge(ManualLogSource logger, string pluginVersion)
    {
        _pipeClient = new LocalPipeClient(logger, pluginVersion, _state.CreateSnapshot);
    }

    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        Subscribe();
        _pipeClient.Start();
    }

    public void Dispose()
    {
        if (!_started)
        {
            return;
        }

        _started = false;
        Unsubscribe();
        _pipeClient.Dispose();
    }

    private void Subscribe()
    {
        RacingApi.LevelLoaded += OnLevelLoaded;
        RacingApi.PlayerSpawned += OnPlayerSpawned;
        RacingApi.RoundStarted += OnRoundStarted;
        RacingApi.RoundEnded += OnRoundEnded;
        RacingApi.PassedCheckpoint += OnPassedCheckpoint;
        RacingApi.CrossedFinishLine += OnCrossedFinishLine;
        RacingApi.Crashed += OnCrashed;
        RacingApi.QuickReset += OnQuickReset;
        RacingApi.EnteredFirstPerson += OnEnteredFirstPerson;
        RacingApi.EnteredThirdPerson += OnEnteredThirdPerson;
        RacingApi.WheelBroken += OnWheelBroken;
        RacingApi.Quit += OnQuit;
    }

    private void Unsubscribe()
    {
        RacingApi.LevelLoaded -= OnLevelLoaded;
        RacingApi.PlayerSpawned -= OnPlayerSpawned;
        RacingApi.RoundStarted -= OnRoundStarted;
        RacingApi.RoundEnded -= OnRoundEnded;
        RacingApi.PassedCheckpoint -= OnPassedCheckpoint;
        RacingApi.CrossedFinishLine -= OnCrossedFinishLine;
        RacingApi.Crashed -= OnCrashed;
        RacingApi.QuickReset -= OnQuickReset;
        RacingApi.EnteredFirstPerson -= OnEnteredFirstPerson;
        RacingApi.EnteredThirdPerson -= OnEnteredThirdPerson;
        RacingApi.WheelBroken -= OnWheelBroken;
        RacingApi.Quit -= OnQuit;
    }

    private void OnLevelLoaded()
    {
        var levelHash = LevelApi.CurrentHash;
        var levelHashV2 = LevelApi.CurrentHashV2?.Hash;
        _state.LevelLoaded(levelHash, levelHashV2);
        _pipeClient.TryEnqueue(
            CompanionEventTypes.LevelLoaded,
            new LevelLoadedPayload
            {
                LevelHash = levelHash,
                LevelHashV2 = levelHashV2
            });
    }

    private void OnPlayerSpawned()
    {
        _state.PlayerSpawned();
        _pipeClient.TryEnqueue(CompanionEventTypes.PlayerSpawned, EmptyPayload.Instance);
    }

    private void OnRoundStarted()
    {
        _state.RunStarted();
        _pipeClient.TryEnqueue(CompanionEventTypes.RunStarted, EmptyPayload.Instance);
    }

    private void OnRoundEnded()
    {
        _state.RunEnded();
        _pipeClient.TryEnqueue(CompanionEventTypes.RunEnded, EmptyPayload.Instance);
    }

    private void OnPassedCheckpoint(float timeSeconds)
    {
        var checkpointNumber = _state.CheckpointPassed(timeSeconds);
        _pipeClient.TryEnqueue(
            CompanionEventTypes.RunCheckpoint,
            new TimedRunPayload
            {
                TimeSeconds = timeSeconds,
                CheckpointNumber = checkpointNumber
            });
    }

    private void OnCrossedFinishLine(float timeSeconds)
    {
        _state.RunFinished(timeSeconds);
        _pipeClient.TryEnqueue(
            CompanionEventTypes.RunFinished,
            new TimedRunPayload { TimeSeconds = timeSeconds });
    }

    private void OnCrashed(CrashReason reason)
    {
        _state.RunCrashed();
        _pipeClient.TryEnqueue(
            CompanionEventTypes.RunCrashed,
            new CrashPayload { Reason = reason.ToString() });
    }

    private void OnQuickReset()
    {
        _state.RunReset();
        _pipeClient.TryEnqueue(CompanionEventTypes.RunReset, EmptyPayload.Instance);
    }

    private void OnEnteredFirstPerson()
    {
        PublishCameraChange(CameraModes.FirstPerson);
    }

    private void OnEnteredThirdPerson()
    {
        PublishCameraChange(CameraModes.ThirdPerson);
    }

    private void PublishCameraChange(string cameraMode)
    {
        _state.CameraChanged(cameraMode);
        _pipeClient.TryEnqueue(
            CompanionEventTypes.CameraChanged,
            new CameraChangedPayload { Mode = cameraMode });
    }

    private void OnWheelBroken()
    {
        var brokenWheelCount = _state.WheelBroken();
        _pipeClient.TryEnqueue(
            CompanionEventTypes.WheelBroken,
            new WheelBrokenPayload { BrokenWheelCount = brokenWheelCount });
    }

    private void OnQuit()
    {
        _pipeClient.TryEnqueue(CompanionEventTypes.SessionEnded, EmptyPayload.Instance);
    }
}

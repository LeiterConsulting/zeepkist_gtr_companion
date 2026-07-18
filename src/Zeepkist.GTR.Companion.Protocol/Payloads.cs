using System.Runtime.Serialization;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Protocol;

[DataContract]
public sealed class EmptyPayload
{
    public static readonly EmptyPayload Instance = new();
}

[DataContract]
public sealed class SessionHelloPayload
{
    [DataMember(Name = "pluginVersion", Order = 1)]
    public string PluginVersion { get; set; } = string.Empty;

    [DataMember(Name = "game", Order = 2)]
    public string Game { get; set; } = "Zeepkist";

    [DataMember(Name = "containsPlayerIdentity", Order = 3)]
    public bool ContainsPlayerIdentity { get; set; }
}

[DataContract]
public sealed class SessionSnapshotPayload
{
    [DataMember(Name = "levelHash", Order = 1, EmitDefaultValue = false)]
    public string? LevelHash { get; set; }

    [DataMember(Name = "levelHashV2", Order = 2, EmitDefaultValue = false)]
    public string? LevelHashV2 { get; set; }

    [DataMember(Name = "runState", Order = 3)]
    public string RunState { get; set; } = RunStates.Unknown;

    [DataMember(Name = "checkpointCount", Order = 4)]
    public int CheckpointCount { get; set; }

    [DataMember(Name = "lastTimeSeconds", Order = 5, EmitDefaultValue = false)]
    public float? LastTimeSeconds { get; set; }

    [DataMember(Name = "cameraMode", Order = 6)]
    public string CameraMode { get; set; } = CameraModes.Unknown;

    [DataMember(Name = "brokenWheelCount", Order = 7)]
    public int BrokenWheelCount { get; set; }
}

[DataContract]
public sealed class LevelLoadedPayload
{
    [DataMember(Name = "levelHash", Order = 1, EmitDefaultValue = false)]
    public string? LevelHash { get; set; }

    [DataMember(Name = "levelHashV2", Order = 2, EmitDefaultValue = false)]
    public string? LevelHashV2 { get; set; }
}

[DataContract]
public sealed class TimedRunPayload
{
    [DataMember(Name = "timeSeconds", Order = 1)]
    public float TimeSeconds { get; set; }

    [DataMember(Name = "checkpointNumber", Order = 2, EmitDefaultValue = false)]
    public int? CheckpointNumber { get; set; }
}

[DataContract]
public sealed class CrashPayload
{
    [DataMember(Name = "reason", Order = 1)]
    public string Reason { get; set; } = string.Empty;
}

[DataContract]
public sealed class CameraChangedPayload
{
    [DataMember(Name = "mode", Order = 1)]
    public string Mode { get; set; } = CameraModes.Unknown;
}

[DataContract]
public sealed class WheelBrokenPayload
{
    [DataMember(Name = "brokenWheelCount", Order = 1)]
    public int BrokenWheelCount { get; set; }
}

public static class RunStates
{
    public const string Unknown = "unknown";
    public const string Ready = "ready";
    public const string Running = "running";
    public const string Finished = "finished";
    public const string Crashed = "crashed";
    public const string Ended = "ended";
}

public static class CameraModes
{
    public const string Unknown = "unknown";
    public const string FirstPerson = "firstPerson";
    public const string ThirdPerson = "thirdPerson";
}

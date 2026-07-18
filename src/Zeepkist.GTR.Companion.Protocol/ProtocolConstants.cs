namespace LeiterConsulting.Zeepkist.GtrCompanion.Protocol;

public static class ProtocolConstants
{
    public const int Version = 1;
    public const string PipeName = "LeiterConsulting.Zeepkist.GtrCompanion.v1";
    public const int MaximumMessageBytes = 64 * 1024;
}

public static class CompanionEventTypes
{
    public const string SessionHello = "session.hello";
    public const string SessionSnapshot = "session.snapshot";
    public const string SessionEnded = "session.ended";
    public const string LevelLoaded = "level.loaded";
    public const string PlayerSpawned = "run.spawned";
    public const string RunStarted = "run.started";
    public const string RunCheckpoint = "run.checkpoint";
    public const string RunFinished = "run.finished";
    public const string RunCrashed = "run.crashed";
    public const string RunReset = "run.reset";
    public const string RunEnded = "run.ended";
    public const string CameraChanged = "camera.changed";
    public const string WheelBroken = "vehicle.wheelBroken";
}

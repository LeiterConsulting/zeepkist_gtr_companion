using BepInEx;
using LeiterConsulting.Zeepkist.GtrCompanion.Transport;

namespace LeiterConsulting.Zeepkist.GtrCompanion;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK", "2.6.1")]
public sealed class Plugin : BaseUnityPlugin
{
    private CompanionEventBridge? _eventBridge;

    private void Awake()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        Logger.LogInfo("Live data is limited to same-user local IPC; LAN sharing and player identity are disabled.");

        _eventBridge = new CompanionEventBridge(Logger, MyPluginInfo.PLUGIN_VERSION);
        _eventBridge.Start();
    }

    private void OnDestroy()
    {
        _eventBridge?.Dispose();
        _eventBridge = null;
    }
}

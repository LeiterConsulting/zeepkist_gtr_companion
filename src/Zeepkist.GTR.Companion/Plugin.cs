using BepInEx;

namespace LeiterConsulting.Zeepkist.GtrCompanion;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK", "2.6.1")]
public sealed class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        Logger.LogInfo("LAN companion features are not enabled in this development build.");
    }
}

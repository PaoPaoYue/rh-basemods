using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using Game;

namespace UnlockMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Harmony.CreateAndPatchAll(typeof(UnlockPatch));
    }
}

class UnlockPatch
{
    [HarmonyPatch(typeof(GameManager), "Initialize")]
    [HarmonyPrefix]
    static void RollRealDice(ref GameManager __instance)
    {
        __instance.GMEnable= true;
        Plugin.Logger.LogInfo("GM panel enabled!");
    }
}

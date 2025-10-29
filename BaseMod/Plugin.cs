using BepInEx;
using BepInEx.Logging;

namespace BaseMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("Initializing...");
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(BattleManagerPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(BattleOrderManagerPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(BuffModelPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(ElementEntityPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(ElementTipCellPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(LotteCellPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(ModModelPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(RelicEntityPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(RelicTipCellPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(PlayerEntityPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(PlayerInfoPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(HardRoleInfoPatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(UIHardChoosePatch));
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(UILeaderboardPatch));
        Logger.LogInfo("Initialization completed!");
    }
}

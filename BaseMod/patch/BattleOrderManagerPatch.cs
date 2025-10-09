using HarmonyLib;
using Game;

namespace BaseMod;

static class BattleOrderManagerPatch
{

    [HarmonyPatch(typeof(BattleOrderManager), "CreateAction")]
    [HarmonyPrefix]
    static bool CreateActionPrefix(int nActionID, ref IEventAction __result)
    {
        if (ModRegister.IsGlobalId(nActionID) && GlobalRegister.TryGetRegistered<IEventAction>(nActionID, out var action))
        {
            __result = action;
            return false;
        }
        return true;
    }
}
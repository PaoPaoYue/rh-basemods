using HarmonyLib;
using Game;
using System.Collections.Generic;
using System.Reflection.Emit;
using cfg;

namespace BaseMod;

static class UILeaderboardPatch
{

    [HarmonyPatch(typeof(UILeaderboard), "OnShow")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> OnShowTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Stloc_0)
            )
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_0),
                Transpilers.EmitDelegate(FilterModRolePatch),
                new CodeInstruction(OpCodes.Stloc_0)
            )
            .InstructionEnumeration();
    }

    static List<Role> FilterModRolePatch(List<Role> dataList)
    {
        var filteredList = new List<Role>();

        foreach (var role in dataList)
        {
            if (!GlobalRegister.IsRegistered<Role>(role.Id))
            {
                filteredList.Add(role);
            }
        }

        return filteredList;
    }


}
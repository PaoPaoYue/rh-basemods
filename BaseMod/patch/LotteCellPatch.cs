using HarmonyLib;
using Game;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BaseMod;

static class LotteCellPatch
{

    [HarmonyPatch(typeof(LotteCell), "UpdateAttribute")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> UpdateAttributeTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LotteCell), "objAttr")),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LotteCell), "mAttrIndex"))
            )
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0), // load LotteCell instance
                new CodeInstruction(OpCodes.Ldloc_0), // load ElementEntity argument
                Transpilers.EmitDelegate(UpdateAttributePatch)
            )
            .InstructionEnumeration();
    }

    static void UpdateAttributePatch(LotteCell __instance, ElementEntity elementData)
    {
        foreach(var (id, attr) in GlobalRegister.EnumerateRegistered<cfg.Attribute>())
        {
            ReflectionUtil.TryInvokePrivateMethod(__instance, "AddAttribute", [elementData, id]);
        }
    }
}
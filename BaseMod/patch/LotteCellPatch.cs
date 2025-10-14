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
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsField(AccessTools.Field(typeof(LotteCell), "objAttr")))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return Transpilers.EmitDelegate(UpdateAttributePatch);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                found = true;
            }
            yield return instruction;
        }
        if (found is false)
            Plugin.Logger.LogError("Failed to patch LotteCell.UpdateAttribute, field ModElementConf not found!");
    }

    static void UpdateAttributePatch(LotteCell __instance, ElementEntity elementData)
    {
        foreach(var (id, attr) in GlobalRegister.EnumerateRegistered<cfg.Attribute>())
        {
            ReflectionUtil.TryInvokePrivateMethod(__instance, "UpdateAttributeIcon", [elementData, id]);
        }
    }
}
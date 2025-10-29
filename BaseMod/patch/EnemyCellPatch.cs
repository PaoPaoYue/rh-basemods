using HarmonyLib;
using Game;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BaseMod;

static class EnemyCellPatch
{
    [HarmonyPatch(typeof(EnemyCell), "UpdateAttribute")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> UpdateAttributeTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Stloc_S)
            )
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_0),
                Transpilers.EmitDelegate(UpdateAttributePatch)
            )
            .InstructionEnumeration();
    }

    static int UpdateAttributePatch(int nIndex, EnemyCell __instance, EnemyEntity enemyEntity)
    {
        foreach (var (id, attr) in GlobalRegister.EnumerateRegistered<cfg.Attribute>())
        {
            int value = enemyEntity.GetAttribute(id);
            if (value > 0)
            {
                if (ReflectionUtil.TryInvokePrivateMethod(__instance, "CreateBuff", out BuffCell buffCell, [nIndex]))
                {
                    buffCell.Show(id, value);
                    nIndex++;
                }
                else
                {
                    Debug.LogError("Failed to invoke CreateAttribute in PlayerInfoPatch");
                }
            }
        }
        return nIndex;
    }
}
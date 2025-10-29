using HarmonyLib;
using Game;
using UnityEngine.UI;
using UnityEngine;
using System;
using cfg;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BaseMod;

static class PlayerInfoPatch
{

    [HarmonyPatch(typeof(PlayerInfo), "Show")]
    [HarmonyPrefix]
    static bool ShowPrefix(PlayerInfo __instance, ref Sprite ___mOriginSprite, ref Sprite ___mHurtSprite, ref Sprite ___mWinSprite, ref Sprite ___mFailSprite, ref Image ___imgIcon, ref EEntityType ___mOwner, int nRoleID, EEntityType rOwner)
    {
        if (GlobalRegister.TryGetRegistered<Role>(nRoleID, out var role))
        {
            UpdateIcon(role.Icon, ref ___mOriginSprite, ref ___mHurtSprite, ref ___mWinSprite, ref ___mFailSprite, ref ___imgIcon);
            ___mOwner = rOwner;
            return false;
        }
        return true;
    }

    static void UpdateIcon(string rSpriteName, ref Sprite ___mOriginSprite, ref Sprite ___mHurtSprite, ref Sprite ___mWinSprite, ref Sprite ___mFailSprite, ref Image ___imgIcon)
    {
        try
        {
            ___mOriginSprite = Singleton<Model>.Instance.Mod.GetModSprite(rSpriteName);
            ___mHurtSprite = Singleton<Model>.Instance.Mod.GetModSprite(rSpriteName + "_1");
            ___mWinSprite = Singleton<Model>.Instance.Mod.GetModSprite(rSpriteName + "_2");
            ___mFailSprite = Singleton<Model>.Instance.Mod.GetModSprite(rSpriteName + "_3");

            ___imgIcon.sprite = ___mOriginSprite;
            ___imgIcon.SetNativeSize();
        }
        catch (Exception ex)
        {
            Debug.LogError($"UpdateIcon failed: {ex}");
        }
    }

    [HarmonyPatch(typeof(PlayerInfo), "UpdatePlayerAttribute")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> UpdateAttributeTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Stloc_2)
            )
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_1),
                Transpilers.EmitDelegate(UpdatePlayerAttributePatch),
                new CodeInstruction(OpCodes.Stloc_0)
            )
            .InstructionEnumeration();
    }

    static int UpdatePlayerAttributePatch(PlayerInfo __instance, int nIndex, PlayerEntity playerEntity)
    {
        foreach (var (id, attr) in GlobalRegister.EnumerateRegistered<cfg.Attribute>())
        {
            int value = playerEntity.GetAttribute(id);
            if (value > 0)
            {
                if (ReflectionUtil.TryInvokePrivateMethod(__instance, "CreateAttribute", out BuffCell buffCell, [nIndex]))
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
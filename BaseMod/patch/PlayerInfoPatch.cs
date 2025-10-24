using HarmonyLib;
using Game;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using System;
using cfg;

namespace BaseMod;

static class PlayerInfoPatch
{

    [HarmonyPatch(typeof(PlayerInfo), "Show")]
    [HarmonyPrefix]
    static bool ShowPrefix(PlayerInfo __instance,  ref Sprite ___mOriginSprite, ref Sprite ___mHurtSprite, ref Sprite ___mWinSprite, ref Sprite ___mFailSprite, ref Image ___imgIcon, ref EEntityType ___mOwner, int nRoleID, EEntityType rOwner)
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
}
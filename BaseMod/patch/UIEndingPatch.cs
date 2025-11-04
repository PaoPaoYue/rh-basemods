using HarmonyLib;
using Game;
using cfg;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace BaseMod;

static class UIEndingPatch
{

    [HarmonyPatch(typeof(UIEnding), "OnShow")]
    [HarmonyPostfix]
    static void OnShowPostfix(Image ___imgRole)
    {
        int roleID = Singleton<BattleManager>.Instance.Player.RoleID;
        if (GlobalRegister.TryGetRegistered<Role>(roleID, out var roleConf))
        {
            string iconPath = string.Format("{0}_4", roleConf.Icon);
            Sprite roleSprite = Singleton<Model>.Instance.Mod.GetModSprite(iconPath);
            if (roleSprite != null)
            {
                ___imgRole.sprite = roleSprite;
            }
        }
    }
}
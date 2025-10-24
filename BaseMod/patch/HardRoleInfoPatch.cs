using HarmonyLib;
using Game;
using cfg;
using UnityEngine;
using UnityEngine.UI;

namespace BaseMod;

static class HardRoleInfoPatch
{

    [HarmonyPatch(typeof(HardRoleInfo), "SetRole")]
    [HarmonyPrefix]
    static void SetRolePrefix(HardRoleInfo __instance, Role rRoleConf, ref int ___mRoleID, ref GameObject ___mObjRole, Transform ___transIcon)
    {
        if (rRoleConf.Id != ___mRoleID && GlobalRegister.IsRegistered<Role>(rRoleConf.Id))
        {
            if (___mObjRole != null)
            {
                Object.Destroy(___mObjRole);
                ___mObjRole = null;
            }


            ___mObjRole = new("RoleHalfModObj");
            Image battleCryImage = ___mObjRole.AddComponent<Image>();

            battleCryImage.sprite = Singleton<Model>.Instance.Mod.GetModSprite(rRoleConf.Half);
            ___mObjRole.transform.SetParent(___transIcon, false); 

            RectTransform rectTransform = ___mObjRole.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(550, 715);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            ___mRoleID = rRoleConf.Id;
        }
    }
    
}
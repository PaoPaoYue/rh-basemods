using HarmonyLib;
using Game;
using cfg;
using TMPro;
using UnityEngine;

namespace BaseMod;

static class RelicTipCellPatch
{

    [HarmonyPatch(typeof(RelicTipCell), "show")]
    [HarmonyPostfix]
    static void ShowPostfix(ref RelicTipCell __instance, Relics rRelicConf)
    {
        if (ReflectionUtil.TryGetPrivateField<GameObject>(__instance, "objRelicOtherTip", out var objRelicOtherTipField))
        {            
            foreach (var tip in rRelicConf.DescTip)
            {
                if (ModRegister.IsGlobalId((int)tip) && GlobalRegister.TryGetRegistered<DescTip>((int)tip, out var descTip))
                {
                    if (ReflectionUtil.TryGetPrivateField<TextMeshProUGUI>(__instance, "txRelicOtherTipTitle", out var txRelicOtherTipTitleField))
                        txRelicOtherTipTitleField.text = Singleton<Model>.Instance.Localize.GetLocalize(descTip.Title);

                    if (ReflectionUtil.TryGetPrivateField<TextMeshProUGUI>(__instance, "txRelicOtherTipDesc", out var txRelicOtherTipDescField))
                        txRelicOtherTipDescField.text = Singleton<Model>.Instance.Localize.GetLocalize(descTip.Desc);

                        objRelicOtherTipField.SetActive(true);

                    break;
                }
            }
        }
    }
}
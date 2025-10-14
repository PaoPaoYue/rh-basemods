using HarmonyLib;
using Game;
using cfg;
using TMPro;
using UnityEngine;

namespace BaseMod;

static class ModElementTipCellPatch
{

    [HarmonyPatch(typeof(ModElementTipCell), "show")]
    [HarmonyPostfix]
    static void ShowPostfix(ref ModElementTipCell __instance)
    {
        Element tipConf = Singleton<Model>.Instance.Element.TipConf;
        foreach (var tip in tipConf.Desctip)
        {
            if (ModRegister.IsGlobalId((int)tip) && GlobalRegister.TryGetRegistered<DescTip>((int)tip, out var descTip))
            {
                if (ReflectionUtil.TryGetPrivateField<TextMeshProUGUI>(__instance, "txElementOtherTipTitle", out var txElementOtherTipTitleField))
                    txElementOtherTipTitleField.text = Singleton<Model>.Instance.Localize.GetLocalize(descTip.Title);

                if (ReflectionUtil.TryGetPrivateField<TextMeshProUGUI>(__instance, "txElementOtherTipDesc", out var txElementOtherTipDescField))
                    txElementOtherTipDescField.text = Singleton<Model>.Instance.Localize.GetLocalize(descTip.Desc);

                if (ReflectionUtil.TryGetPrivateField<GameObject>(__instance, "objElementOtherTip", out var objElementOtherTipField))
                    objElementOtherTipField.SetActive(true);

                break;
            }
        }
    }
}
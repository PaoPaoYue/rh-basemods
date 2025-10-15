using HarmonyLib;
using Game;
using cfg;
using TMPro;
using UnityEngine;

namespace BaseMod;

static class ElementTipCellPatch
{

    [HarmonyPatch(typeof(ElementTipCell), "Show")]
    [HarmonyPostfix]
    static void ShowPostfix(ref ElementTipCell __instance, TextMeshProUGUI ___txElementOtherTipTitle, TextMeshProUGUI ___txElementOtherTipDesc, GameObject ___objElementOtherTip)
    {
        Element tipConf = Singleton<Model>.Instance.Element.TipConf;
        foreach (var tip in tipConf.Desctip)
        {
            if (ModRegister.IsGlobalId((int)tip) && GlobalRegister.TryGetRegistered<DescTip>((int)tip, out var descTip))
            {
                ___txElementOtherTipTitle.text = Singleton<Model>.Instance.Localize.GetLocalize(descTip.Title);
                ___txElementOtherTipDesc.text = Singleton<Model>.Instance.Localize.GetLocalize(descTip.Desc);
                ___objElementOtherTip.SetActive(true);
                break;
            }
        }
    }
}
using HarmonyLib;
using Game;
using cfg;
using TMPro;
using UnityEngine;

namespace BaseMod;

static class RelicTipCellPatch
{

    [HarmonyPatch(typeof(RelicTipCell), "Show")]
    [HarmonyPostfix]
    static void ShowPostfix(RelicTipCell __instance, Relics rRelicConf, TextMeshProUGUI ___txRelicOtherTipTitle, TextMeshProUGUI ___txRelicOtherTipDesc, GameObject ___objRelicOtherTip)
    {
        if (___objRelicOtherTip != null)
        {            
            foreach (var tip in rRelicConf.DescTip)
            {
                if (ModRegister.IsGlobalId((int)tip) && GlobalRegister.TryGetRegistered<DescTip>((int)tip, out var descTip))
                {
                    ___txRelicOtherTipTitle.text = Singleton<Model>.Instance.Localize.GetLocalize(descTip.Title);
                    ___txRelicOtherTipDesc.text = Singleton<Model>.Instance.Localize.GetLocalize(descTip.Desc);
                    ___objRelicOtherTip.SetActive(true);

                    break;
                }
            }
        }
    }
}
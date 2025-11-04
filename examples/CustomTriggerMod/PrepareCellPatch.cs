using HarmonyLib;
using Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomTriggerMod;

static class PrepareCellPatch
{

    static List<GameObject> batteCrayImageObjs = new(4);
    static List<Image> battleCryImages = new(4);

    [HarmonyPatch(typeof(PrepareCell), "Initialize")]
    [HarmonyPrefix]
    static void InitializePrefix(int nIndex, Image ___imgIcon)
    {
        GameObject imageObj = new GameObject("BattleCryImageObj");
        Image battleCryImage = imageObj.AddComponent<Image>();
        // battleCryImage.enabled = true;
        battleCryImage.sprite = Singleton<Model>.Instance.Mod.GetModSprite("hasDoneBattleCry");
        imageObj.transform.SetParent(___imgIcon.transform.parent, false); // 保持和原图同级
        // imageObj.transform.SetAsLastSibling(); // 确保在最上层

        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(32, 32);
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);
        rectTransform.pivot = new Vector2(1, 0);
        // battleCryImage.canvas.sortingOrder = ___imgLevel1.canvas.sortingOrder + 1;
        batteCrayImageObjs.Add(imageObj);
        battleCryImages.Add(battleCryImage);


    }

    [HarmonyPatch(typeof(PrepareCell), "UpdateData")]
    [HarmonyPostfix]
    static void UpdatePostfix(PrepareCell __instance)
    {
        int index = __instance.Index;
        ReflectionUtil.TryInvokePrivateMethod<ElementEntity>(__instance, "GetElementData", out ElementEntity elementData);
        if (elementData == null)
        {
            Plugin.Logger.LogError("Failed to patch PrepareCell.UpdateData, GetElementData returned null!");
            return;
        }
		if (elementData.Fill)
        {
            var elementConf = Singleton<Model>.Instance.Element.GetElementConf(elementData.ID);
            var hasDoneBattleCryAttrId = Plugin.Register.GetEntityAttributeId(100001);
            if (elementConf.Desctip != null && elementConf.Desctip.Contains((cfg.element.Etip)hasDoneBattleCryAttrId) && elementData.GetAttribute(hasDoneBattleCryAttrId) == 0)
            {
                batteCrayImageObjs[index].SetActive(true);
                return;
            }
            else
            {
                Plugin.Logger.LogInfo($"PrepareCellPatch: Hiding BattleCryImageObj for index {index}");
            }
        }
        batteCrayImageObjs[index].SetActive(false);
    }
}
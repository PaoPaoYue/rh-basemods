using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using cfg;
using cfg.element;
using Game;
using UnityEngine;

namespace BaseMod;

static class ModModelPatch
{

    [HarmonyPatch(typeof(ModModel), "LoadElementData")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> LoadElementDataTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.StoresField(AccessTools.Field(typeof(ModModel), "ModElementConf")))
            {
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return Transpilers.EmitDelegate(LoadElementDataPatch);
                found = true;
                continue;
            }
            yield return instruction;
        }
        if (found is false)
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, field ModElementConf not found!");
    }
    
    static void LoadElementDataPatch(ModModel __instance)
    {
        if (__instance.ModElementConf.DataMap == null || __instance.ModElementConf.DataList == null)
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, fields DataMap or DataList not found!");
            return;
        }

        var modName = __instance.mModData.ModName.RStrip("(debug)");
        if (string.IsNullOrEmpty(modName))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, ModName is null or empty!");
            return;
        }

        foreach (KeyValuePair<int, Element> entry in __instance.ModElementConf.DataMap)
        {
            var element = entry.Value;
            OverwriteElement(modName, element);
        }
        foreach (Element element in __instance.ModElementConf.DataList)
        {
            OverwriteElement(modName, element);
        }
    }

    static void OverwriteElement(string modName, Element element)
    {
        if (ModRegister.IsValidModId(element.TriggerType) && GlobalRegister.TryGetGlobalId<ElementTrigger>(modName, element.TriggerType, out int triggerGlobalId))
        {
            Plugin.Logger.LogDebug($"Overwrite Element {element.Id} TriggerType from {element.TriggerType} to {triggerGlobalId}");
            ReflectionUtil.TrySetReadonlyField(element, "TriggerType", triggerGlobalId);
        }
        if (ModRegister.IsValidModId(element.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, element.TriggerAction, out int actionGlobalId))
        {
            Plugin.Logger.LogDebug($"Overwrite Element {element.Id} TriggerAction from {element.TriggerAction} to {actionGlobalId}");
            ReflectionUtil.TrySetReadonlyField(element, "TriggerAction", actionGlobalId);
        }
        for (int i = 0; i < element.TriggerParam.Count; i++)
        {
            var id = element.TriggerParam[i];
            if (ModRegister.IsValidModId(id) && GlobalRegister.TryGetGlobalId<int>(modName, id, out int globalId))
            {
                Plugin.Logger.LogDebug($"Overwrite Element {element.Id} TriggerParam from {id} to {globalId}");
                element.TriggerParam[i] = globalId;
            }
        }
        for (int i = 0; i < element.TriggerValue.Count; i++)
        {
            var ids = element.TriggerValue[i].Value;
            for (int j = 0; j < ids.Count; j++)
            {
                var id = ids[j];
                if (ModRegister.IsValidModId(id) && GlobalRegister.TryGetGlobalId<int>(modName, id, out int globalId))
                {
                    Plugin.Logger.LogDebug($"Overwrite Element {element.Id} TriggerValue from {id} to {globalId}");
                    ids[j] = globalId;
                }
            }
        }
        for (int i = 0; i < element.OtherValue.Count; i++)
        {
            var id = element.OtherValue[i];
            if (ModRegister.IsValidModId(id) && GlobalRegister.TryGetGlobalId<int>(modName, id, out int globalId))
            {
                Plugin.Logger.LogDebug($"Overwrite Element {element.Id} OtherValue from {id} to {globalId}");
                element.OtherValue[i] = globalId;
            }
        }
        for (int i = 0; i < element.Desctip.Count; i++)
        {
            var tip = (int)element.Desctip[i];
            if (ModRegister.IsValidModId(tip) && GlobalRegister.TryGetGlobalId<DescTip>(modName, tip, out int descTipGlobalId))
            {
                Plugin.Logger.LogDebug($"Overwrite Element {element.Id} Desctip from {tip} to {descTipGlobalId}");
                element.Desctip[i] = (Etip)descTipGlobalId;
            }
        }
    }

    [HarmonyPatch(typeof(ModModel), "LoadRelicData")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> LoadRelicDataTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.StoresField(AccessTools.Field(typeof(ModModel), "ModRelicConf")))
            {
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return Transpilers.EmitDelegate(LoadRelicDataPatch);
                found = true;
                continue;
            }
            yield return instruction;
        }
        if (found is false)
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, field ModRelicConf not found!");
    }

    static void LoadRelicDataPatch(ModModel __instance)
    {
        if (__instance.ModRelicConf.DataMap == null || __instance.ModRelicConf.DataList == null)
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, fields DataMap or DataList not found!");
            return;
        }

        var modName = __instance.mModData.ModName.RStrip("(debug)");
        if (string.IsNullOrEmpty(modName))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, ModName is null or empty!");
            return;
        }

        foreach (KeyValuePair<int, Relics> entry in __instance.ModRelicConf.DataMap)
        {
            var relic = entry.Value;
            OverwriteRelic(modName, relic);
        }
        foreach (Relics relic in __instance.ModRelicConf.DataList)
        {
            OverwriteRelic(modName, relic);
        }
    }

    static void OverwriteRelic(string modName, Relics relic)
    {
        if (ModRegister.IsValidModId(relic.TriggerType) && GlobalRegister.TryGetGlobalId<RelicTrigger>(modName, relic.TriggerType, out int triggerGlobalId))
        {
            Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} TriggerType from {relic.TriggerType} to {triggerGlobalId}");
            ReflectionUtil.TrySetReadonlyField(relic, "TriggerType", triggerGlobalId);
        }
        if (ModRegister.IsValidModId(relic.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, relic.TriggerAction, out int actionGlobalId))
        {
            Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} TriggerAction from {relic.TriggerAction} to {actionGlobalId}");
            ReflectionUtil.TrySetReadonlyField(relic, "TriggerAction", actionGlobalId);
        }

        for (int i = 0; i < relic.Passive.Count; i++)
        {
            var id = relic.Passive[i];
            if (ModRegister.IsValidModId(id) && GlobalRegister.TryGetGlobalId<int>(modName, id, out int globalId))
            {
                Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} Passive from {id} to {globalId}");
                relic.Passive[i] = globalId;
            }
        }

        for (int i = 0; i < relic.TriggerParam.Count; i++)
        {
            var id = relic.TriggerParam[i];
            if (ModRegister.IsValidModId(id) && GlobalRegister.TryGetGlobalId<int>(modName, id, out int globalId))
            {
                Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} TriggerParam from {id} to {globalId}");
                relic.TriggerParam[i] = globalId;
            }
        }
        for (int i = 0; i < relic.TriggerValue.Count; i++)
        {
            var id = relic.TriggerValue[i];
            if (ModRegister.IsValidModId(id) && GlobalRegister.TryGetGlobalId<int>(modName, id, out int globalId))
            {
                Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} TriggerValue from {id} to {globalId}");
                relic.TriggerValue[i] = globalId;
            }
        }
        for (int i = 0; i < relic.OtherValue.Count; i++)
        {
            var id = relic.OtherValue[i];
            if (ModRegister.IsValidModId(id) && GlobalRegister.TryGetGlobalId<int>(modName, id, out int globalId))
            {
                Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} OtherValue from {id} to {globalId}");
                relic.OtherValue[i] = globalId;
            }
        }
        for (int i = 0; i < relic.DescTip.Count; i++)
        {
            var tip = (int)relic.DescTip[i];
            if (ModRegister.IsValidModId(tip) && GlobalRegister.TryGetGlobalId<DescTip>(modName, tip, out int descTipGlobalId))
            {
                Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} DescTip from {tip} to {descTipGlobalId}");
                relic.DescTip[i] = (Etip)descTipGlobalId;
            }
        }
    }

    [HarmonyPatch(typeof(ModModel), "LoadLocalizationData")]
    [HarmonyPostfix]
    static void LoadLocalizationDataPostfix(ModModel __instance, string rPath)
    {
        foreach (var (id, value) in GlobalRegister.EnumerateRegistered<Localization>())
        {
            __instance.ModLocalizationConf.DataMap[id] = value;
            __instance.ModLocalizationConf.DataList.Add(value);
        }
    }

    [HarmonyPatch(typeof(ModModel), "GetModSprite")]
    [HarmonyPostfix]
    static void GetModSpritePostfix(ModModel __instance, ref Sprite __result, string rSpriteName)
    {
        if (__result == null && ResourceScanner.TryGetSprite(rSpriteName, out var sprite))
        {
            __result = sprite;
        }
    }
    
    [HarmonyPatch(typeof(ModModel), "PlaySound")]
    [HarmonyPostfix]
    static void PlaySoundPostfix(ModModel __instance, ref AudioSource ___mAudioSource, ref bool __result, int nSoundID)
    {
        if (!__result && ResourceScanner.TryGetSound(nSoundID, out var clip))
        {
            if (___mAudioSource == null)
            {
                GameObject gameObject = new GameObject("audioObject");
                ___mAudioSource = gameObject.AddComponent<AudioSource>();
            }
            ___mAudioSource.Stop();
            ___mAudioSource.clip = clip;
            ___mAudioSource.Play();
            __result = true;
        }
    }


}
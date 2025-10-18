using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Game;
using cfg;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine;
using cfg.element;

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
            ReflectionUtil.TrySetReadonlyField(element, "TriggerType", triggerGlobalId);
            Plugin.Logger.LogDebug($"Overwrite Element {element.Id} TriggerType from {element.TriggerType} to {triggerGlobalId}");
        }
        if (ModRegister.IsValidModId(element.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, element.TriggerAction, out int actionGlobalId))
        {
            ReflectionUtil.TrySetReadonlyField(element, "TriggerAction", actionGlobalId);
            Plugin.Logger.LogDebug($"Overwrite Element {element.Id} TriggerAction from {element.TriggerAction} to {actionGlobalId}");
        }

        bool changed = false;
        var desctipCopy = new List<Etip>(element.Desctip);
        for (int i = 0; i < desctipCopy.Count; i++)
        {
            var tip = (int)desctipCopy[i];
            if (ModRegister.IsValidModId(tip) && GlobalRegister.TryGetGlobalId<DescTip>(modName, tip, out int descTipGlobalId))
            {
                desctipCopy[i] = (Etip)descTipGlobalId;
                Plugin.Logger.LogDebug($"Overwrite Element {element.Id} Desctip from {tip} to {descTipGlobalId}");
                changed = true;
            }
        }
        if (changed)
            ReflectionUtil.TrySetReadonlyField(element, "Desctip", desctipCopy);
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
            ReflectionUtil.TrySetReadonlyField(relic, "TriggerType", triggerGlobalId);
            Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} TriggerType from {relic.TriggerType} to {triggerGlobalId}");
        }
        if (ModRegister.IsValidModId(relic.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, relic.TriggerAction, out int actionGlobalId))
        {
            ReflectionUtil.TrySetReadonlyField(relic, "TriggerAction", actionGlobalId);
            Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} TriggerAction from {relic.TriggerAction} to {actionGlobalId}");
        }

        bool changed = false;
        var descTipCopy = new List<Etip>(relic.DescTip);
        for (int i = 0; i < descTipCopy.Count; i++)
        {
            var tip = (int)descTipCopy[i];
            if (ModRegister.IsValidModId(tip) && GlobalRegister.TryGetGlobalId<DescTip>(modName, tip, out int descTipGlobalId))
            {
                descTipCopy[i] = (Etip)descTipGlobalId;
                Plugin.Logger.LogDebug($"Overwrite Relic {relic.Id} DescTip from {tip} to {descTipGlobalId}");
                changed = true;
            }
        }
        if (changed)
            ReflectionUtil.TrySetReadonlyField(relic, "DescTip", descTipCopy);
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
    
    [HarmonyPatch(typeof(ModModel), "LoadModOther")]
    [HarmonyPostfix]
    static void LoadModOtherPostfix(ModModel __instance)
    {
        foreach (var (name, sprite) in ResourceScanner.GetAllSprites())
        {
            __instance.ModSpriteDict[name] = sprite;
        }
        foreach (var (id, audioClip) in ResourceScanner.GetAllAudioClips())
        {
            __instance.modAudio[id] = audioClip;
        }
    }

    private static string RStrip(this string s, string suffix)
    {
        if (s != null && suffix != null && s.EndsWith(suffix))
        {
            return s[..^suffix.Length];
        }
        return s;
    }
}
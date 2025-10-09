using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Game;
using cfg;

namespace BaseMod;

static class ModModelPatch
{
    static void LoadElementDataPatch(ModModel __instance)
    {
        if (!ReflectionUtil.TryGetPrivateField(__instance.ModElementConf, "_dataMap", out Dictionary<int, Element> dataMap) ||
            !ReflectionUtil.TryGetPrivateField(__instance.ModElementConf, "_dataList", out List<Element> dataList))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, fields _dataMap or _dataList not found!");
            return;
        }

        var modName = __instance.mModData.ModName;
        if (string.IsNullOrEmpty(modName))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, ModName is null or empty!");
            return;
        }

        foreach (KeyValuePair<int, Element> entry in dataMap)
        {
            var element = entry.Value;
            if (ModRegister.IsValidModId(element.TriggerType) && GlobalRegister.TryGetGlobalId<IElementTrigger>(modName, element.TriggerType, out int triggerGlobalId))
            {
                ReflectionUtil.TrySetReadonlyField(element, "TriggerType", triggerGlobalId);
            }
            if (ModRegister.IsValidModId(element.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, element.TriggerAction, out int actionGlobalId))
            {
                ReflectionUtil.TrySetReadonlyField(element, "TriggerAction", actionGlobalId);
            }
        }
        foreach (Element element in dataList)
        {
            if (ModRegister.IsValidModId(element.TriggerType) && GlobalRegister.TryGetGlobalId<IElementTrigger>(modName, element.TriggerType, out int triggerGlobalId))
            {
                ReflectionUtil.TrySetReadonlyField(element, "TriggerType", triggerGlobalId);
            }
            if (ModRegister.IsValidModId(element.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, element.TriggerAction, out int actionGlobalId))
            {
                ReflectionUtil.TrySetReadonlyField(element, "TriggerAction", actionGlobalId);
            }
        }
    }

    [HarmonyPatch(typeof(ModModel), "LoadElementData")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> LoadElementDataTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.StoresField(AccessTools.Field(typeof(ModModel), "ModElementConf")))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return Transpilers.EmitDelegate(LoadElementDataPatch);
                found = true;
            }
            yield return instruction;
        }
        if (found is false)
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, field ModElementConf not found!");
    }

    static void LoadRelicDataPatch(ModModel __instance)
    {
        if (!ReflectionUtil.TryGetPrivateField(__instance.ModRelicConf, "_dataMap", out Dictionary<int, Relics> dataMap) ||
            !ReflectionUtil.TryGetPrivateField(__instance.ModRelicConf, "_dataList", out List<Relics> dataList))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, fields _dataMap or _dataList not found!");
            return;
        }

        var modName = __instance.mModData.ModName;
        if (string.IsNullOrEmpty(modName))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, ModName is null or empty!");
            return;
        }

        foreach (KeyValuePair<int, Relics> entry in dataMap)
        {
            var relic = entry.Value;
            if (ModRegister.IsValidModId(relic.TriggerType) && GlobalRegister.TryGetGlobalId<IRelicTrigger>(modName, relic.TriggerType, out int triggerGlobalId))
            {
                ReflectionUtil.TrySetReadonlyField(relic, "TriggerType", triggerGlobalId);
            }
            if (ModRegister.IsValidModId(relic.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, relic.TriggerAction, out int actionGlobalId))
            {
                ReflectionUtil.TrySetReadonlyField(relic, "TriggerAction", actionGlobalId);
            }
        }
        foreach (Relics relic in dataList)
        {
            if (ModRegister.IsValidModId(relic.TriggerType) && GlobalRegister.TryGetGlobalId<IRelicTrigger>(modName, relic.TriggerType, out int triggerGlobalId))
            {
                ReflectionUtil.TrySetReadonlyField(relic, "TriggerType", triggerGlobalId);
            }
            if (ModRegister.IsValidModId(relic.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, relic.TriggerAction, out int actionGlobalId))
            {
                ReflectionUtil.TrySetReadonlyField(relic, "TriggerAction", actionGlobalId);
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
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return Transpilers.EmitDelegate(LoadRelicDataPatch);
                found = true;
            }
            yield return instruction;
        }
        if (found is false)
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, field ModRelicConf not found!");
    }
}
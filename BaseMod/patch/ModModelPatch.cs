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
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return Transpilers.EmitDelegate(LoadElementDataPatch);
                found = true;
            }
            yield return instruction;
        }
        if (found is false)
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, field ModElementConf not found!");
    }
    
    static void LoadElementDataPatch(ModModel __instance)
    {
        if (!ReflectionUtil.TryGetPrivateField(__instance.ModElementConf, "_dataMap", out Dictionary<int, Element> dataMap) ||
            !ReflectionUtil.TryGetPrivateField(__instance.ModElementConf, "_dataList", out List<Element> dataList))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, fields _dataMap or _dataList not found!");
            return;
        }

        var modName = __instance.mModData.ModName.RStrip("(debug)");
        if (string.IsNullOrEmpty(modName))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadElementData, ModName is null or empty!");
            return;
        }

        foreach (KeyValuePair<int, Element> entry in dataMap)
        {
            var element = entry.Value;
            OverwriteElement(modName, element);
        }
        foreach (Element element in dataList)
        {
            OverwriteElement(modName,element);
        }
    }

    static void OverwriteElement(string modName, Element element)
    {
        if (ModRegister.IsValidModId(element.TriggerType) && GlobalRegister.TryGetGlobalId<ElementTrigger>(modName, element.TriggerType, out int triggerGlobalId))
        {
            ReflectionUtil.TrySetReadonlyField(element, "TriggerType", triggerGlobalId);
        }
        if (ModRegister.IsValidModId(element.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, element.TriggerAction, out int actionGlobalId))
        {
            ReflectionUtil.TrySetReadonlyField(element, "TriggerAction", actionGlobalId);
        }

        bool changed = false;
        var desctipCopy = new List<Etip>(element.Desctip);
        for (int i = 0; i < desctipCopy.Count; i++)
        {
            var tip = (int)desctipCopy[i];
            if (ModRegister.IsValidModId(tip) && GlobalRegister.TryGetGlobalId<DescTip>(modName, tip, out int descTipGlobalId))
            {
                desctipCopy[i] = (Etip)descTipGlobalId;
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
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return Transpilers.EmitDelegate(LoadRelicDataPatch);
                found = true;
            }
            yield return instruction;
        }
        if (found is false)
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, field ModRelicConf not found!");
    }

    static void LoadRelicDataPatch(ModModel __instance)
    {
        if (!ReflectionUtil.TryGetPrivateField(__instance.ModRelicConf, "_dataMap", out Dictionary<int, Relics> dataMap) ||
            !ReflectionUtil.TryGetPrivateField(__instance.ModRelicConf, "_dataList", out List<Relics> dataList))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, fields _dataMap or _dataList not found!");
            return;
        }

        var modName = __instance.mModData.ModName.RStrip("(debug)");
        if (string.IsNullOrEmpty(modName))
        {
            Plugin.Logger.LogError("Failed to patch ModModel.LoadRelicData, ModName is null or empty!");
            return;
        }

        foreach (KeyValuePair<int, Relics> entry in dataMap)
        {
            var relic = entry.Value;
            OverwriteRelic(modName, relic);
        }
        foreach (Relics relic in dataList)
        {
            OverwriteRelic(modName, relic);
        }
    }

    static void OverwriteRelic(string modName, Relics relic)
    {
        if (ModRegister.IsValidModId(relic.TriggerType) && GlobalRegister.TryGetGlobalId<RelicTrigger>(modName, relic.TriggerType, out int triggerGlobalId))
        {
            ReflectionUtil.TrySetReadonlyField(relic, "TriggerType", triggerGlobalId);
        }
        if (ModRegister.IsValidModId(relic.TriggerAction) && GlobalRegister.TryGetGlobalId<IEventAction>(modName, relic.TriggerAction, out int actionGlobalId))
        {
            ReflectionUtil.TrySetReadonlyField(relic, "TriggerAction", actionGlobalId);
        }

        bool changed = false;
        var descTipCopy = new List<Etip>(relic.DescTip);
        for (int i = 0; i < descTipCopy.Count; i++)
        {
            var tip = (int)descTipCopy[i];
            if (ModRegister.IsValidModId(tip) && GlobalRegister.TryGetGlobalId<DescTip>(modName, tip, out int descTipGlobalId))
            {
                descTipCopy[i] = (Etip)descTipGlobalId;
                changed = true;
            }
        }
        if (changed)
            ReflectionUtil.TrySetReadonlyField(relic, "DescTip", descTipCopy);
    }


    [HarmonyPatch(typeof(ModModel), "LoadModImage")]
    [HarmonyPrefix]
    static bool LoadModImagePrefix(ModModel __instance, string rPath, ref UniTask<bool> __result)
    {
        __result = LoadModImage(__instance, rPath);
        return false;
    }

    static async UniTask<bool> LoadModImage(ModModel __instance, string rPath)
    {
        // 加载MOD图片
        var rImageDir = rPath + "\\Image_Mod";
        if (Directory.Exists(rImageDir))
        {
            foreach (var rPair in __instance.ModElementConf.DataMap)
            {
                var rIconName = rPair.Value.Icon;
                ReflectionUtil.TryInvokePrivateMethod(__instance, "LoadImage", [rImageDir, rIconName]);
                await UniTask.WaitForSeconds(0.05f);
            }

            foreach (var rPair in __instance.ModRaceConf.DataMap)
            {
                var rIconName = rPair.Value.Icon;
                ReflectionUtil.TryInvokePrivateMethod(__instance, "LoadImage", [rImageDir, rIconName]);
                await UniTask.WaitForSeconds(0.05f);
            }
        }
        else
        {
            LogUtil.LogError("image not exist");
            GameEventManager.Instance.Dispatch(EventName.OnModLoadFinish, Model.Instance.Localize.GetLocalize(182, "png"));
            return false;
        }
        var extraImageDir = rPath + "\\Extra_Image_Mod";
        if (Directory.Exists(extraImageDir))
        {
            // enumarate all png files in the Extra_Image_Mod folder and load them
            foreach (var file in Directory.GetFiles(extraImageDir, "*.png", SearchOption.AllDirectories))
            {
                var rIconName = Path.GetFileNameWithoutExtension(file);
                ReflectionUtil.TryInvokePrivateMethod(__instance, "LoadImage", [extraImageDir, rIconName]);
                await UniTask.WaitForSeconds(0.05f);
            }
        }
        return true;
    }

    [HarmonyPatch(typeof(ModModel), "LoadModSound")]
    [HarmonyPrefix]
    static bool LoadModSoundPrefix(ModModel __instance, string rPath, ref UniTask __result)
    {
        __result = LoadModSound(__instance, rPath);
        return false;
    }


    static async UniTask LoadModSound(ModModel __instance, string rPath)
    {
        // 加载MOD图片
        var rSoundDir = rPath + "\\Sound_Mod";
        if (Directory.Exists(rSoundDir))
        {
            var rSoundFiles = Directory.GetFiles(rSoundDir, "*.ogg");
            if (rSoundFiles == null || rSoundFiles.Length == 0)
            {
                return;
            }

            if (__instance.ModElementConf != null)
            {
                foreach (var rPair in __instance.ModElementConf.DataMap)
                {
                    var nSoundID = rPair.Value.SelectSound;

                    if (nSoundID != 0)
                    {
                        var rFileName = rSoundDir + "\\" + nSoundID + ".ogg";
                        if (File.Exists(rFileName))
                        {
                            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(rFileName, AudioType.OGGVORBIS))
                            {
                                var asyncOp = www.SendWebRequest();

                                var nWhileCount = 10000;

                                while (!asyncOp.isDone && nWhileCount > 0)
                                {
                                    nWhileCount--;
                                    await UniTask.Yield();
                                }

                                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                                {
                                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                                    __instance.modAudio[nSoundID] = clip;
                                }
                            }
                        }
                        await UniTask.WaitForSeconds(0.05f);
                    }

                    nSoundID = rPair.Value.AttackSound;
                    if (nSoundID != 0)
                    {
                        var rFileName = rSoundDir + "\\" + nSoundID + ".ogg";
                        if (File.Exists(rFileName))
                        {
                            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(rFileName, AudioType.OGGVORBIS))
                            {
                                var asyncOp = www.SendWebRequest();

                                var nWhileCount = 10000;

                                while (!asyncOp.isDone && nWhileCount > 0)
                                {
                                    nWhileCount--;
                                    await UniTask.Yield();
                                }

                                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                                {
                                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                                    __instance.modAudio[nSoundID] = clip;
                                }
                            }
                        }
                        await UniTask.WaitForSeconds(0.05f);
                    }
                }

            }

            if (__instance.ModEnemyConf != null)
            {
                foreach (var rPair in __instance.ModEnemyConf.DataMap)
                {
                    var nSoundID = rPair.Value.ShowSound;

                    if (nSoundID != 0)
                    {
                        var rFileName = rSoundDir + "\\" + nSoundID + ".ogg";
                        if (File.Exists(rFileName))
                        {
                            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(rFileName, AudioType.OGGVORBIS))
                            {
                                var asyncOp = www.SendWebRequest();

                                var nWhileCount = 10000;

                                while (!asyncOp.isDone && nWhileCount > 0)
                                {
                                    await UniTask.Yield();
                                }

                                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                                {
                                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                                    __instance.modAudio[nSoundID] = clip;
                                }
                            }
                        }
                        await UniTask.WaitForSeconds(0.05f);
                    }

                    nSoundID = rPair.Value.AttackSound;
                    if (nSoundID != 0)
                    {
                        var rFileName = rSoundDir + "\\" + nSoundID + ".ogg";
                        if (File.Exists(rFileName))
                        {
                            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(rFileName, AudioType.OGGVORBIS))
                            {
                                var asyncOp = www.SendWebRequest();

                                var nWhileCount = 10000;

                                while (!asyncOp.isDone && nWhileCount > 0)
                                {
                                    await UniTask.Yield();
                                }

                                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                                {
                                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                                    __instance.modAudio[nSoundID] = clip;
                                }
                            }
                        }
                        await UniTask.WaitForSeconds(0.05f);
                    }
                }
            }
        }
        else
        {
            LogUtil.LogError("sound not exist");
        }
        var extraSoundDir = rPath + "\\Extra_Sound_Mod";
        if (Directory.Exists(extraSoundDir))
        {
            // enumarate all ogg files in the Extra_Sound_Mod folder and load them
            foreach (var file in Directory.GetFiles(extraSoundDir, "*.ogg", SearchOption.AllDirectories))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!int.TryParse(name, out int nSoundID))
                {
                    Plugin.Logger.LogWarning($"Invalid sound file name: {name}, should be an integer ID.");
                    continue;
                }
                if (nSoundID != 0)
                {
                    using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(file, AudioType.OGGVORBIS))
                    {
                        var asyncOp = www.SendWebRequest();

                        var nWhileCount = 10000;
                        while (!asyncOp.isDone && nWhileCount > 0)
                        {
                            nWhileCount--;
                            await UniTask.Yield();
                        }

                        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                            __instance.modAudio[nSoundID] = clip;
                        }
                    }
                    await UniTask.WaitForSeconds(0.05f);
                }
            }
        }
    }
    
    public static string RStrip(this string s, string suffix)
    {
        if (s != null && suffix != null && s.EndsWith(suffix))
        {
            return s[..^suffix.Length];
        }
        return s;
    }
}
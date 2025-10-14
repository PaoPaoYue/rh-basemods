using HarmonyLib;
using Game;
using System.Collections.Generic;
using cfg.attribute;

namespace BaseMod;

static class BuffModelPatch
{

    [HarmonyPatch(typeof(BuffModel), "GetAttributeIcon")]
    [HarmonyPrefix]
    static bool GetAttributeIconPrefix(BuffModel __instance, int nAttributeID, ref string __result)
    {
        if (ReflectionUtil.TryGetPrivateField<Dictionary<int, cfg.Attribute>>(__instance, "mAttributeDict", out var mAttributeDict))
        {
            if (mAttributeDict == null)
            {
                mAttributeDict = DataManager.GetVOData<tbattribute>().DataMap;

                foreach (var (id, attr) in GlobalRegister.EnumerateRegistered<cfg.Attribute>())
                {
                    if (!mAttributeDict.ContainsKey(id))
                        mAttributeDict[id] = attr;
                }

                ReflectionUtil.TrySetPrivateField(__instance, "mAttributeDict", mAttributeDict);
                __result = mAttributeDict[nAttributeID].Icon;
                return false;
            }
        }
        return true;
    }
    
    [HarmonyPatch(typeof(BuffModel), "GetAttributeConf")]
    [HarmonyPrefix]
    static bool GetAttributeConfPrefix(BuffModel __instance, int nAttrID, ref cfg.Attribute __result)
    {
        if (ReflectionUtil.TryGetPrivateField<Dictionary<int, cfg.Attribute>>(__instance, "mAttributeDict", out var mAttributeDict))
        {
            if (mAttributeDict == null)
            {
                mAttributeDict = DataManager.GetVOData<tbattribute>().DataMap;

                foreach(var (id, attr) in GlobalRegister.EnumerateRegistered<cfg.Attribute>())
                {
                    if (!mAttributeDict.ContainsKey(id))
                        mAttributeDict[id] = attr;
                }

                ReflectionUtil.TrySetPrivateField(__instance, "mAttributeDict", mAttributeDict);
                __result = mAttributeDict[nAttrID];
                return false;
            }
        }
        return true;
    }
}
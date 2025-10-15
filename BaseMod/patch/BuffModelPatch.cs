using HarmonyLib;
using Game;
using System.Collections.Generic;
using cfg.attribute;

namespace BaseMod;

static class BuffModelPatch
{

    [HarmonyPatch(typeof(BuffModel), "GetAttributeIcon")]
    [HarmonyPrefix]
    static bool GetAttributeIconPrefix(BuffModel __instance, int nAttributeID, ref Dictionary<int, cfg.Attribute> ___mAttributeDict, ref string __result)
    {

        if (___mAttributeDict == null)
        {
            ___mAttributeDict = DataManager.GetVOData<tbattribute>().DataMap;

            foreach (var (id, attr) in GlobalRegister.EnumerateRegistered<cfg.Attribute>())
            {
                if (!___mAttributeDict.ContainsKey(id))
                    ___mAttributeDict[id] = attr;
            }

            __result = ___mAttributeDict[nAttributeID].Icon;
            return false;
        }
        return true;
    }
    
    [HarmonyPatch(typeof(BuffModel), "GetAttributeConf")]
    [HarmonyPrefix]
    static bool GetAttributeConfPrefix(BuffModel __instance, int nAttrID, ref Dictionary<int, cfg.Attribute> ___mAttributeDict, ref cfg.Attribute __result)
    {
        if (___mAttributeDict == null)
        {
            ___mAttributeDict = DataManager.GetVOData<tbattribute>().DataMap;

            foreach(var (id, attr) in GlobalRegister.EnumerateRegistered<cfg.Attribute>())
            {
                if (!___mAttributeDict.ContainsKey(id))
                    ___mAttributeDict[id] = attr;
            }

            __result = ___mAttributeDict[nAttrID];
            return false;
        }
        return true;
    }
}
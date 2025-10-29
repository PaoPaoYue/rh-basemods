using HarmonyLib;
using Game;
using cfg;
using cfg.role;
using System.Collections.Generic;
using System.Linq;

namespace BaseMod;

static class BattleManagerPatch
{

    [HarmonyPatch(typeof(BattleManager), "ChangePlayer")]
    [HarmonyPrefix]
    static void ChangePlayerPrefix(ref BattleManager __instance, int nRoleID)
    {
        if (DataManager.GetVOData<tbrole>().DataMap.ContainsKey(nRoleID))
            return;
            
        ModModel modModel = Singleton<Model>.Instance.Mod;
        ModRegister register = null;
        string modName = null;

        List<int> modRoleIds = [];
        if (modModel.mModData != null)
        {
            modName = modModel.mModData.ModName.RStrip("(debug)");
            register = GlobalRegister.GetModRegister(modName);
            if (register != null)
            {
                modRoleIds = [.. register.roleDict.Keys];
            }
        }

        // remove non-selected mod roles from DataManager.GetVOData<tbrole>().DataMap and DataList
        var roleDataMap = DataManager.GetVOData<tbrole>().DataMap;
        var roleDataList = DataManager.GetVOData<tbrole>().DataList;
        var toRemove = roleDataMap.Keys
            .Where(id => ModRegister.IsValidModId(id) && !modRoleIds.Contains(id))
            .ToList();

        foreach (var id in toRemove)
        {
            Plugin.Logger.LogDebug(
                $"BattleManagerPatch ChangePlayerPrefix: Removing mod role {id} from role data map"
            );
            roleDataMap.Remove(id);
        }
        roleDataList.RemoveAll(role => toRemove.Contains(role.Id));

        // Re-add mod roles to DataManager.GetVOData<tbrole>().DataMap and DataList, updating TriggerType and TriggerAction 
        if (register == null || modRoleIds.Count == 0)
            return;
        foreach (var (id, value) in register.roleDict)
        {
            if (roleDataMap.ContainsKey(id))
                continue;
            if (ModRegister.IsValidModId(value.TriggerType))
            {
                if (GlobalRegister.TryGetGlobalId<PlayerTrigger>(modName, value.TriggerType, out int triggerGlobalId))
                {
                    Plugin.Logger.LogDebug($"Overwrite Role {value.Id} TriggerType from {value.TriggerType} to {triggerGlobalId}");
                    ReflectionUtil.TrySetReadonlyField(value, "TriggerType", triggerGlobalId);
                }
            }
            if (ModRegister.IsValidModId(value.TriggerAction))
            {
                if (GlobalRegister.TryGetGlobalId<IEventAction>(modName, value.TriggerAction, out int actionGlobalId))
                {
                    Plugin.Logger.LogDebug($"Overwrite Role {value.Id} TriggerAction from {value.TriggerAction} to {actionGlobalId}");
                    ReflectionUtil.TrySetReadonlyField(value, "TriggerAction", actionGlobalId);
                }
            }
            Plugin.Logger.LogDebug($"BattleManagerPatch ChangePlayerPrefix: adding mod role {id} to role data map");
            roleDataMap[id] = value;
            roleDataList.Add(value);
        }
    }
}
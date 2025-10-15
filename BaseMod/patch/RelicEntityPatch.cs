using HarmonyLib;
using System.Reflection;
using System;
using Game;

namespace BaseMod;

class RelicEntityPatch
{
    [HarmonyPatch(typeof(RelicEntity), "Binding")]
    [HarmonyPostfix]
    static void BindingPostfix(RelicEntity __instance, bool ___mBinding)
    {
        if (!___mBinding)
            return;
        var triggerType = Singleton<Model>.Instance.Relic.GetRelicsConf(__instance.ID).TriggerType;
        if (ModRegister.IsGlobalId(triggerType))
        {
            if (GlobalRegister.TryGetRegistered<RelicTrigger>(triggerType, out var trigger))
            {
                if (ReflectionUtil.TryGetPrivateMethod(typeof(RelicEntity), "OnEvent", out MethodInfo onEventMethod))
                    Singleton<GameEventManager>.Instance.Binding(trigger.EventId, (Action<EventArg>)onEventMethod.CreateDelegate(typeof(Action<EventArg>), __instance));
                else
                    Plugin.Logger.LogError("Failed to patch RelicEntity.Binding, method OnEvent not found!");
            }
            else
                Plugin.Logger.LogError($"Failed to patch RelicEntity.Binding, global trigger id {triggerType} not found!");

        }
    }

    [HarmonyPatch(typeof(RelicEntity), "Unbinding")]
    [HarmonyPrefix]
    static void UnbindingPrefix(RelicEntity __instance, bool ___mBinding)
    {
        if (!___mBinding)
            return;
        var triggerType = Singleton<Model>.Instance.Relic.GetRelicsConf(__instance.ID).TriggerType;
        if (ModRegister.IsGlobalId(triggerType))
        {
            if (GlobalRegister.TryGetRegistered<RelicTrigger>(triggerType, out var trigger))
            {
                if (ReflectionUtil.TryGetPrivateMethod(typeof(RelicEntity), "OnEvent", out MethodInfo onEventMethod))
                    Singleton<GameEventManager>.Instance.Unbinding(trigger.EventId, (Action<EventArg>)onEventMethod.CreateDelegate(typeof(Action<EventArg>), __instance));
                else
                    Plugin.Logger.LogError("Failed to patch RelicEntity.Unbinding, method OnEvent not found!");
            }
            else
                Plugin.Logger.LogError($"Failed to patch RelicEntity.Unbinding, global trigger id {triggerType} not found!");
        }
    }

    [HarmonyPatch(typeof(RelicEntity), "OnEvent")]
    [HarmonyPrefix]
    static bool OnEventPrefix(RelicEntity __instance, EventArg rEventArg)
    {
        var RelicConf = Singleton<Model>.Instance.Relic.GetRelicsConf(__instance.ID);
        var triggerType = RelicConf.TriggerType;
        if (!ModRegister.IsGlobalId(triggerType))
            return true;

        if (GlobalRegister.TryGetRegistered<RelicTrigger>(triggerType, out var trigger))
        {
            if (trigger.OnTrigger(__instance, RelicConf, rEventArg, out var ActionParams))
                Singleton<BattleManager>.Instance.OrderManager.AddEventRelic(__instance.Index, __instance.Owner, RelicConf, ActionParams, RelicConf.EventTip);
            return false;
        }
        else
            Plugin.Logger.LogError($"Failed to patch RelicEntity.OnEvent, global trigger id {triggerType} not found!");
        return true;

    }
}
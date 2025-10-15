using HarmonyLib;
using System.Reflection;
using System;
using Game;

namespace BaseMod;

class ElementEntityPatch
{
    [HarmonyPatch(typeof(ElementEntity), "Binding")]
    [HarmonyPostfix]
    static void BindingPostfix(ElementEntity __instance, bool ___mBinding, int ___mTriggerType)
    {
        if (!___mBinding)
            return;
        if (ModRegister.IsGlobalId(___mTriggerType))
        {
            if (GlobalRegister.TryGetRegistered<ElementTrigger>(___mTriggerType, out var trigger))
            {
                if (ReflectionUtil.TryGetPrivateMethod(typeof(ElementEntity), "OnEvent", out MethodInfo onEventMethod))
                    Singleton<GameEventManager>.Instance.Binding(trigger.EventId, (Action<EventArg>)onEventMethod.CreateDelegate(typeof(Action<EventArg>), __instance));
                else
                    Plugin.Logger.LogError("Failed to patch ElementEntity.Binding, method OnEvent not found!");
            }
            else
                Plugin.Logger.LogError($"Failed to patch ElementEntity.Binding, global trigger id {___mTriggerType} not found!");

        }
    }

    [HarmonyPatch(typeof(ElementEntity), "Unbinding")]
    [HarmonyPrefix]
    static void UnbindingPrefix(ElementEntity __instance, bool ___mBinding, int ___mTriggerType)
    {
        if (!___mBinding)
            return;
        if (ModRegister.IsGlobalId(___mTriggerType))
        {
            if (GlobalRegister.TryGetRegistered<ElementTrigger>(___mTriggerType, out var trigger))
            {
                if (ReflectionUtil.TryGetPrivateMethod(typeof(ElementEntity), "OnEvent", out MethodInfo onEventMethod))
                    Singleton<GameEventManager>.Instance.Unbinding(trigger.EventId, (Action<EventArg>)onEventMethod.CreateDelegate(typeof(Action<EventArg>), __instance));
                else
                    Plugin.Logger.LogError("Failed to patch ElementEntity.Unbinding, method OnEvent not found!");
            }
            else
                Plugin.Logger.LogError($"Failed to patch ElementEntity.Unbinding, global trigger id {___mTriggerType} not found!");
        }
    }

    [HarmonyPatch(typeof(ElementEntity), "OnEvent")]
    [HarmonyPrefix]
    static bool OnEventPrefix(ElementEntity __instance, EventArg rEventArg, int ___mTriggerType)
    {
        if (!ModRegister.IsGlobalId(___mTriggerType) || !__instance.Fill || !__instance.Enable || __instance.Wait)
            return true;

        if (GlobalRegister.TryGetRegistered<ElementTrigger>(___mTriggerType, out var trigger))
        {
            var elementConf = Singleton<Model>.Instance.Element.GetElementConf(__instance.ID);
            if (trigger.OnTrigger(__instance, elementConf, rEventArg, out var ActionParams))
                Singleton<BattleManager>.Instance.OrderManager.AddEventElement(__instance.Index, __instance.Owner, ActionParams, elementConf.EventTip);
            return false;
        }
        else
            Plugin.Logger.LogError($"Failed to patch ElementEntity.OnEvent, global trigger id {___mTriggerType} not found!");
        return true;

    }
}
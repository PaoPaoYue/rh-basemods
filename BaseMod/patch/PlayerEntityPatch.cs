using HarmonyLib;
using System.Reflection;
using System;
using Game;

namespace BaseMod;

class PlayerEntityPatch
{
    [HarmonyPatch(typeof(PlayerEntity), "Binding")]
    [HarmonyPostfix]
    static void BindingPostfix(PlayerEntity __instance, bool ___mBinding)
    {
        if (!___mBinding)
            return;
        var triggerType = Singleton<Model>.Instance.Player.GetRoleConf(__instance.RoleID).TriggerType;
        if (ModRegister.IsGlobalId(triggerType))
        {
            if (GlobalRegister.TryGetRegistered<PlayerTrigger>(triggerType, out var trigger))
            {
                if (ReflectionUtil.TryGetPrivateMethod(typeof(PlayerEntity), "OnEvent", out MethodInfo onEventMethod))
                    Singleton<GameEventManager>.Instance.Binding(trigger.EventId, (Action<EventArg>)onEventMethod.CreateDelegate(typeof(Action<EventArg>), __instance));
                else
                    Plugin.Logger.LogError("Failed to patch PlayerEntity.Binding, method OnEvent not found!");
            }
            else
                Plugin.Logger.LogError($"Failed to patch PlayerEntity.Binding, global trigger id {triggerType} not found!");

        }
    }

    [HarmonyPatch(typeof(PlayerEntity), "Unbinding")]
    [HarmonyPrefix]
    static void UnbindingPrefix(PlayerEntity __instance, bool ___mBinding)
    {
        if (!___mBinding || __instance.RoleID == 0)
            return;
        var triggerType = Singleton<Model>.Instance.Player.GetRoleConf(__instance.RoleID).TriggerType;
        if (ModRegister.IsGlobalId(triggerType))
        {
            if (GlobalRegister.TryGetRegistered<PlayerTrigger>(triggerType, out var trigger))
            {
                if (ReflectionUtil.TryGetPrivateMethod(typeof(PlayerEntity), "OnEvent", out MethodInfo onEventMethod))
                    Singleton<GameEventManager>.Instance.Unbinding(trigger.EventId, (Action<EventArg>)onEventMethod.CreateDelegate(typeof(Action<EventArg>), __instance));
                else
                    Plugin.Logger.LogError("Failed to patch PlayerEntity.Unbinding, method OnEvent not found!");
            }
            else
                Plugin.Logger.LogError($"Failed to patch PlayerEntity.Unbinding, global trigger id {triggerType} not found!");
        }
    }

    [HarmonyPatch(typeof(PlayerEntity), "OnEvent")]
    [HarmonyPrefix]
    static bool OnEventPrefix(PlayerEntity __instance, EventArg rEventArg)
    {
        var triggerType = __instance.Conf.TriggerType;
        if (!ModRegister.IsGlobalId(triggerType))
            return true;

        if (GlobalRegister.TryGetRegistered<PlayerTrigger>(triggerType, out var trigger))
        {
            var roleConf = Singleton<Model>.Instance.Player.GetRoleConf(__instance.RoleID);
            if (trigger.OnTrigger(__instance, roleConf, rEventArg))
                Singleton<BattleManager>.Instance.OrderManager.AddPlayerAction(roleConf, __instance.Owner);
            return false;
        }
        else
            Plugin.Logger.LogError($"Failed to patch PlayerEntity.OnEvent, global trigger id {triggerType} not found!");
        return true;

    }
}
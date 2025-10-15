using BepInEx;
using BepInEx.Logging;
using BaseMod;
using Game;
using cfg;
using System.Collections.Generic;
using UnityEngine;

namespace CustomActionMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BaseMod", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("CustomTriggerMod", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public static readonly string ModName = "CustomActionMod";

    internal static new ManualLogSource Logger;

    internal static ModRegister Register;

    private void Awake()
    {
        Logger = base.Logger;
        Register = ModRegister.Create(ModName, ["CustomTriggerMod"]);

        Register.RegisterEventAction(100001, new ActionPlayerAddAndElementAddAction());
    }

    public class ActionPlayerAddAndElementAddAction : EventActionBase
    {
        public override void ExcuteElement()
        {
            Vector3 lotteCellPosition = Singleton<Model>.Instance.Element.GetLotteCellPosition(base.Index, base.Owner);
            ElementEntity elementData = Singleton<Model>.Instance.Element.GetElementData(base.Index, base.Owner);

            int nPlayerAddType = base.ElementConf.OtherValue[0];
            if (ModRegister.IsValidModId(nPlayerAddType))
            {
                nPlayerAddType = Plugin.Register.GetEntityAttributeId(nPlayerAddType);
            }
            int nPlayerAddCount = base.ElementConf.TriggerValue[base.Level - 1].Value[0];
            string attributeIcon = Singleton<Model>.Instance.Buff.GetAttributeIcon(nPlayerAddType);
            if (nPlayerAddType == 1)
            {
                SingletonMono<AssetManager>.Instance.InstantiateParticle(9, null, lotteCellPosition, null);
            }

            int nElementAddType = base.ElementConf.OtherValue[1];
            if (ModRegister.IsValidModId(nElementAddType))
            {
                nPlayerAddType = Plugin.Register.GetEntityAttributeId(nElementAddType);
            }
            int nElementAddCount = base.ElementConf.TriggerValue[base.Level - 1].Value[1];
            elementData.ChangeAttribute(nElementAddType, nElementAddCount);

            int nOrderID = base.OrderID;
            EEntityType rOwner = base.Owner;
            UIAnimText.ShowFlyText(lotteCellPosition, Singleton<Model>.Instance.Buff.GetPlayerPosition(base.Owner), attributeIcon, StringUtil.AddNumberToString(nPlayerAddCount), delegate
            {
                Singleton<Model>.Instance.Buff.GetPlayerEntity(rOwner).ChangeAttribute(nPlayerAddType, nPlayerAddCount);
                Singleton<BattleManager>.Instance.OrderManager.OnBattleOrderExcuteEnd(nOrderID);
            });
        }
    }
}


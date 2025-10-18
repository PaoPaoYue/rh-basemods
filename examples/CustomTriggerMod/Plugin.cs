using BepInEx;
using BepInEx.Logging;
using BaseMod;
using Game;
using cfg;
using System.Collections.Generic;

namespace CustomTriggerMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BaseMod", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    public static readonly string ModName = "CustomTriggerMod";

    internal static new ManualLogSource Logger;

    internal static ModRegister Register;

    private void Awake()
    {
        Logger = base.Logger;
        Register = ModRegister.Create(ModName);

        Register.RegisterDescTip(100001, new DescTip(1000001, 1000002));
        var attrId = Register.RegisterVisableAttribute(100001, "hasDoneBattleCry");
        Register.RegisterElementTrigger(100001, new BattleCryTrigger(attrId));
    }

    public class BattleCryTrigger : ElementTrigger
    {

        private int attrId;
        public BattleCryTrigger(int attrId) : base(EventName.OnLoopElementChange)
        {
            this.attrId = attrId;
        }

        public override bool OnTrigger(Entity element, Element elementConf, EventArg rEventArg, out List<int> actionParams)
        {
            actionParams = null; // no special params this case, or you can pass some params from rEventArg if needed
            if (element.EntityType == EEntityType.Element)
            {
                var hasDoneBattleCry = element.GetAttribute(attrId);
                if (hasDoneBattleCry == 0)
                {
                    element.SetAttribute(attrId, 1, true); // Mark as done
                    return true;
                }
            }
            return false;
        }
    }
}


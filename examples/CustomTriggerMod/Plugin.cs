using BepInEx;
using BepInEx.Logging;
using BaseMod;
using Game;
using cfg;
using System.Collections.Generic;

namespace CustomTriggerMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static readonly string ModName = "CustomTriggerMod";

    internal static new ManualLogSource Logger;

    internal static ModRegister Register;

    private void Awake()
    {
        Logger = base.Logger;
        Register = ModRegister.Create(ModName);

        var attrId = Register.RegisterEntityAttribute((int)ModAttribute.HasDoneBattleCry);
        Register.RegisterElementTrigger(100001, new BattleCryTrigger(attrId));
    }

}

public enum ModAttribute
{
    HasDoneBattleCry = 100001
}

public class BattleCryTrigger : ElementTrigger
{

    private int mAttrId;
    public BattleCryTrigger(int attrId) : base(EventName.OnLoopElementChange)
    {
        mAttrId = attrId;
    }

    public override bool OnTrigger(Entity element, Element elementConf, EventArg rEventArg, out List<int> actionParams)
    {
        actionParams = null; // no special params this case, or you can pass some params from rEventArg if needed
        if (element.EntityType == EEntityType.Element)
        {
            var hasDoneBattleCry = element.GetAttribute(mAttrId);
            if (hasDoneBattleCry == 0)
            {
                element.SetAttribute(mAttrId, 1); // Mark as done
                return true;
            }
        }
        return false;
    }
}

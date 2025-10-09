using System.Collections.Generic;
using cfg;
using Game;

namespace BaseMod;

public interface IElementTrigger
{
    public bool OnTrigger(Entity element, Element elementConf, EventArg rEventArg, out List<int> actionParams);
}

public interface IRelicTrigger
{
    public bool OnTrigger(Entity relic, Relics relicConf, EventArg rEventArg, out List<int> actionParams);
}


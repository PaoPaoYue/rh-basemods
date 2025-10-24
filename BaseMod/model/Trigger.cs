using System.Collections.Generic;
using cfg;
using Game;

namespace BaseMod;

public abstract class PlayerTrigger
{
    public int EventId;
    public PlayerTrigger(int eventId)
    {
        EventId = eventId;
    }
    public abstract bool OnTrigger(Entity player, Role playerConf, EventArg rEventArg);
}

public abstract class ElementTrigger
{
    public int EventId;
    public ElementTrigger(int eventId)
    {
        EventId = eventId;
    }
    public abstract bool OnTrigger(Entity element, Element elementConf, EventArg rEventArg, out List<int> actionParams);
}

public abstract class RelicTrigger
{
    public int EventId;
    public RelicTrigger(int eventId)
    {
        EventId = eventId;
    }
    public abstract bool OnTrigger(Entity relic, Relics relicConf, EventArg rEventArg, out List<int> actionParams);
}


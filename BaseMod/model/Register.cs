using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cfg;
using Game;
using Cysharp.Threading.Tasks;

namespace BaseMod;

public class ModRegister
{

    public static int ModMinId = 100_000;
    public static int ModMaxId = 10_000_000 - 1;

    public static bool IsValidModId(int id)
    {
        return id >= ModMinId && id <= ModMaxId;
    }

    public static bool IsGlobalId(int id)
    {
        return id > ModMaxId;
    }

    public static ModRegister Create(string modName, List<string> superRegisters = null)
    {
        var superMods = superRegisters?
            .Select(n => GlobalRegister.GetModRegister(n))
            .Where(r => r != null)
            .ToList();

        if (superMods != null && superMods.Count != superRegisters.Count)
        {
            var missing = superRegisters
                .Where(n => GlobalRegister.GetModRegister(n) == null);
            foreach (var m in missing)
                Plugin.Logger.LogError($"Super ModRegister {m} not found for MOD {modName}!");
        }

        var modRegister = new ModRegister(modName, superMods);
        modRegister.ModIndex = GlobalRegister.AddModRegister(modName, modRegister);

        ResourceScanner.ScanAssemblyResourcesAsync(Assembly.GetCallingAssembly()).Forget();

        return modRegister;
    }

    public string ModName { get; private set; }

    internal int ModIndex { get; private set; }

    internal List<ModRegister> superRegisters = [];

    private Dictionary<int, IEventAction> eventActionDict = []; // id -> action
    private Dictionary<int, ElementTrigger> elementTriggerDict = []; // id -> (eventId, trigger)
    private Dictionary<int, RelicTrigger> relicTriggerDict = []; // id -> (eventId, trigger)
    private Dictionary<int, Localization> localizationDict = []; // id -> localization
    private Dictionary<int, DescTip> descTipDict = []; // id -> descTip

    private Dictionary<int, int> EventDict = []; // id -> eventId
    private int eventCounter = 1;
    private Dictionary<int, int> entityAttributeDict = []; // id -> attrId

    private ModRegister(string modName, List<ModRegister> superRegisters = null)
    {
        ModName = modName;
        if (superRegisters != null)
        {
            this.superRegisters = superRegisters;
        }
    }

    public void RegisterEventAction(int id, IEventAction action)
    {
        if (eventActionDict.ContainsKey(id))
        {
            throw new Exception($"Event action with id {id} already registered in mod {ModName}!");
        }
        eventActionDict[id] = action;
        GlobalRegister.AddRegistered(ConvertToGlobalId(id), action);
    }

    public void RegisterElementTrigger(int id, ElementTrigger trigger)
    {
        if (elementTriggerDict.ContainsKey(id))
        {
            throw new Exception($"Element trigger with id {id} already registered in mod {ModName}!");
        }
        elementTriggerDict[id] = trigger;
        GlobalRegister.AddRegistered(ConvertToGlobalId(id), trigger);
    }

    public void RegisterRelicTrigger(int id, RelicTrigger trigger)
    {
        if (relicTriggerDict.ContainsKey(id))
        {
            throw new Exception($"Relic trigger with id {id} already registered in mod {ModName}!");
        }
        relicTriggerDict[id] = trigger;
        GlobalRegister.AddRegistered(ConvertToGlobalId(id), trigger);
    }

    public int RegisterEvent(int id)
    {
        if (EventDict.ContainsKey(id))
        {
            throw new Exception($"Event with id {id} already registered in mod {ModName}!");
        }
        var eventId = eventCounter++;
        EventDict[id] = eventId;
        return eventId;
    }

    public int GetEventId(int id)
    {
        if (EventDict.ContainsKey(id))
        {
            return EventDict[id];
        }
        foreach (var super in superRegisters)
        {
            var eventId = super.GetEventId(id);
            if (eventId != -1)
            {
                return eventId;
            }
        }
        return -1;
    }

    public int RegisterLocalization(int id, string CN = null, string EN = null, string JP = null, string CNT = null)
    {
        if (localizationDict.ContainsKey(id))
        {
            throw new Exception($"Localization with id {id} already registered in mod {ModName}!");
        }
        var locId = ConvertToGlobalId(id);
        var loc = ReflectionUtil.CreateReadonly<Localization>(locId, CN, EN, JP, CNT);
        localizationDict[id] = loc;
        GlobalRegister.AddRegistered(locId, loc);
        return locId;
    }

    public int GetLocalizationId(int id)
    {
        if (localizationDict.ContainsKey(id))
        {
            return ConvertToGlobalId(id);
        }
        foreach (var super in superRegisters)
        {
            var locId = super.GetLocalizationId(id);
            if (locId != -1)
            {
                return locId;
            }
        }
        return -1;
    }

    public int RegisterEntityAttribute(int id)
    {
        if (entityAttributeDict.ContainsKey(id))
        {
            throw new Exception($"Entity attribute id {id} already registered in mod {ModName}!");
        }
        var attrId = ConvertToGlobalId(id);
        entityAttributeDict[id] = attrId;
        return attrId;
    }

    public int RegisterVisableAttribute(int id, string icon, int type = 0) // type: 0 int, 1 percent
    {
        if (entityAttributeDict.ContainsKey(id))
        {
            throw new Exception($"Entity attribute id {id} already registered in mod {ModName}!");
        }
        var attrId = ConvertToGlobalId(id);
        entityAttributeDict[id] = attrId;

        cfg.Attribute attr = ReflectionUtil.CreateReadonly<cfg.Attribute>(attrId, id, icon, type);

        GlobalRegister.AddRegistered(attrId, attr);
        return attrId;
    }

    public int GetEntityAttributeId(int id)
    {
        if (entityAttributeDict.ContainsKey(id))
        {
            return entityAttributeDict[id];
        }
        foreach (var super in superRegisters)
        {
            var attrId = super.GetEntityAttributeId(id);
            if (attrId != -1)
            {
                return attrId;
            }
        }
        return -1;
    }

    public void RegisterDescTip(int id, DescTip descTip)
    {
        if (descTipDict.ContainsKey(id))
        {
            throw new Exception($"DescTip with id {id} already registered in mod {ModName}!");
        }
        descTipDict[id] = descTip;
        GlobalRegister.AddRegistered(ConvertToGlobalId(id), descTip);
    }

    internal int ConvertToGlobalId(int id)
    {
        return (ModIndex + 1) * (ModMaxId + 1) + id;
    }

}

internal static class GlobalRegister
{

    private static Dictionary<string, ModRegister> modRegisters = new Dictionary<string, ModRegister>();

    private static Dictionary<(Type, int), object> globalRegisterDict = new Dictionary<(Type, int), object>(); // (type, globalId) -> object

    internal static ModRegister GetModRegister(string modName)
    {
        if (modRegisters.ContainsKey(modName))
        {
            return modRegisters[modName];
        }
        return null;
    }

    internal static int AddModRegister(string modName, ModRegister modRegister)
    {
        if (modRegisters.ContainsKey(modName))
        {
            throw new Exception($"Mod register with name {modName} already exists!");
        }
        modRegisters[modName] = modRegister;
        return modRegisters.Count - 1;
    }

    internal static void AddRegistered<T>(int globalId, T value)
    {
        var type = typeof(T);
        var key = (type, globalId);
        globalRegisterDict[key] = value;
    }

    internal static bool TryGetRegistered<T>(int globalId, out T value)
    {
        var key = (typeof(T), globalId);
        if (globalRegisterDict.TryGetValue(key, out var obj) && obj is T tValue)
        {
            value = tValue;
            return true;
        }
        value = default;
        return false;
    }

    internal static IEnumerable<(int id, T value)> EnumerateRegistered<T>()
    {
        var type = typeof(T);
        foreach (var kv in globalRegisterDict)
        {
            if (kv.Key.Item1 == type && kv.Value is T value)
                yield return (kv.Key.Item2, value);
        }
    }

    internal static bool TryGetGlobalId<T>(string modName, int id, out int globalId)
    {
        var modRegister = GetModRegister(modName);
        if (modRegister == null)
        {
            globalId = -1;
            return false;
        }
        return TryGetGlobalId<T>(modRegister, id, out globalId);
    }

    internal static bool TryGetGlobalId<T>(ModRegister modRegister, int id, out int globalId)
    {
        if (IsRegistered<T>(modRegister.ConvertToGlobalId(id)))
        {
            globalId = modRegister.ConvertToGlobalId(id);
            return true;
        }
        foreach (var superRegister in modRegister.superRegisters)
        {
            if (TryGetGlobalId<T>(superRegister, id, out globalId))
            {
                return true;
            }
        }

        globalId = -1;
        return false;
    }

    internal static bool IsRegistered<T>(int globalId)
    {
        return globalRegisterDict.ContainsKey((typeof(T), globalId));
    }


}
using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using ModDllPreloader;

namespace BaseMod;


public class ModRegister
{

    public static int ModMinId = 10_000;
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
        GlobalRegister.AddModRegister(modName, modRegister);
        return modRegister;
    }

    public string ModName { get; private set; }

    internal int ModIndex;
    internal List<ModRegister> superRegisters = new List<ModRegister>();

    private Dictionary<int, IEventAction> eventActionDict = new Dictionary<int, IEventAction>(); // id -> action
    private Dictionary<int, (int, IElementTrigger)> elementTriggerDict = new Dictionary<int, (int, IElementTrigger)>(); // id -> (eventId, trigger)
    private Dictionary<int, (int, IRelicTrigger)> relicTriggerDict = new Dictionary<int, (int, IRelicTrigger)>(); // id -> (eventId, trigger)
    private Dictionary<string, int> nameToEventDict = new Dictionary<string, int>(); // name -> eventId
    private int eventCounter = 1;
    private Dictionary<string, int> entityAttributeDict = new Dictionary<string, int>(); // name -> attrId
    private int entityAttributeCounter = 1;

    private ModRegister(string modName, List<ModRegister> superRegisters = null)
    {
        ModName = modName;
        ModIndex = Preloader.GetModIndex(modName);
        if (ModIndex == -1)
        {
            throw new Exception($"Mod {ModName} is not enabled or not found!");
        }
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
        GlobalRegister.AddRegistered(this, id, action);
    }

    public void RegisterElementTrigger(int id, int eventId, IElementTrigger trigger)
    {
        if (elementTriggerDict.ContainsKey(id))
        {
            throw new Exception($"Element trigger with id {id} already registered in mod {ModName}!");
        }
        var binding = new ValueTuple<int, IElementTrigger>(eventId, trigger);
        elementTriggerDict[id] = binding;
        GlobalRegister.AddRegistered(this, id, binding);
    }

    public void RegisterRelicTrigger(int id, int eventId, IRelicTrigger trigger)
    {
        if (relicTriggerDict.ContainsKey(id))
        {
            throw new Exception($"Relic trigger with id {id} already registered in mod {ModName}!");
        }
        var binding = new ValueTuple<int, IRelicTrigger>(eventId, trigger);
        relicTriggerDict[id] = binding;
        GlobalRegister.AddRegistered(this, id, binding);
    }

    public void RegisterEvent(string name)
    {
        if (nameToEventDict.ContainsKey(name))
        {
            throw new Exception($"Event name {name} already registered in mod {ModName}!");
        }
        var eventId = ConvertToGlobalId(eventCounter++);
        nameToEventDict[name] = eventId;
    }

    public int RegisterEntityAttribute(string name)
    {
        if (entityAttributeDict.ContainsKey(name))
        {
            throw new Exception($"Entity attribute name {name} already registered in mod {ModName}!");
        }
        var attrId = ConvertToGlobalId(entityAttributeCounter++);
        entityAttributeDict[name] = attrId;
        return attrId;
    }

    public IEventAction GetEventAction(int id)
    {
        if (eventActionDict.ContainsKey(id))
        {
            return eventActionDict[id];
        }
        foreach (var super in superRegisters)
        {
            var action = super.GetEventAction(id);
            if (action != null)
            {
                return action;
            }
        }
        return null;
    }

    public ValueTuple<int, IElementTrigger> GetElementTrigger(int id)
    {
        if (elementTriggerDict.ContainsKey(id))
        {
            return elementTriggerDict[id];
        }
        foreach (var super in superRegisters)
        {
            var binding = super.GetElementTrigger(id);
            if (binding != default)
            {
                return binding;
            }
        }
        return default;
    }

    public ValueTuple<int, IRelicTrigger> GetRelicTrigger(int id)
    {
        if (relicTriggerDict.ContainsKey(id))
        {
            return relicTriggerDict[id];
        }
        foreach (var super in superRegisters)
        {
            var binding = super.GetRelicTrigger(id);
            if (binding != default)
            {
                return binding;
            }
        }
        return default;
    }

    public int GetEventId(string name)
    {
        if (nameToEventDict.ContainsKey(name))
        {
            return nameToEventDict[name];
        }
        foreach (var super in superRegisters)
        {
            var eventId = super.GetEventId(name);
            if (eventId != -1)
            {
                return eventId;
            }
        }
        return -1;
    }

    public int GetEntityAttributeId(string name)
    {
        if (entityAttributeDict.ContainsKey(name))
        {
            return entityAttributeDict[name];
        }
        foreach (var super in superRegisters)
        {
            var attrId = super.GetEntityAttributeId(name);
            if (attrId != -1)
            {
                return attrId;
            }
        }
        return -1;
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

    internal static void AddModRegister(string modName, ModRegister modRegister)
    {
        if (modRegisters.ContainsKey(modName))
        {
            throw new Exception($"Mod register with name {modName} already exists!");
        }
        modRegisters[modName] = modRegister;
    }

    internal static void AddRegistered<T>(ModRegister modRegister, int id, T value)
    {
        var globalId = modRegister.ConvertToGlobalId(id);
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
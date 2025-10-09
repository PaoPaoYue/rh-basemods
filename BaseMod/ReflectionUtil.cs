using System;
using System.Collections.Concurrent;
using System.Reflection;

public static class ReflectionUtil
{
    private static readonly ConcurrentDictionary<(Type, string), FieldInfo> _fieldCache = new();
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> _propertyCache = new();
    private static readonly ConcurrentDictionary<(Type, string), MethodInfo> _methodCache = new();

    // ----------------------- 缓存基础方法 -----------------------
    private static bool TryGetCachedMember<T>(ConcurrentDictionary<(Type, string), T> cache, Type type, string name, out T member)
        where T : class
    {
        return cache.TryGetValue((type, name), out member);
    }

    private static T GetOrAddCachedMember<T>(ConcurrentDictionary<(Type, string), T> cache, Type type, string name, Func<Type, string, T> resolver)
        where T : class
    {
        return cache.GetOrAdd((type, name), key => resolver(key.Item1, key.Item2));
    }

    // ----------------------- Field -----------------------
    public static bool TryGetPrivateField<T>(object instance, string fieldName, out T value)
    {
        value = default;
        if (instance == null || string.IsNullOrEmpty(fieldName)) return false;

        var type = instance.GetType();
        if (!TryGetCachedMember(_fieldCache, type, fieldName, out var field))
        {
            field = GetOrAddCachedMember(_fieldCache, type, fieldName,
                (t, n) => t.GetField(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
        }

        if (field == null) return false;

        if (field.GetValue(instance) is T cast)
        {
            value = cast;
            return true;
        }
        return false;
    }

    public static bool TrySetPrivateField(object instance, string fieldName, object value)
    {
        if (instance == null || string.IsNullOrEmpty(fieldName)) return false;

        var type = instance.GetType();
        if (!TryGetCachedMember(_fieldCache, type, fieldName, out var field))
        {
            field = GetOrAddCachedMember(_fieldCache, type, fieldName,
                (t, n) => t.GetField(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
        }

        if (field == null) return false;

        field.SetValue(instance, value);
        return true;
    }

    public static bool TrySetReadonlyField(object instance, string fieldName, object value)
        => TrySetPrivateField(instance, fieldName, value);

    // ----------------------- Property -----------------------
    public static bool TryGetPrivateProperty<T>(object instance, string propertyName, out T value)
    {
        value = default;
        if (instance == null || string.IsNullOrEmpty(propertyName)) return false;

        var type = instance.GetType();
        if (!TryGetCachedMember(_propertyCache, type, propertyName, out var prop))
        {
            prop = GetOrAddCachedMember(_propertyCache, type, propertyName,
                (t, n) => t.GetProperty(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
        }

        if (prop == null) return false;

        if (prop.GetValue(instance) is T cast)
        {
            value = cast;
            return true;
        }
        return false;
    }

    public static bool TrySetPrivateProperty(object instance, string propertyName, object value)
    {
        if (instance == null || string.IsNullOrEmpty(propertyName)) return false;

        var type = instance.GetType();
        if (!TryGetCachedMember(_propertyCache, type, propertyName, out var prop))
        {
            prop = GetOrAddCachedMember(_propertyCache, type, propertyName,
                (t, n) => t.GetProperty(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
        }

        if (prop == null) return false;

        prop.SetValue(instance, value);
        return true;
    }

    // ----------------------- Method -----------------------
    public static bool TryGetPrivateMethod(Type type, string methodName, out MethodInfo method)
    {
        method = null;
        if (type == null || string.IsNullOrEmpty(methodName)) return false;

        if (!TryGetCachedMember(_methodCache, type, methodName, out method))
        {
            method = GetOrAddCachedMember(_methodCache, type, methodName,
                (t, n) => t.GetMethod(n, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
        }

        return method != null;
    }

    public static bool TryInvokePrivateMethod<T>(object instance, string methodName, out T result, params object[] parameters)
    {
        result = default;
        if (!TryGetPrivateMethod(instance.GetType(), methodName, out var method))
            return false;

        var raw = method.Invoke(instance, parameters);
        if (raw is T cast)
        {
            result = cast;
            return true;
        }
        return false;
    }

    public static bool TryInvokePrivateMethod(object instance, string methodName, params object[] parameters)
    {
        if (!TryGetPrivateMethod(instance.GetType(), methodName, out var method))
            return false;

        method.Invoke(instance, parameters);
        return true;
    }
}

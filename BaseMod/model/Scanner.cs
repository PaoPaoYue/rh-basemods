using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using BaseMod;

public static class ResourceScanner
{

    private static readonly Dictionary<string, Sprite> _assemblySpriteDict = new();
    private static readonly Dictionary<int, AudioClip> _assemblyAudioClipDict = new();

    public static IEnumerable<KeyValuePair<string, Sprite>> GetAllSprites()
    {
        foreach (var kv in _assemblySpriteDict)
            yield return kv;
    }

    public static IEnumerable<KeyValuePair<int, AudioClip>> GetAllAudioClips()
    {
        foreach (var kv in _assemblyAudioClipDict)
            yield return kv;
    }

    internal static async UniTask ScanAssemblyResourcesAsync(Assembly assembly)
    {
        var resourceNames = assembly.GetManifestResourceNames();
        var tasks = new List<UniTask>();

        foreach (var resName in resourceNames)
        {
            if (resName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                tasks.Add(LoadSpriteAsync(assembly, resName, (name, sprite) =>
                {
                    Plugin.Logger.LogDebug($"Loaded embedded sprite resource: {name}");
                    _assemblySpriteDict[name] = sprite;
                }));
            }
            else if (resName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                     resName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
                     resName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                tasks.Add(LoadAudioClipAsync(assembly, resName, (key, clip) =>
                {
                    Plugin.Logger.LogDebug($"Loaded embedded audio clip resource: {key}");
                    _assemblyAudioClipDict[key] = clip;
                }));
            }
        }

        await UniTask.WhenAll(tasks);
    }

    private static async UniTask LoadSpriteAsync(Assembly assembly, string resName, Action<string, Sprite> callback)
    {
        var fileName = Path.GetFileNameWithoutExtension(resName);
        fileName = fileName[(fileName.LastIndexOf('.') + 1)..];

        using var stream = assembly.GetManifestResourceStream(resName);
        if (stream == null) return;

        byte[] data = new byte[stream.Length];
        await stream.ReadAsync(data, 0, data.Length);

        var tex = new Texture2D(1, 1);
        tex.LoadImage(data);

        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), UnityEngine.Vector2.zero);

        callback?.Invoke(fileName, sprite);
    }
    
    private static async UniTask LoadAudioClipAsync(Assembly assembly, string resName, Action<int, AudioClip> callback)
    {
        var fileName = Path.GetFileNameWithoutExtension(resName);
        fileName = fileName[(fileName.LastIndexOf('.') + 1)..];
        if (!int.TryParse(fileName, out int key)) return;

        using var stream = assembly.GetManifestResourceStream(resName);
        if (stream == null) return;

        byte[] audioData = new byte[stream.Length];
        await stream.ReadAsync(audioData, 0, audioData.Length);

        var clip = await LoadAudioClipFromTempFileAsync(audioData, resName);
        if (clip != null)
            callback?.Invoke(key, clip);
    }

    private static async UniTask<AudioClip> LoadAudioClipFromTempFileAsync(byte[] data, string resName)
    {
        string tempPath = Path.Combine(Application.temporaryCachePath, resName);
        try
        {
            await File.WriteAllBytesAsync(tempPath, data);

            AudioType type = AudioType.UNKNOWN;
            if (resName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) type = AudioType.WAV;
            else if (resName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)) type = AudioType.OGGVORBIS;
            else if (resName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)) type = AudioType.MPEG;

            var request = UnityWebRequestMultimedia.GetAudioClip("file:///" + tempPath, type);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load audio {resName}: {request.error}");
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(request);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while loading audio {resName}: {ex.Message}");
            return null;
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}

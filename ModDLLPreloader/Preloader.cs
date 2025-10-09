using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModDllPreloader
{
    public static class Preloader
    {

        public static IEnumerable<string> TargetDLLs { get; } = [];

        public static void Patch(AssemblyDefinition assembly) { }

        public class ModInfo
        {
            public string ModName { get; private set; }
            public string Dir { get; private set; }
            public List<string> Dlls { get; private set; }

            public ModInfo(string modName, string dir, List<string> dlls)
            {
                ModName = modName;
                Dir = dir;
                Dlls = dlls ?? [];
            }
        }

        private static List<ModInfo> enabledMods = [];

        public static IReadOnlyList<ModInfo> GetAllEnabledMods()
        {
            return enabledMods.AsReadOnly();
        }

        public static ModInfo GetModInfo(string modName)
        {
            return enabledMods.FirstOrDefault(m => m.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase));
        }

        public static int GetModIndex(string modName)
        {
            return enabledMods.FindIndex(m => m.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase));
        }

        public static ModInfo GetModInfoByCallingDll()
        {
            string dllPath = Assembly.GetCallingAssembly().Location;
            return enabledMods.FirstOrDefault(m => m.Dlls.Contains(dllPath));
        }

        private class PreloaderConfig
        {
            public List<string> ScanPaths { get; set; } = [];
            public List<string> Include { get; set; } = [];
            public List<string> Exclude { get; set; } = [];
            public bool ImportIndividualDll { get; set; } = false;

            public void AddDefaultScanPath()
            {
                string defaultPath = Path.Combine(AppContext.BaseDirectory, "Mod", "ModDebug");
                if (!ScanPaths.Contains(defaultPath))
                {
                    ScanPaths.Add(defaultPath);
                }
            }
        }

        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ModDllPreloader");

        private static string BaseDir = AppContext.BaseDirectory;
        private static string ConfigPath = Path.Combine(BaseDir, "ModPreloaderConfig.json");
        private static string PluginDir = Path.Combine(BaseDir, "BepInEx", "plugins");

        public static void Initialize()
        {
            try
            {
                var config = LoadConfig();
                var mods = ScanMods(config);
                FilterAndDeploy(config, mods);
                SaveConfig(config);
            }
            catch (Exception ex)
            {
                Logger.LogError($"ERROR: {ex}");
            }
        }

        private static PreloaderConfig LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                Logger.LogInfo("ModPreloaderConfig.json not found, creating default...");
                var cfg = new PreloaderConfig();
                cfg.AddDefaultScanPath();
                return cfg;
            }

            try
            {
                string jsonContent = File.ReadAllText(ConfigPath);
                var config = JsonConvert.DeserializeObject<PreloaderConfig>(jsonContent);
                return config;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to parse config: {ex}");
                var cfg = new PreloaderConfig();
                cfg.AddDefaultScanPath();
                return cfg;
            }
        }

        private static void SaveConfig(PreloaderConfig cfg)
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(cfg, Formatting.Indented);
                File.WriteAllText(ConfigPath, jsonContent);
                Logger.LogInfo("Config saved successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to save config: {ex}");
            }
        }

        private static List<ModInfo> ScanMods(PreloaderConfig cfg)
        {
            var mods = new List<ModInfo>();
            var individualDlls = new List<ModInfo>();

            foreach (var scanPath in cfg.ScanPaths)
            {
                string full = Path.GetFullPath(scanPath);
                if (!Directory.Exists(full))
                {
                    Logger.LogInfo($"Scan path not found: {full}");
                    continue;
                }

                // 1. 扫描所有含有 moddata.json 的 mod 文件夹
                var modDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var moddataPath in Directory.EnumerateFiles(full, "moddata.json", SearchOption.AllDirectories))
                {
                    string modDir = Path.GetDirectoryName(moddataPath)!;
                    modDirs.Add(modDir);

                    try
                    {
                        var json = JObject.Parse(File.ReadAllText(moddataPath));
                        string modName = json.Value<string>("ModName");
                        if (string.IsNullOrWhiteSpace(modName))
                        {
                            Logger.LogWarning($"ModName missing in {moddataPath}, skipping...");
                            continue;
                        }

                        var dlls = Directory.EnumerateFiles(modDir, "*.dll", SearchOption.AllDirectories).ToList();
                        mods.Add(new ModInfo(modName, modDir, dlls));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error reading moddata.json in {modDir}: {ex.Message}");
                    }
                }

                Logger.LogInfo($"Found {mods.Count} MODs.");

                // 2. 若启用了导入独立 DLL 功能，则扫描未包含在 mod 目录内的 DLL
                if (cfg.ImportIndividualDll)
                {
                    foreach (var dllPath in Directory.EnumerateFiles(full, "*.dll", SearchOption.AllDirectories))
                    {
                        string dllDir = Path.GetDirectoryName(dllPath)!;

                        // 判断是否在某个 modDir 下
                        bool insideMod = modDirs.Any(modDir =>
                            dllDir.StartsWith(modDir, StringComparison.OrdinalIgnoreCase));

                        if (!insideMod)
                        {
                            string dllName = Path.GetFileNameWithoutExtension(dllPath);
                            individualDlls.Add(new ModInfo(dllName, string.Empty, [dllPath]));
                        }
                    }
                    mods.AddRange(individualDlls);
                    Logger.LogInfo($"Found {individualDlls.Count} individual DLLs.");
                }
            }

            return mods;
        }

        private static void FilterAndDeploy(PreloaderConfig cfg, List<ModInfo> mods)
        {

            // 更新全局 EnabledMods 列表
            if (cfg.Include.Count > 0 || cfg.Exclude.Count > 0)
                enabledMods = [.. mods.Where(m => cfg.Include.Contains(m.ModName) && !cfg.Exclude.Contains(m.ModName))];
            else
                enabledMods = [.. mods];
            Logger.LogInfo($"Scan complete. Start importing {enabledMods.Sum(m => m.Dlls.Count)} DLLs in {enabledMods.Count} MODs .");

            // 清空 plugins 目录
            if (Directory.Exists(PluginDir))
            {
                foreach (var file in Directory.GetFiles(PluginDir, "*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to clear old dll file {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(PluginDir);
            }

            // 拷贝被 include 的 mod 的 DLL
            var failedMods = new List<string>();
            foreach (var mod in enabledMods)
            {

                foreach (var dll in mod.Dlls)
                {
                    try
                    {
                        string target = Path.Combine(PluginDir, Path.GetFileName(dll));
                        File.Copy(dll, target, true);
                        Logger.LogInfo($"Imported: {mod.ModName}/{Path.GetFileName(dll)}");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Import failed: {mod.ModName}/{Path.GetFileName(dll)} ({ex.Message})");
                        failedMods.Add(mod.ModName);
                    }
                }
            }

            // 移除导入失败的 mod
            enabledMods.RemoveAll(m => failedMods.Contains(m.ModName));
            Logger.LogInfo($"Completed. Imported {enabledMods.Sum(m => m.Dlls.Count)} DLLs in {enabledMods.Count} MODs.");

        }
    }
}

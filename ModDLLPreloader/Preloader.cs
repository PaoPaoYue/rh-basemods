using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using BepInEx.Logging;

namespace ModDllPreloader
{
    public static class Preloader
    {

        public static IEnumerable<string> TargetDLLs { get; } = [];

        public static void Patch(AssemblyDefinition assembly) { }

        internal static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ModDllPreloader");

        private static string BaseDir = AppContext.BaseDirectory;
        private static string modDebugDir = Path.Combine(BaseDir, "Mod", "ModDebug");
        private static string PluginDir = Path.Combine(BaseDir, "BepInEx", "plugins");

        public static void Initialize()
        {
            try
            {
                Logger.LogInfo("Starting Mod DLL Preloader...");
                var dlls = ScanMods();
                Logger.LogInfo($"Scan complete. Start importing {dlls.Count} DLLs.");
                Deploy(dlls);
                Logger.LogInfo($"Completed. Imported {dlls.Count} DLLs.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"ERROR: {ex}");
            }
        }

        private static List<string> ScanMods()
        {
            var dlls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            List<string> scanPaths = [ modDebugDir ];
            scanPaths.AddRange(SteamUtil.GetSubscribedModPaths());

            foreach (var scanPath in scanPaths)
            {
                string full = Path.GetFullPath(scanPath);
                if (!Directory.Exists(full))
                {
                    Logger.LogInfo($"Scan path not found: {full}");
                    continue;
                }

                foreach (var moddataPath in Directory.EnumerateFiles(full, "moddata.json", SearchOption.AllDirectories))
                {
                    string modDir = Path.GetDirectoryName(moddataPath);
                    var modDlls = Directory.EnumerateFiles(modDir, "*.dll", SearchOption.AllDirectories).ToList();
                    foreach (var modDll in modDlls)
                    {
                        string dllName = Path.GetFileName(modDll);
                        if (dlls.ContainsKey(dllName))
                        {
                            Logger.LogWarning($"Duplicate DLL found: {dllName} in {modDir} (already found in {dlls[dllName]}), skipped.");
                        }
                        else
                        {
                            dlls[dllName] = modDll;
                        }
                    }
                }
            }

            return [.. dlls.Values];
        }

        private static void Deploy(List<string> dlls)
        {
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
            var failed = new List<string>();
            foreach (var dll in dlls)
            {
                try
                {
                    string target = Path.Combine(PluginDir, Path.GetFileName(dll));
                    File.Copy(dll, target, true);
                    Logger.LogInfo($"Imported: {Path.GetFileName(dll)}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Import failed: {Path.GetFileName(dll)} ({ex.Message})");
                    failed.Add(dll);
                }
            }

            dlls.RemoveAll(failed.Contains);

        }
    }
}

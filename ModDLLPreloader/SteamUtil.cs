#nullable enable
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModDllPreloader;

public static class SteamUtil
{

    public static List<string?> GetSubscribedModPaths()
    {
        if (!SteamAPI.Init())
        {
            Preloader.Logger.LogWarning("SteamAPI initialization failed. Are you running the game through Steam?");
            return [];
        }

        try
        {
            uint count = SteamUGC.GetNumSubscribedItems();
            if (count == 0)
                return [];

            var ids = new PublishedFileId_t[count];
            SteamUGC.GetSubscribedItems(ids, count);

            var paths = ids
                .Select(id => TryGetInstallPath(id))
                .Where(path => path != null)
                .ToList()!;

            return paths;
        }
        finally
        {
            SteamAPI.Shutdown();
        }

        static string? TryGetInstallPath(PublishedFileId_t id)
        {
            return SteamUGC.GetItemInstallInfo(id, out _, out string folder, 1024U, out _) ? folder : null;
        }
    }
}

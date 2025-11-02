using HarmonyLib;
using Game;
using Cysharp.Threading.Tasks;

namespace BaseMod;

class GameManagerPatch
{
    [HarmonyPatch(typeof(GameManager), "Initialize")]
    [HarmonyPrefix]
    static void GameManagerInitializePrefix()
    {
        ResourceScanner.Load().Forget();
    }
}

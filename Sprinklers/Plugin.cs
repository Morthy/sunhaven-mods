using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Wish;

namespace Sprinklers;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony _harmony = new(PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;
    
    private void Awake()
    {
        logger = Logger;
        _harmony.PatchAll();

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }
    
    
    [HarmonyPatch]
    private class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "Awake")]
        public static void MainMenuControllerAwake()
        {
            try
            {
                ItemHandler.CreateSprinklerItems();
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Placeable), "OnDisable")]
        public static bool PlaceableOnDisable(ref Placeable __instance)
        {
            try
            {
                if (!__instance.Player)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
            
            return true;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UseItem), "SetPlayer")]
        public static bool UseItemSetPlayer(ref UseItem __instance)
        {
            try
            {
                if (!__instance.gameObject.activeSelf)
                {
                    __instance.gameObject.SetActive(true);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }

            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using Wish;
using ZeroFormatter;

namespace Sprinklers;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("CustomItems", "0.2.2")]
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
        [HarmonyPatch(typeof(MainMenuController), "PlayGame", new Type[] {})]
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "UpdateDecorationsOvernight", new []{ typeof(DecorationUpdateType)})]
        public static void GameManagerUpdateDecorationsOvernight(DecorationUpdateType updateType)
        {
            if (updateType != DecorationUpdateType.Late1)
            {
                return;
            }
            
            
            foreach (var decoration in SingletonBehaviour<GameSave>.Instance.CurrentWorld.Decorations)
            {
                foreach (var keyValuePair in decoration.Value)
                {
                    DecorationPositionData decorationPositionData = keyValuePair.Value;
                    int id = keyValuePair.Value.id;

                    if (id is not (ItemHandler.SmallSprinklerId or ItemHandler.LargeSprinklerId or ItemHandler.NelvariSprinklerId or ItemHandler.WithergateSprinklerId))
                    {
                        continue;
                    }
                    
                    CustomSprinkler.SprinkleSprinkle(ref decorationPositionData);
                }
            }

        }
    }
}
using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wish;

namespace CustomItems
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

        public static UseItem useItem;
        

        private void Awake()
        {
            logger = this.Logger;
            try
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
                this.harmony.PatchAll();
            }
            catch (Exception e)
            {
                logger.LogError("{PluginInfo.PLUGIN_GUID} Awake failed: " + e);
            }
        }
        
        
        [HarmonyPatch]
        class Patches
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(UseItem), "SetPlayer")]
            public static bool UseItemSetPlayer(ref UseItem __instance)
            {
                if (!__instance.gameObject.activeSelf)
                {
                    __instance.gameObject.SetActive(true);
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Placeable), "OnDisable")]
            public static bool PlaceableOnDisable(ref Placeable __instance)
            {
                if (!__instance.Player)
                {
                    return false;
                }

                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MainMenuController), "Awake")]
            public static void MainMenuControllerAwake()
            {
                try
                {
                    CustomItems.AddItems();
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }
            }
            
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SaleManager), "Awake")]
            public static void SaleManagerAwake()
            {
                try
               {
                   CustomItems.AddItemsToShops();
               }
               catch (Exception e)
               {
                   logger.LogError(e);
               }
            }
        }
    }
}
using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using Wish;

namespace CustomItems
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

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
            [HarmonyPatch(typeof(Database), "GetCacheCapacity")]
            public static bool DatabaseGetCacheCapacity(ref int __result)
            {
                __result = 999999;
                return false;
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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MainMenuController), "Start")]
            public static void MainMenuControllerStart()
            {
                try
                {
                    CustomItems.AddItems();
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }

                //logger.LogInfo(Resources.FindObjectsOfTypeAll<RecipeList>().Aggregate("", (current, rl) => current + (rl.name + "\n")));
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

               //logger.LogInfo(SingletonBehaviour<SaleManager>.Instance.merchantTables.Aggregate("", (current, x) => current + (x.name + "\n")));
            }
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CraftingTable), "Awake")]
            public static void CraftingTableAwake(CraftingTable __instance)
            { 
                try
                {
                    CustomItems.AddItemsToRecipeList(__instance);
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }
            }
        }
    }
}
using System;
using System.Linq;
using System.Reflection;
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
                
                harmony.Patch(typeof(Database).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(
                    m => m.Name.Equals("GetDataInternal")).MakeGenericMethod(typeof(ItemData)), 
                    new HarmonyMethod(typeof(Patches).GetMethod("DatabaseGetData"))
                );
            }
            catch (Exception e)
            {
                logger.LogError("{PluginInfo.PLUGIN_GUID} Awake failed: " + e);
            }
        }
        
        
        [HarmonyPatch]
        class Patches
        {
            
            public static bool DatabaseGetData(int itemId, Action <ItemData>onItemLoaded, Action onItemLoadFailed = null)
            {
                try
                {
                    if (itemId == 0)
                    {
                        return true;
                    }

                    if (CustomItems.HasCustomItemWithID(itemId))
                    {
                        onItemLoaded?.Invoke(CustomItems.GetCustomItem(itemId));
                        return false;
                    }
                    
                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Database), "ValidID")]
            public static bool DatabaseValidID(int id, ref bool __result)
            {
                try
                {

                    if (CustomItems.HasCustomItemWithID(id))
                    {
                        __result = true;
                        return false;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Database), "GetID")]
            public static bool GetID(string name, ref int __result)
            {
                try
                {
                    var id = CustomItems.IDForCustomItemName(name);

                    if (id != null)
                    {
                        __result = (int)id;
                        return false;
                    }

                    return true;
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
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using UnityEngine;
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
                harmony.PatchAll();
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
            [HarmonyPatch(typeof(Player), "SetUseItem")]
            public static bool PlayerSetUseItem(ref ushort item, ref int index, ref bool fromLocal, ref Player __instance)
            {
                if (!CustomItems.HasCustomItemWithID(item))
                {
                    return true;
                }
                
                foreach (Component component in __instance.UseItemTransform)
                    Destroy(component.gameObject);

                __instance._useItem = null;

                var itemData = CustomItems.GetCustomItem(item);
                var useItem = itemData.useItem;

                if (useItem != null)
                {
                    var useItem2 = Instantiate<UseItem>(useItem, __instance.UseItemTransform.position, __instance.transform.rotation, __instance.UseItemTransform);
                    useItem2.SetPlayer(__instance);
                    useItem2.SetItemData(itemData);
                    __instance._useItem = useItem2;
                }
                
                __instance.onLoadedItem?.Invoke();
                __instance.onLoadedItem = null;
                
                __instance.Item = item;
                __instance.ItemIndex = index;
                __instance.ItemData = itemData;
                if (!fromLocal)
                    return false;
                
                __instance.onSetUseItem?.Invoke(item);

                return false;
            }

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
                    Database.GetData<ItemData>(ItemID.Chest, data =>
                    {
                        CustomFurniture.ExampleChest = data;
                        CustomItems.TryAddItems();
                    });
                    
                    Database.GetData<ItemData>(ItemID.CowPlushie, data =>
                    {
                        CustomFurniture.ExampleFurniture = data;
                        CustomItems.TryAddItems();
                    });
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
                   logger.LogInfo("SaleManager::Awake");
                   CustomItems.AddItemsToShops();
               }
               catch (Exception e)
               {
                   logger.LogError(e);
               }
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
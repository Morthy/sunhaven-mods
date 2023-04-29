using System;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Wish;

namespace ReStack
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

        public static int swap1 = -1;
        public static int swap2 = -1;

        private void Awake()
        {            
            logger = Logger;
            this.harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        [HarmonyPatch]
        class Patches
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PlayerInventory), "Update")]
            private static void InventoryUpdate(ref Inventory __instance)
            {
                if (swap1 != -1 && swap2 != -1)
                {
                    ItemIcon a;
                    ItemIcon b;
                    Player.Instance.Inventory.SwapItems(swap1, swap2, out a, out b);
                    swap1 = -1;
                    swap2 = -1;
                }
            }
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Inventory), "RemoveItemAt", new Type[] { typeof(int), typeof(int) } )]
            private static void RemoveItemAtPrefix(ref Inventory __instance, int slot, int amount, out int __state)
            {
                __state = 0;
                
                if (__instance.GetType() != typeof(PlayerInventory))
                {
                    return;
                }

                try
                {
                    if (slot < 10)
                    {
                        __state = __instance.Items[slot].id;
                    }
                }
                catch (Exception e)
                {
                    logger.LogInfo($"{PluginInfo.PLUGIN_GUID}: {e}");
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Inventory), "RemoveItemAt", new Type[] { typeof(int), typeof(int) } )]
            private static void RemoveItemAt(ref Inventory __instance, int slot, int amount, int __state)
            {
                if (__instance.GetType() != typeof(PlayerInventory))
                {
                    return;
                }
                
                try
                {
                    if (slot < 10 && __instance.Items[slot].id == 0 && __state != 0)
                    {
                        for (int index = Mathf.Min(__instance.maxSlots - 1, __instance.Items.Count - 1); index >= 10; --index)
                        {
                            var checkSlot = __instance.Items[index];

                            if (checkSlot.id == __state)
                            {
                                swap1 = slot;
                                swap2 = index;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogInfo($"{PluginInfo.PLUGIN_GUID}: {e}");
                }
            }
        }
        
    }

}


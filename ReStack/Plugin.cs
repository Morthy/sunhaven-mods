using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Wish;

namespace ReStack;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;

    public static int swap1 = -1;
    public static int swap2 = -1;
    public static int SwapTimer = 0;
        
    private void Awake()
    {            
        logger = Logger;
        this.harmony.PatchAll();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }

    private static void TryFindSwappableStack(Inventory __instance, int itemID, int oldSlot)
    {
        for (int index = Mathf.Min(__instance.maxSlots - 1, __instance.Items.Count - 1); index >= 10; --index)
        {
            var checkSlot = __instance.Items[index];

            if (checkSlot.id == itemID)
            {
                SwapTimer = 10;
                swap1 = oldSlot;
                swap2 = index;
                break;
            }
        }
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
                if (SwapTimer > 0)
                {
                    SwapTimer--;
                    return;
                }
                ItemIcon a;
                ItemIcon b;
                Player.Instance.Inventory.SwapItems(swap1, swap2, out a, out b);
                swap1 = -1;
                swap2 = -1;
            }
        }
            
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(Item), typeof(int), typeof(int))]
        private static void RemoveItemPrefix(ref Inventory __instance, out Dictionary<int, int> __state)
        {
            if (__instance.GetType() != typeof(PlayerInventory))
            {
                __state = null;
                return;
            }

            try
            {
                __state = new Dictionary<int, int>();

                for (int index = 0; index < 10; index++)
                {
                    if (__instance.Items[index].amount == 1)
                    {
                        __state[index] = __instance.Items[index].id;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError($"RemoveItemPostfix: {e}");
            }

            __state = null;
        }
            
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(Item), typeof(int), typeof(int))]
        private static void RemoveItemPostfix(ref Inventory __instance, ref Dictionary<int, int> __state)
        {
            if (__instance.GetType() != typeof(PlayerInventory) || __state == null)
            {
                return;
            }

            try
            {
                for (int index = 0; index < 10; index++)
                {
                    if (__instance.Items[index].amount == 0 && __state.TryGetValue(index, out var itemId))
                    {
                        TryFindSwappableStack(__instance, itemId, index);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError($"RemoveItemPostfix: {e}");
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
                logger.LogError($"RemoveItemAtPrefix: {e}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "RemoveItemAt", new Type[] { typeof(int), typeof(int) } )]
        private static void RemoveItemAt(ref Inventory __instance, int slot, int amount, int __state)
        {
            if (__instance.GetType() != typeof(PlayerInventory) || __state == 0)
            {
                return;
            }
                
            try
            {
                if (slot < 10 && __instance.Items[slot].id == 0 && __state != 0)
                {
                    TryFindSwappableStack(__instance, __state, slot);
                }
            }
            catch (Exception e)
            {
                logger.LogError($"RemoveItemAt: {e}");
            }
        }
    }
}
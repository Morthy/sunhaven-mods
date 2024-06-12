using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using UnityEngine;
using Wish;

namespace QuickMount
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

        private static ConfigEntry<string> HotKey;

        private void Awake()
        {
            logger = this.Logger;
            try
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
                HotKey = this.Config.Bind<string>("General", "Mount/Unmount key", KeyCode.LeftShift.ToString(), "Unity Keycode to open the menu");
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
            [HarmonyPatch(typeof(Wish.Player), "Update")]
            public static void PlayerUpdatePostfix(ref Wish.Player __instance)
            {
                if (!__instance.IsOwner || Cutscene.Active || __instance.Pathing || UIHandler.InventoryOpen || __instance.pause || __instance.Sleeping)
                {
                    return;
                }

                if (!Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), HotKey.Value))) return;
                
                var found = false;

                foreach (var slotItemData in __instance.Inventory.Items.Where(slotItemData => slotItemData.item != null && slotItemData.amount > 0))
                {
                    Database.GetData<ItemData>(slotItemData.item.ID(), (itemData) =>
                    {
                        if (!found && itemData.useItem && itemData.useItem.GetType() == typeof(MountWhistle))
                        {
                            found = true;
                                    
                            itemData.useItem.SetPlayer(Player.Instance);
                            itemData.useItem.UseDown1();
                                    
                        }
                    });

                    if (found)
                    {
                        break;
                    }
                }

                if (!found)
                {
                    NotificationStack.Instance.SendNotification("No mount whistle found in your inventory.");
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
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
            /*
            // ReInput.mapping.Actions
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ReInput.MappingHelper), "Actions", MethodType.Getter)]
            public static void ReInputMappingHelper(ref IList<InputAction> __result)
            {
                if (__result == null)
                {
                    return;
                }

                logger.LogInfo(__result);

                var action = new InputAction();
                typeof(InputAction).GetProperty("id").SetValue(action, 100);
                logger.LogInfo("yyy");
                typeof(InputAction).GetProperty("name").SetValue(action, "Mount/unmount");
                typeof(InputAction).GetProperty("type").SetValue(action, InputActionType.Button);
                typeof(InputAction).GetProperty("descriptiveName").SetValue(action, "Mount/unmount");
                logger.LogInfo("xxx");
                typeof(InputAction).GetProperty("positiveDescriptiveName").SetValue(action, "Mount/unmount");
                typeof(InputAction).GetProperty("negativeDescriptiveName").SetValue(action, "Mount/unmount");
                typeof(InputAction).GetProperty("behaviorId").SetValue(action, 0);
                typeof(InputAction).GetProperty("userAssignable").SetValue(action, true);
                typeof(InputAction).GetProperty("categoryId").SetValue(action, 1);

                var newCollection = new List<InputAction>(__result) {  };
                __result = newCollection;
            }
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Keybinds), "SetupKeybindButtons")]
            public static void KeybindsSetupKeybindButtons(ref Keybinds __instance)
            {
                typeof(Keybinds).GetMethod("SetupKeybindFromSettings", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { (Button)100, false });
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Keybinds), "GetButtonName")]
            public static bool KeybindsGetButtonName(ref Button button, ref string __result)
            {
                if ((int)button == 100)
                {
                    __result = "Mount/unmount";
                    return false;
                }

                return true;
            }
            */
            
            // GetButtonName(Button button)
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Wish.Player), "Update")]
            public static void PlayerUpdatePostfix(ref Wish.Player __instance)
            {
                if (!__instance.IsOwner || Cutscene.Active || __instance.Pathing || UIHandler.InventoryOpen || __instance.pause || __instance.Sleeping)
                {
                    return;
                }
                
                if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), HotKey.Value)))
                {
                    UseItem useItem = null;

                    foreach (var slotItemData in __instance.Inventory.Items)
                    {
                        if (slotItemData.item != null && slotItemData.amount > 0)
                        {
                            var itemData = ItemDatabase.GetItemData(slotItemData.item.ID());


                            if (itemData.useItem && itemData.useItem.GetType() == typeof(MountWhistle))
                            {
                                useItem = itemData.useItem;
                                break;
                            }
                        }
                    }

                    if (useItem)
                    {
                        useItem.SetPlayer(__instance);
                        useItem.UseDown1();
                    }
                    else
                    {
                        NotificationStack.Instance.SendNotification("No mount whistle found in your inventory.");
                    }
                }
            }
        }
    }
}
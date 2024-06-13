using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using UnityEngine;
using Wish;

namespace WhatItemsCanICraft
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;
        private static CraftingUI _craftingUI;
        private static RecipeList[] _recipeLists = { };
        private static ConfigEntry<string> _modifierKey;
        private static int _originalExternalUISiblingIndex;

        private void Awake()
        {            
            logger = Logger;
            
            _modifierKey = Config.Bind<string>("General", "Keyboard Button", "LeftControl", "Keyboard button that should trigger opening the popup when clicking on an item ");
                     
            this.harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        public static IEnumerator GetRecipesUsingItem(ItemData item, Action<List<(ItemData, string)>> onDone)
        {
            if (_recipeLists.Length == 0)
            {
                _recipeLists = Resources.FindObjectsOfTypeAll<RecipeList>();
            }
            
            var results = new List<(ItemData, string)>();
            var count = 0;
            
            foreach (var rl in _recipeLists)
            {
                foreach (var r in rl.craftingRecipes.Where(r => r.input2.Any(i => i.id == item.id)))
                {
                    count++;
                    Database.GetData<ItemData>(r.output2.id, data =>
                    {
                        results.Add((data, rl.name));
                    });
                    
                    
                }
            }

            while (count != results.Count)
            {
                yield return null;
            }

            results = results.OrderBy(i => i.Item1.name).ToList();
            onDone(results);
        }
        
        /*
        private static List<(ItemData, string)> GetRecipesUsingItem(ItemData item)
        {
            if (_recipeLists.Length == 0)
            {
                _recipeLists = Resources.FindObjectsOfTypeAll<RecipeList>();
            }

            var results = new List<(ItemData, string)>();
            
            foreach (var rl in _recipeLists)
            {
                foreach (var r in rl.craftingRecipes.Where(r => r.input2.Any(i => i.id == item.id)))
                {
                    results.Add((r.output2.id, rl.name));
                }
            }

            results = results.OrderBy(i => i.Item1.name).ToList();

            return results;
        }*/
        

        [HarmonyPatch]
        class Patches
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof (ItemIcon), "OnPointerDown")]
            private static bool ItemIconOnPointerDown(ItemIcon __instance)
            {
                try
                {
                    if (!Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode),_modifierKey.Value)))
                        return true;

                    Database.GetData<ItemData>(__instance.item.ID(), itemData =>
                    {
                        Player.Instance.StartCoroutine(GetRecipesUsingItem(itemData, craftables =>
                        {
                            if (craftables.Count > 0)
                            {
                                if (UIHandler.InventoryOpen)
                                {
                                    UIHandler.Instance.CloseExternalUI();
                                    UIHandler.Instance.CloseInventory(false);
                                }
                        
                                UIHandler.Instance.OpenUI(_craftingUI.gameObject, _craftingUI.gameObject.transform.parent);
                                
                                _craftingUI.ClearContent();
                                _craftingUI.Populate(craftables, itemData.name);
                            }
                            else
                            {
                                NotificationStack.Instance.SendNotification("<color=orange>Craftable Items</color>: You can't make anything using this!");
                            }
                        }));
                    });

                    return false;
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                    return true;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UIHandler), "Initialize")]
            private static void UIHandlerInitialize(UIHandler __instance)
            {
                if (_craftingUI)
                {
                    return;
                }
                
                _craftingUI = new GameObject("What can I craft?").AddComponent<CraftingUI>();
                _craftingUI.gameObject.SetActive(false);
                _craftingUI.transform.SetParent(UIHandler.Instance.transform);
                _craftingUI.transform.localPosition = Vector3.zero;
                _craftingUI.transform.localRotation = Quaternion.identity;
                _craftingUI.transform.localScale = new Vector3(1, 1, 1);
                _craftingUI.transform.SetAsLastSibling();
                _craftingUI.Create();
            }
        }
        
    }

}


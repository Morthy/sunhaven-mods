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
        private static List<RecipeList> _recipeLists = new();
        private static Dictionary<string, List<Recipe>> _recipes = new ();
        private static ConfigEntry<string> _modifierKey;

        // "Old" items: 9351 9352  9353 9350 9355
        // Lynn anvil: 9357
        private static int[] _craftingTables = {
            9356, 9359, 10684, 10689, 10703, 9354, 10317, 10465, 10464, 10537, 10574, 10687, 10686, 10685, 10690, 10683, 10688, 10691, 10702, 10704, 10705, 10707, 10700, 10708, 10701, 10706, 10709, 10711, 10710, 10721,
            10725, 10716, 10717, 10724, 10715, 10713, 10712, 10718, 10714, 10723, 10720, 10726, 10722, 10727, 10728, 10738, 10864, 10867, 10868, 10866, 10890, 10891, 10892, 10923
        };
        

        private void Awake()
        {            
            logger = Logger;
            
            _modifierKey = Config.Bind<string>("General", "Keyboard Button", "LeftControl", "Keyboard button that should trigger opening the popup when clicking on an item ");
                     
            this.harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        public static IEnumerator DetermineCraftingTables(Action<List<int>> onDone)
        {
            
            var count = SingletonBehaviour<ItemInfoDatabase>.Instance.allDecorations.Count;
            var ids = new List<int>();
            var done = 0;
            
            foreach (var id in SingletonBehaviour<ItemInfoDatabase>.Instance.allDecorations)
            {
                Database.GetData<ItemData>(id, data =>
                {
                    if ((object)data.useItem != null && data.useItem is Placeable { Decoration: CraftingTable })
                    {
                        ids.Add(data.id);
                        logger.LogInfo(data.name);
                    }

                    done++;
                });
            }

            while (count != done)
            {
                yield return null;
            }
            
            onDone(ids);
        }
        
        public static IEnumerator LoadAllRecipes(Action onDone)
        {
            if (_recipes.Count > 0)
            {
                onDone();
                yield break;
            }
            
            NotificationStack.Instance.SendNotification("Loading all recipes... (this may take a few seconds)");
            var done = 0;
            
            foreach (var id in  _craftingTables)
            {
                logger.LogDebug($"Crafting table {id}");
                Database.GetData<ItemData>(id, data =>
                {
                    var table = ((Placeable)data.useItem)._decoration as CraftingTable;
                    var recipes = table._craftingRecipes;
                    var name = data.name;

                    if (recipes.Count == 0 && table.recipeList)
                    {
                        recipes = table.recipeList.craftingRecipes;
                    }
                    
                    _recipes[name] = recipes;
                    
                    logger.LogDebug($"Added {recipes.Count} recipes for {name}");

                    done++;
                });
            }

            while (_craftingTables.Length != done)
            {
                yield return null;
            }

            _recipeLists = _recipeLists.GroupBy(r => r.name).Select(r => r.First()).ToList();
            
            onDone();
        }

        public static IEnumerator GetRecipesUsingItem(ItemData item, Action<List<(ItemData, string)>> onDone)
        {
            var results = new List<(ItemData, string)>();
            var count = 0;
            
            foreach (var recipeList in _recipes)
            {
                foreach (var r in recipeList.Value.Where(r => r.input2.Any(i => i.id == item.id)))
                {
                    count++;
                    Database.GetData<ItemData>(r.output2.id, data =>
                    {
                        results.Add((data, recipeList.Key));
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

                    /*
                    Player.Instance.StartCoroutine(DetermineCraftingTables((ids) =>
                    {
                        logger.LogInfo(String.Join(", ", ids.ToArray()));
                    }));

                    return false;
                    */

                    Player.Instance.StartCoroutine(LoadAllRecipes(() =>
                    {
                        Database.GetData<ItemData>(__instance.item.ID(), itemData =>
                        {
                            if (itemData.useItem && itemData.useItem.GetType() == typeof(FishingRod))
                            {
                                return;
                            }
                            
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
                    }));

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


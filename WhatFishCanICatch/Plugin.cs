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

namespace WhatFishCanICatch
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;
        private static FishUI _fishUI;
        private static ConfigEntry<string> _modifierKey;

        private void Awake()
        {
            logger = Logger;

            _modifierKey = Config.Bind<string>("General", "Mouse Button", "LeftControl", "Mouse button that should trigger opening the fishing info box");

            this.harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        public static Dictionary<FishData, float> GetFishChancesFromFishData(RandomFishArray array, float fishingLevel)
        {
            float totalProbability = 0;
            var dictionary = new Dictionary<FishData, float>();

            float num1 = 0.0f;
            float num2 = 0.0f;
            if (GameSave.Fishing.GetNode("Fishing6b"))
                num1 += 0.05f * GameSave.Fishing.GetNodeAmount("Fishing6b");
            if (GameSave.Fishing.GetNode("Fishing10b"))
                num2 += 0.1f * GameSave.Fishing.GetNodeAmount("Fishing10b");

            for (int index = 0; index < array.drops.Length; ++index)
            {
                FishLoot drop = array.drops[index];
                float num3 = 1f;
                float num4;
                switch (drop.fish.rarity)
                {
                    case ItemRarity.Epic:
                        num4 = num3 + num1;
                        break;
                    case ItemRarity.Legendary:
                        num4 = num3 + num1 + num2;
                        break;
                    default:
                        num4 = num3 + num1;
                        break;
                }

                var adjustedOdds = (float)typeof(RandomFishArray).GetMethod("AdjustedOddsBasedOnRarityAndLevel", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(array, new object[] { drop.fish.rarity, fishingLevel });
                var chance = drop.dropChance * num4 * adjustedOdds;
                totalProbability += chance;

                dictionary[drop.fish] = chance;
            }

            var adjustedDictionary = new Dictionary<FishData, float>();

            foreach (var item in dictionary)
            {
                adjustedDictionary[item.Key] = item.Value / totalProbability;
            }

            return adjustedDictionary;
        }

        private static void ShowFish()
        {
            if (_fishUI.gameObject.activeSelf)
            {
                return;
            }

            var closest = FindObjectsOfType<FishSpawner>().OrderBy(spawner => Vector2.Distance(spawner.transform.position, Player.Instance.transform.position)).FirstOrDefault();

            if (!closest)
            {
                SingletonBehaviour<NotificationStack>.Instance.SendNotification("No fishing locations found");
                return;
            }

            UIHandler.Instance.CloseExternalUI(_fishUI.gameObject);

            closest.GetFish(); // Force currentFishSeason to be set
            var fishList = closest.currentFishSeason;

            var data = GetFishChancesFromFishData(fishList, Player.Instance.FishingSkill);
            data = (from entry in data orderby entry.Value descending select entry).ToDictionary(i => i.Key, i => i.Value);

            UIHandler.Instance.OpenUI(_fishUI.gameObject, _fishUI.gameObject.transform.parent);

            _fishUI.ClearContent();
            _fishUI.Populate(data);
        }

        [HarmonyPatch]
        class Patches
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(UseItem), "Use2")]
            private static bool FishingRodUse2(UseItem __instance)
            {
                ShowFish();
                return false;
            }
            
            [HarmonyPrefix]
            [HarmonyBefore("WhatItemsCanICraft")]
            [HarmonyPatch(typeof(ItemIcon), "OnPointerDown")]
            private static bool ItemIconOnPointerDown(ItemIcon __instance)
            {
                try
                {
                    if (!Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), _modifierKey.Value)))
                        return true;

                    Database.GetData<ItemData>(__instance.item.ID(), itemData =>
                    {
                        if (itemData.useItem && itemData.useItem.GetType() == typeof(FishingRod))
                        {
                            ShowFish();
                        }
                    });
                    
                    return false;
                }
                catch (Exception e)
                {
                    logger.LogInfo($"{PluginInfo.PLUGIN_GUID}: {e}");
                    return true;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UIHandler), "Initialize")]
            private static void UIHandlerInitialize(UIHandler __instance)
            {
                if (_fishUI)
                {
                    return;
                }

                _fishUI = new GameObject("What can I catch").AddComponent<FishUI>();
                _fishUI.gameObject.SetActive(false);
                _fishUI.transform.SetParent(UIHandler.Instance.transform);
                _fishUI.transform.localPosition = Vector3.zero;
                _fishUI.transform.localRotation = Quaternion.identity;
                _fishUI.transform.localScale = new Vector3(1, 1, 1);
                _fishUI.transform.SetAsLastSibling();
                _fishUI.Create();
            }
        }
    }
}
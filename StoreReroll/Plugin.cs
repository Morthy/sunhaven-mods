using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wish;

namespace StoreReroll;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;

    private static ConfigEntry<int> CostGold;
    private static ConfigEntry<int> CostTickets;
    private static ConfigEntry<int> CostOrbs;

    private static List<SaleType> AcceptableTypes = new ()
    {
        SaleType.GeneralStoreSmall,
        SaleType.GeneralStoreMedium,
        SaleType.GeneralStoreLarge,
        SaleType.PetStore,
        SaleType.WithergatePetStore,
        SaleType.RomanceWagon,
        SaleType.MountStands,
        SaleType.WithergateSmall,
        SaleType.WithergateLarge,
        SaleType.NelvariElvenFurniture,
        SaleType.NelvariSmallItems,
        SaleType.Anne,
        SaleType.GeneralStoreWallpaper,
        SaleType.NelvariPetStoreIce,
        SaleType.NelvariPetStoreDesert,
        SaleType.NelvariPetStoreNormal,
        SaleType.NelvariPetStoreSwamp,
        SaleType.ClothingStoreMannequin0,
        SaleType.WithergateMedium,
        SaleType.GeneralStoreExtraLarge,
        SaleType.ClothingStoreMannequin1,
        SaleType.ClothingStoreMannequin2,
        SaleType.ClothingStoreMannequin3,
    };

    private static Dictionary<SaleType, float> RerollCostMultiplier = new()
    {
        { SaleType.GeneralStoreMedium, 1.5f },
        { SaleType.GeneralStoreLarge, 2f },
        { SaleType.GeneralStoreExtraLarge, 2.5f },
        { SaleType.WithergateMedium, 1.5f },
        { SaleType.WithergateLarge, 2.0f },
        { SaleType.NelvariElvenFurniture, 1.5f },
        { SaleType.Anne, 10f }
    };

    private static Dictionary<string, int> Rerolls = new ();
    private static Dictionary<(string Key, int Reroll), int> BuyCounts = new ();
    private static Dictionary<string, List<ShopLootAmount2>> History = new ();
    private static Dictionary<SaleType, bool> SkipPrompt = new();

    private void Awake()
    {
        logger = Logger;
        this.harmony.PatchAll();
            
        CostGold = Config.Bind<int>("General", "CostGold", 200, "Cost of rerolling in gold");
        CostOrbs = Config.Bind<int>("General", "CostOrbs", 10, "Cost of rerolling in tickets");
        CostTickets = Config.Bind<int>("General", "CostTickets", 10, "Cost of rerolling in orbs");
            
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }

    private static bool DidSkipPrompt(SaleType type)
    {
        if (type is SaleType.ClothingStoreMannequin1 or SaleType.ClothingStoreMannequin2 or SaleType.ClothingStoreMannequin3)
        {
            type = SaleType.ClothingStoreMannequin0;
        }
        
        if (SkipPrompt.TryGetValue(type, out bool result))
        {
            return result;
        }

        return false;
    }

    private static void SetSkipPrompt(SaleType type)
    {
        if (type is SaleType.ClothingStoreMannequin1 or SaleType.ClothingStoreMannequin2 or SaleType.ClothingStoreMannequin3)
        {
            type = SaleType.ClothingStoreMannequin0;
        }
        
        SkipPrompt[type] = true;
    }
    
    private static int GetRerollPrice(Entity saleStand)
    {
        var saleType = Traverse.Create(saleStand).Field("saleType").GetValue<SaleType>();
        if (!RerollCostMultiplier.TryGetValue(saleType, out var multiplier))
        {
            multiplier = 1.0f;
        }

        var goldType = Traverse.Create(saleStand).Field("goldType").GetValue<GoldType>();
        
        switch (goldType)
        {
            case GoldType.Tickets:
                return (int)Math.Round(CostTickets.Value * multiplier);
            case GoldType.Orbs:
                return (int)Math.Round(CostOrbs.Value * multiplier);
            default:
                return (int)Math.Round(CostGold.Value * multiplier);
        }
    }

    private static string GetFormattedRerollPrice(SaleStand saleStand)
    {
        var cost = GetRerollPrice(saleStand).FormatWithCommas();
            
        switch (saleStand.goldType)
        {
            case GoldType.Tickets:
                return $"{cost} <sprite=\"ticket_icon\" index=0>";
            case GoldType.Orbs:
                return $"{cost} <sprite=\"orb_icon\" index=0>";
            default:
                return $"{cost} <sprite=\"gold_icon\" index=0>";
        }
    }
    
    private static string GetFormattedRerollPrice(MannequinTable saleStand)
    {
        var cost = GetRerollPrice(saleStand).FormatWithCommas();
            
        switch (saleStand.goldType)
        {
            case GoldType.Tickets:
                return $"{cost} <sprite=\"ticket_icon\" index=0>";
            case GoldType.Orbs:
                return $"{cost} <sprite=\"orb_icon\" index=0>";
            default:
                return $"{cost} <sprite=\"gold_icon\" index=0>";
        }
    }

    private static string GetFormattedRerollPrice(Entity saleStand)
    {
        if (saleStand is SaleStand)
        {
            return GetFormattedRerollPrice((SaleStand)saleStand);
        }

        return GetFormattedRerollPrice((MannequinTable)saleStand);
    }
        
    private static int GetPrice(SaleStand saleStand, ShopLoot2 shopLoot)
    {
        switch (saleStand.goldType)
        {
            case GoldType.Tickets:
                return shopLoot.tickets;
            case GoldType.Orbs:
                return shopLoot.orbs;
            default:
                return shopLoot.Price;
        }
    }
    
    private static int GetPrice(MannequinTable saleStand, ShopLoot2 shopLoot)
    {
        switch (saleStand.goldType)
        {
            case GoldType.Tickets:
                return shopLoot.tickets;
            case GoldType.Orbs:
                return shopLoot.orbs;
            default:
                return shopLoot.Price;
        }
    }

    private static ShopLootAmount2 GenerateItem(List<ShopLootAmount2> history, SaleType saleType, int rerolls)
    {
        var previousItem = history[rerolls - 1];
        var i = 0;
        ShopLootAmount2 loot;
            
        // Prevent a reroll ever rolling the exact same item twice in a row
        do
        {
            loot = SingletonBehaviour<SaleManager>.Instance.GenerateRandomItems(saleType, ref history);
        } while (++i < 10 && loot.shopLoot.id == previousItem.shopLoot.id);

        return loot;
    }

    private static void SetItemFromReroll(SaleStand saleStand)
    {
        var key = GetKey(saleStand);
        var rerolls = RerollCount(saleStand);
        List<ShopLootAmount2> history = History[key];
        var t = Traverse.Create(saleStand);

        ShopLootAmount2 loot;
        
        if (history.Count < rerolls + 1)
        {
            loot = GenerateItem(history, t.Field("saleType").GetValue<SaleType>(), rerolls);
            history.Add(loot);
        }
        else
        {
            loot = history[rerolls];
        }

        if (BuyCounts.TryGetValue((key, rerolls), out int numBought))
        {
            loot.amount -= numBought;
        }

        var price = GetPrice(saleStand, loot.shopLoot);
        
        Database.GetData<ItemData>(loot.shopLoot.id, data =>
        {
            t.Field("shopItem").SetValue(loot);
            t.Field("price").SetValue(price);
            t.Field("itemData").SetValue(data);

            saleStand.GetType().GetMethod("SetVisualFromItem", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(saleStand, new object[]{ data, t.Field("useIcon").GetValue<bool>(), price });
        });
    }

    private static void SetItemFromReroll(MannequinTable saleStand)
    {
        var key = GetKey(saleStand);
        var rerolls = RerollCount(saleStand);
        List<ShopLootAmount2> history = History[key];
        var t = Traverse.Create(saleStand);

        ShopLootAmount2 loot;

        if (history.Count < rerolls + 1)
        {
            loot = GenerateItem(history, t.Field("saleType").GetValue<SaleType>(), rerolls);
            history.Add(loot);
        }
        else
        {
            loot = history[rerolls];
        }

        if (BuyCounts.TryGetValue((key, rerolls), out int numBought))
        {
            loot.amount -= numBought;
        }

        var price = GetPrice(saleStand, loot.shopLoot);

        Database.GetData<MannequinOutfit>(loot.shopLoot.id, data =>
        {
            t.Field("currentOutfit").SetValue(data);
            t.Field("price").SetValue(loot.shopLoot.Price);

            saleStand.GetType().GetMethod("InitializeOutfit", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(saleStand, new object[] { });

            saleStand.GetType().GetMethod("SetVisualFromItem", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(saleStand, new object[] { price });

        });
    }


    private static int RerollCount(Entity saleStand)
    {
        var key = GetKey(saleStand);
        if (Rerolls.TryGetValue(key, out int count))
        {
            return count;
        }

        Rerolls[key] = 0;
        return 0;
    }

    private static bool TakeMoney(GoldType type, int cost)
    {
        switch (type)
        {
            case GoldType.Gold:
                if (GameSave.Coins < cost)
                {
                    AudioManager.Instance.PlayAudioImmediate(SingletonBehaviour<Prefabs>.Instance.errorSound, 0.5f);
                    return false;
                }
                Player.Instance.AddMoneyAndRegisterSource(-cost, 60101, 1, MoneySource.Exploration, showNotification: true);
                break;
            case GoldType.Tickets:
                if (GameSave.Tickets < cost)
                {
                    AudioManager.Instance.PlayAudioImmediate(SingletonBehaviour<Prefabs>.Instance.errorSound, 0.5f);
                    return false;
                }
                Player.Instance.AddTicketsAndRegisterSource(-cost, 60002, 1, MoneySource.Exploration, showNotification: true);
                break;
            case GoldType.Orbs:
                if (GameSave.Orbs < cost)
                {
                    AudioManager.Instance.PlayAudioImmediate(SingletonBehaviour<Prefabs>.Instance.errorSound, 0.5f);
                    return false;
                }
                Player.Instance.AddOrbsAndRegisterSource(-cost, 60001, 1, MoneySource.Exploration, showNotification: true);
                break;
        }

        return true;
    }

    private static void DoReroll(SaleStand saleStand)
    {
        var key = GetKey(saleStand);
        var cost = GetRerollPrice(saleStand);

        if (!TakeMoney(saleStand.goldType, cost))
        {
            return;
        }

        Rerolls[key]++;
        SetItemFromReroll(saleStand);
        AudioManager.Instance.PlayAudioImmediate(SingletonBehaviour<Prefabs>.Instance.placeDecorationSound, 0.75f);
    }
    
    private static void DoReroll(MannequinTable saleStand)
    {
        var key = GetKey(saleStand);
        var cost = GetRerollPrice(saleStand);

        if (!TakeMoney(saleStand.goldType, cost))
        {
            return;
        }

        Rerolls[key]++;
        SetItemFromReroll(saleStand);
        AudioManager.Instance.PlayAudioImmediate(SingletonBehaviour<Prefabs>.Instance.placeDecorationSound, 0.75f);
    }

    private static void DoReroll(Entity saleStand)
    {
        if (saleStand is MannequinTable)
        {
            DoReroll((MannequinTable)saleStand);
        }
        else
        {
            DoReroll((SaleStand)saleStand);
        }
    }

    private static void PromptReroll(Entity saleStand)
    {
        // todo: this is slightly shit
        var saleType = Traverse.Create(saleStand).Field("saleType").GetValue<SaleType>();

        if (DidSkipPrompt(saleType))
        {
            DoReroll(saleStand);
            return;
        }
            
        var responses = new Dictionary<int, Response>()
        {
            {
                1,
                new Response()
                {
                    responseText = () => "Yes",
                    action = (() =>
                    {
                        DoReroll(saleStand);
                    })
                }
            },
            {
                2,
                new Response()
                {
                    responseText = () => "Yes, and don't ask again for this this price",
                    action = () =>
                    {
                        DoReroll(saleStand);
                        SetSkipPrompt(saleType);
                    }
                }
            },
            {
                3,
                new Response()
                {
                    responseText = () => "No",
                    action = null
                }
            },
        };

        var cost = GetFormattedRerollPrice(saleStand);
        
        DialogueController.Instance.SetDefaultBox();
        DialogueController.Instance.PushDialogue(new DialogueNode()
        {
            dialogueText = new List<string>() { $"Would you like to reroll this item for {cost}?" },
            responses = responses
        });
        
        MouseVisualManager.Instance.SetDefault();
        PlayerInput.RemoveDisableButton("reroll", Button.Use2);
    }

    private static string GetKey(Entity saleStand)
    {
        return SceneManager.GetActiveScene().buildIndex + "_" + saleStand.gameObject.name;
    }

    private static void RecordBoughtCount(Entity saleStand, int numBought)
    {
        if (numBought > 0)
        {
            var buyCountKey = (GetKey(saleStand), RerollCount(saleStand));

            if (!BuyCounts.ContainsKey(buyCountKey))
            {
                BuyCounts[buyCountKey] = numBought;
            }
            else
            {
                BuyCounts[buyCountKey] += numBought;
            }
        }
    }

    private static void UpdatePromptOnTarget(Entity saleStand, SaleType saleType)
    {
        if (!UIHandler.InventoryOpen && PlayerInput.AllowInput && !DialogueController.Instance.DialogueOnGoing && AcceptableTypes.Contains(saleType))
        {
            PlayerInput.AddDisableButton("reroll", Button.Use2);
            MouseVisualManager.Instance.SetUse();
            if (!Input.GetMouseButtonDown(1))
                return;

            PromptReroll(saleStand);
        }
        else
        {
            PlayerInput.RemoveDisableButton("reroll", Button.Use2);
            MouseVisualManager.Instance.SetDefault();
        }
    }
    

    [HarmonyPatch]
    private class PatchesSaleStand
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaleStand), "BuyItem")]
        public static void SaleStandBuyItem(ref SaleStand __instance, out int __state, ref ShopLootAmount ___shopItem)
        {
            __state = ___shopItem.amount;
        }
            
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaleStand), "BuyItem")]
        public static void SaleStandBuyItemPostfix(ref SaleStand __instance, ref int __state, ref ShopLootAmount ___shopItem)
        {
            try
            {
                RecordBoughtCount(__instance, __state - ___shopItem.amount);
            }
            catch (Exception e)
            {
                logger.LogError($"Error SaleStandBuyItemPostfix: {e}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaleStand), "Initialize")]
        public static bool SaleStandInitialize(ref SaleStand __instance, out bool __state)
        {
            try
            {
                var rerolls = RerollCount(__instance);

                if (rerolls == 0)
                {
                    __state = true;
                    return true;
                }
                
                SetItemFromReroll(__instance);
            }
            catch (Exception e)
            {
                logger.LogError($"Error SaleStandInitialize: {e}");
            }

            __state = false;
            return false;
        }
            
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaleStand), "Initialize")]
        public static void SaleStandInitializePostfix(ref SaleStand __instance, ref bool __state, ref ShopLootAmount2 ___shopItem)
        {
            try
            {
                if (__state)
                {
                    History[GetKey(__instance)] = new List<ShopLootAmount2>() { ___shopItem };
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error SaleStandInitializePostfix: {e}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaleStand), "Target")]
        public static bool SaleStandTarget(ref SaleStand __instance, ref SaleType ___saleType)
        {
            try
            {
                UpdatePromptOnTarget(__instance, ___saleType);
            }
            catch (Exception e)
            {
                logger.LogError($"Error SaleStandTarget: {e}");
            }
                
            return true;
        }
            
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaleStand), "UnTarget")]
        public static void SaleStandUnTarget(ref SaleStand __instance)
        {
            try
            {
                PlayerInput.RemoveDisableButton("reroll", Button.Use2);
                MouseVisualManager.Instance.SetDefault();
            }
            catch (Exception e)
            {
                logger.LogError($"Error SaleStandUnTarget: {e}");
            }
        }
        
    }

    [HarmonyPatch]
    private class PatchesMannequinTable
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MannequinTable), "BuyItem")]
        public static void MannequinTableBuyItemPostfix(ref MannequinTable __instance, ref int __state)
        {
            try
            {
                RecordBoughtCount(__instance, 1);
            }
            catch (Exception e)
            {
                logger.LogError($"Error MannequinTableBuyItemPostfix: {e}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MannequinTable), "Initialize")]
        public static bool MannequinTableInitialize(ref MannequinTable __instance, out bool __state)
        {
            try
            {
                var rerolls = RerollCount(__instance);

                if (rerolls == 0)
                {
                    __state = true;
                    return true;
                }
                
                SetItemFromReroll(__instance);
            }
            catch (Exception e)
            {
                logger.LogError($"Error MannequinTableInitialize: {e}");
            }

            __state = false;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MannequinTable), "Initialize")]
        public static void MannequinTableInitializePostfix(ref MannequinTable __instance, ref bool __state, ref SaleType ___saleType, ref int ___saleIndex)
        {
            try
            {
                if (__state)
                {
                    History[GetKey(__instance)] = new List<ShopLootAmount2>() { SingletonBehaviour<SaleManager>.Instance.GetShopItem(___saleType, ___saleIndex) };
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error MannequinTableInitializePostfix: {e}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MannequinTable), "Target")]
        public static bool MannequinTableTarget(ref MannequinTable __instance, ref SaleType ___saleType)
        {
            try
            {
                UpdatePromptOnTarget(__instance, ___saleType);
            }
            catch (Exception e)
            {
                logger.LogError($"Error MannequinTableTarget: {e}");
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MannequinTable), "UnTarget")]
        public static void MannequinTableUnTarget(ref MannequinTable __instance)
        {
            try
            {
                PlayerInput.RemoveDisableButton("reroll", Button.Use2);
                MouseVisualManager.Instance.SetDefault();
            }
            catch (Exception e)
            {
                logger.LogError($"Error MannequinTableUnTarget: {e}");
            }
        }
    }

    [HarmonyPatch]
    private class PatchesSaleManager
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaleManager), "ResetQuantityForAllShops")]
        public static void SaleManagerResetQuantityForAllShops()
        {
            try
            {
                Rerolls.Clear();
                BuyCounts.Clear();
                History.Clear();
            }
            catch (Exception e)
            {
                logger.LogError($"Error SaleManagerResetQuantityForAllShops: {e}");
            }
        }
    }
}
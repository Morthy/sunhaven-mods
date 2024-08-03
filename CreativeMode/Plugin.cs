using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using TMPro;
using UnityEngine;
using Wish;
using Tree = Wish.Tree;

namespace CreativeMode;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("CustomItems", "0.2.2")]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new (PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;
    private static ConfigEntry<bool> _unlimitedFarmAnimals;
    private static ConfigEntry<bool> _unlimitedFishingNets;
    public static ConfigEntry<int> CataloguePageSize;
    public static ConfigEntry<bool> ShowUnownedDLCItems;
    public static ConfigEntry<bool> RequirePurchasing;
    public static ConfigEntry<bool> ShowItemIDs;

    private static readonly List<int> DebugIgnoreItems = new()
    {
        ItemID.GeodeMuncher,
        ItemID.HouseOld,
        ItemID.Sprinkler,
        ItemID.CandyflossMachine,
        ItemID.PopcornMachine0,
        ItemID.OrchardJamMaker,
        ItemID.OrchardHoneyMaker,
        ItemID.FarmingSlotMaker,
        ItemID.HotChocolateMachine,
        ItemID.SnowConeMachine,
        ItemID.BabyTiger,
        ItemID.BabyDragon,
        ItemID.WitchMineRock1x1,
        ItemID.WitchMineRock2x2,
        ItemID.PumpkinMineRock1x1,
        ItemID.CandyMainMineRock1x1,
        ItemID.CandyYellowMineRock,
        ItemID.CandyBlueMineRock,
        ItemID.CandyWhite0MineRock,
        ItemID.CandyWhite1MineRock,
        ItemID.CandyCornMineRockSm,
        ItemID.CandyCornMineRockMed,
        ItemID.CandyCornMineRockLrg,
        ItemID.ArenaChest,
        ItemID.WaterWheel,
        ItemID.AnimalFeeder,
        ItemID.CropTotem,
        ItemID.BrinestoneDeepsRock1x1_Green,
        ItemID.BrinestoneDeepsRock2x2_Green,
        ItemID.BrinestoneDeepsRock1x1_Grey,
        ItemID.BrinestoneDeepsRock2x2_Grey,
        ItemID.BrinestoneDeepsRock2x2_Yellow,
        ItemID.BrinestoneDeepsRock1x1_Yellow,
        ItemID.KrustyCopperOreNode,
        ItemID.KrustyRockNode,
        ItemID.DizzyRockNode,
        ItemID.SandstoneOreNode,
        ItemID.LargeSandstoneOreNode,
    };

    public static ItemUI ItemUI;
    
    private void Awake()
    {
        logger = Logger;
        _harmony.PatchAll();
        _unlimitedFarmAnimals = Config.Bind("General", "Unlimited farm animals", true, "Allow unlimited farm animals on your farms");
        _unlimitedFishingNets = Config.Bind("General", "Unlimited fishing nets", true, "Allow unlimited fishing nets on your farms");
        CataloguePageSize = Config.Bind("General", "Catalogue page size", 100, "Number of items to show per page in the catalogue");
        ShowUnownedDLCItems = Config.Bind("General", "Show unowned DLC items", true, "Set to false to hide DLC items you cannot obtain");
        RequirePurchasing = Config.Bind("General", "Require purchasing", false, "Set to true to require purchasing items instead of getting them for free");
        ShowItemIDs = Config.Bind("General", "Show Item IDs", false, "Set to true to show item IDs");

        CustomItems.CustomItems.OnCustomItemsAdded += ItemHandler.SetupDecorationCatalogueItem;

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }

    private static string DebugGetItemIDName(int id)
    {
        foreach (var field in typeof(ItemID).GetFields())
        {
            if ((int)field.GetValue(null) == id)
            {
                return field.Name;
            }
        }

        return "unknown";
    }

    private static string DebugGetCategories(int id)
    {
        return DecorationCategorization.GetCategories().Where(a => a.Value.Contains(id)).Aggregate("", (current, a) => current + (a.Key + ","));
    }

    
    public static IEnumerator Debug()
    {
        var lines = new List<string>();
        var total = 0;
        var done = 0;
        var dbg = "";

        foreach (var x in ItemInfoDatabase.Instance.allItemSellInfos)
        {
            var itemId = x.Key;
            
            if (DebugIgnoreItems.Contains(itemId))
            {
                continue;
            }
            

            if (itemId == 10924)
            {
                continue;
            }
            
            total++;
            
            Database.GetData<ItemData>(itemId, i =>
            {
                var name = DebugGetItemIDName(i.id);

                if (!i.useItem)
                {
                    done++;
                    return;
                }
                
                var placeableType = i.useItem.GetType();

                var itemName = i.name;

                if (i.isDLCItem)
                {
                    itemName = itemName += " (DLC)";
                }
                
                if (i.useItem is Placeable)
                {
                    var decoration = Traverse.Create(i.useItem).Field("_decoration").GetValue<Decoration>();
                    if (Traverse.Create(i.useItem).Field("useAbleByPlayer").GetValue<bool>() == true)
                    {
                        var decorationType = decoration != null ? decoration.GetType().ToString() : "";

                        if (decoration is HungryMonster or Forageable or OneTimeChest or Tree or ForageTree or TreasureChest)
                        {
                            done++;
                            return;
                        }
                        
                        lines.Add($"{i.id},{itemName},ItemID.{name},{placeableType},{decorationType}," + DebugGetCategories(i.id));
                    }

                }
                else if (i.useItem is TilePlaceable or Seeds or AnimalSpawnItem or PetSpawnItem)
                {
                    lines.Add($"{i.id},{itemName},ItemID.{name},{placeableType},," + DebugGetCategories(i.id));
                }

                if (i is PetData)
                {
                    dbg += $"ItemID.{name},\n";
                }

                done++;
                
            }, () =>
            {
                done++;
            });
        }
        
        while (total != done)
        {
            logger.LogInfo($"{done}/{total}");
            yield return new WaitForSecondsRealtime(0.5f);
        }


        File.WriteAllText("C:\\Users\\morth\\Desktop\\items.csv", String.Join("\n", lines.ToArray()));
    }
    
    public static void StartDebug()
    {
        MainMenuController.Instance.StartCoroutine(Debug());
    }
    
    public static void DebugDumpDLCData()
    {
        var dbg = "";
        foreach (var x in FindObjectsOfType<DLCMerchants>())
        {
            var progress = x.progress;
            var shop = x.gameObject.GetComponent<Shop>();
            var items = shop.merchantTable;
            var itemIDs = (from item in items.startingItems2 select item.id).ToArray();
            dbg = itemIDs.Aggregate(dbg, (current, itemId) => current + $"{{ ItemID.{DebugGetItemIDName(itemId)}, \"{progress.progressID}\" }},\n");
        }
        
        logger.LogInfo(dbg);
    }
    
    [HarmonyPatch]
    private class Patches
    {

        // Creates the decoration catalogue item
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "Start")]
        public static void MainMenuControllerStart()
        {
            try
            {
                foreach (var itemId in new[] { ItemID.WithergateFurnace, ItemID.WithergateAnvil })
                {
                    Database.GetData<ItemData>(itemId, data =>
                    {
                        ((Placeable)data.useItem)._decoration.pickaxeable = true;
                    });
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
            
            //StartDebug();
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIHandler), "Initialize")]
        private static void UIHandlerInitialize()
        {
            if (ItemUI && ItemUI != null)
            {
                return;
            }

            ItemUI = new GameObject("Item UI").AddComponent<ItemUI>();
            ItemUI.gameObject.SetActive(false);
            ItemUI.transform.SetParent(UIHandler.Instance.transform);
            ItemUI.transform.SetAsLastSibling();
            ItemUI.Create();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "InitializeAsOwner")]
        public static void PlayerFarmQuestManagerSendInitialTips()
        {
            if (!Player.Instance.Inventory.HasEnough(ItemHandler.DecorationCatalogueId, 1))
            {
                NotificationStack.Instance.SendNotification("<color=orange>Creative Mode</color>: Visit Bernard's shop in the <b>Town Hall</b> to get your <i>Decoration Catalogue</i>.");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FarmAnimalHandler), "SetupAnimalCapacity")]
        public static void FarmAnimalHandlerSetupAnimalCapacity(ref TextMeshProUGUI ____animalCountTMP, ref int ___currentAnimalCount)
        {
            if (!_unlimitedFarmAnimals.Value)
            {
                return;
            }
            
            if (SceneSettingsManager.Instance.GetCurrentSceneSettings != null && SceneSettingsManager.Instance.GetCurrentSceneSettings.playerMap)
            {
                ____animalCountTMP.text = ___currentAnimalCount.ToString();
            }
        }        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FarmAnimalHandler), "CanPlaceNewFarmAnimal")]
        public static bool FarmAnimalHandlerSetupAnimalCapacity(ref bool __result)
        {
            if (!_unlimitedFarmAnimals.Value)
            {
                return true;
            }
            
            __result = true;
            return false;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FarmAnimalHandler), "SetupFishNetCapacity")]
        public static void FarmAnimalHandlerSetupFishNetCapacity(ref TextMeshProUGUI ____fishNetCountTMP, ref int ___currentFishingNetCount)
        {
            if (!_unlimitedFishingNets.Value)
            {
                return;
            }
            
            if (ScenePortalManager.ActiveSceneName.Equals("2playerfarm"))
            {
                ____fishNetCountTMP.text = ___currentFishingNetCount.ToString();
            }
        }        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FarmAnimalHandler), "CanPlaceNewFishNet")]
        public static bool FarmAnimalHandlerCanPlaceNewFishNet(ref bool __result)
        {
            if (!_unlimitedFishingNets.Value)
            {
                return true;
            }
            
            __result = true;
            return false;
        }
    }
}
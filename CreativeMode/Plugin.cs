using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Wish;

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
        return DecorationCategorization.Categories.Where(a => a.Value.Contains(id)).Aggregate("", (current, a) => current + (a.Key + ","));
    }

    /*
    private static void Debug()
    {
        var txt = "";
        var dbg = "";
            
        foreach (var i in ItemDatabase.itemDatas.Values)
        {
            if (i == null || i.useItem is null || DebugIgnoreItems.Contains(i.id))
            {
                continue;
            }

            var name = DebugGetItemIDName(i.id);
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
                        continue;
                    }
                    txt += $"{i.id},{itemName},ItemID.{name},{placeableType},{decorationType}," + DebugGetCategories(i.id) + "\n";
                }

            }
            else if (i.useItem is TilePlaceable or Seeds or AnimalSpawnItem or PetSpawnItem)
            {
                txt += $"{i.id},{itemName},ItemID.{name},{placeableType},," + DebugGetCategories(i.id) + "\n";
            }
            
            
            if (i is PetData)
            {
                dbg += $"ItemID.{name},\n";
            }
        }
        
        File.WriteAllText("C:\\Users\\morth\\Desktop\\items.csv", txt);
    }
    */
    
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
                ItemHandler.SetupDecorationCatalogueItem();
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
            //Debug();
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
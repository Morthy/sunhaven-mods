using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Wish;

namespace MuseumTracker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

        private static readonly Dictionary<int, string> BundleNames = new()
        {
            { ItemID.GoldenBundle, "Golden Bundle (Hall of Gems)" },
            { ItemID.Gembundle, "Gem Bundle (Hall of Gems)" },
            { ItemID.Manabundle, "Mana Bundle (Hall of Gems)" },
            { ItemID.BarsBundle, "Bars Bundle (Hall of Gems)" },
            { ItemID.NelvariMinesBundle, "Nel'vari Mines Bundle (Hall of Gems)" },
            { ItemID.WithergateMinesBundle, "Withergate Mines Bundle (Hall of Gems)" },
            { ItemID.WinterCropsBundle, "Winter Crops Bundle (Hall of Culture)"},
            { ItemID.FlowersBundle, "Flowers Bundle (Hall of Culture)"},
            { ItemID.SpringCropsBundle, "Spring Crops Bundle (Hall of Culture)"},
            { ItemID.FallCropsBundle, "Fall Crops Bundle (Hall of Culture)"},
            { ItemID.NelvariTempleBooksBundle, "Nel'vari Temple Bundle (Hall of Culture)"},
            { ItemID.SummerCropsBundle, "Summer Crops Bundle (Hall of Culture)"},
            { ItemID.ForagingBundle, "Foraging Bundle (Hall of Culture)"},
            { ItemID.CombatBundle, "Combat Bundle (Hall of Culture)"},
            { ItemID.AlchemyBundle, "Alchemy Bundle (Hall of Culture)"},
            { ItemID.ExplorationBundle, "Exploration Bundle (Hall of Culture)"},
            { ItemID.WithergateFarmingBundle, "Withergate Farming Bundle (Hall of Culture)"},
            { ItemID.NelvariFarmingBundle, "Nel'vari Farming Bundle (Hall of Culture)"},
            
            { ItemID.BundleAquariumSpring, "Spring Fish Tank (The Aquarium)"},
            { ItemID.BundleAquariumSummer, "Summer Fish Tank (The Aquarium)"},
            { ItemID.BundleAquariumFall, "Fall Fish Tank (The Aquarium)"},
            { ItemID.BundleAquariumWinter, "Winter Fish Tank (The Aquarium)"},
            { ItemID.BundleAquariumNelvari, "Nel'vari Fish Tank (The Aquarium)"},
            { ItemID.BundleAquariumWithergate, "Withergate Fish Tank (The Aquarium)"},
            { ItemID.BundleAquariumBigtank, "Large Tank (The Aquarium)"},
            { ItemID.FishingBundle, "Fishing Bundle (The Aquarium)"},
            
            { ItemID.DynusAltarMining0, "Mining Dynus Altar"},
            { ItemID.DynusAltarFishing0, "Fishing Dynus Altar"},
            { ItemID.DynusAltarForaging0, "Foraging Dynus Altar"},
            { ItemID.DynusAltarMiscenalleous0, "Cooking Dynus Altar"},
            { ItemID.DynusAltarGold0, "Gold Dynus Altar"},
            { ItemID.DynusAltarFarming0, "Farming Dynus Altar"},
            { ItemID.DynusAltarFruit0, "Fruit Dynus Altar"},
            { ItemID.DynusAltarRareitems, "Keepsake Dynus Altar"},
        };

        private static readonly int[] ScenesToCheck = { 315, 324, 314, 140 };

        private static readonly Dictionary<int, Dictionary<int, int>> Bundles = new()
        {

            {
                ItemID.GoldenBundle, new Dictionary<int, int>()
                {
                    { ItemID.GoldenMilk, 1 },
                    { ItemID.GoldenEgg, 1 },
                    { ItemID.GoldenWool, 1 },
                    { ItemID.GoldenPomegranate, 1 },
                    { ItemID.GoldenLog, 1 },
                    { ItemID.GoldenFeather, 1 },
                    { ItemID.GoldenSilk, 1 },
                    { ItemID.GoldenApple, 1 },
                    { ItemID.GoldenOrange, 1 },
                    { ItemID.GoldenStrawberry, 1 },
                    { ItemID.GoldenBlueberry, 1 },
                    { ItemID.GoldenPeach, 1 },
                    { ItemID.GoldenRaspberry, 1 },
                }
            },
            {
                ItemID.Gembundle, new Dictionary<int, int>()
                {
                    { ItemID.Sapphire, 1 },
                    { ItemID.Ruby, 1 },
                    { ItemID.Amethyst, 1 },
                    { ItemID.Diamond, 1 },
                    { ItemID.Havenite, 1 },
                    { ItemID.Dizzite, 1 },
                    { ItemID.BlackDiamond, 1 },
                }
            },
            {
                ItemID.Manabundle, new Dictionary<int, int>()
                {
                    { ItemID.ManaDrop, 20 },
                }
            },
            {
                ItemID.BarsBundle, new Dictionary<int, int>()
                {
                    { ItemID.CopperBar, 1 },
                    { ItemID.IronBar, 1 },
                    { ItemID.GoldBar, 1 },
                    { ItemID.AdamantBar, 1 },
                    { ItemID.MithrilBar, 1 },
                    { ItemID.SuniteBar, 1 },
                    { ItemID.ElvenSteelBar, 1 },
                    { ItemID.GloriteBar, 1 },
                }
            },
            {
                ItemID.NelvariMinesBundle, new Dictionary<int, int>()
                {
                    { ItemID.ManaShard, 5 },
                    { ItemID.SparklingDragonScale, 5 },
                    { ItemID.SharpDragonScale, 5 },
                    { ItemID.ToughDragonScale, 5 },
                }
            },
            {
                ItemID.WithergateMinesBundle, new Dictionary<int, int>()
                {
                    { ItemID.CandyCornPieces, 5 },
                    { ItemID.RockCandyGem, 5 },
                    { ItemID.JawbreakerGem, 5 },
                    { ItemID.HardButterscotchGem, 5 },
                }
            },
            {
                ItemID.WinterCropsBundle, new Dictionary<int, int>()
                {
                    { ItemID.TeaLeaves, 1 },
                    { ItemID.Turnip, 1 },
                    { ItemID.PurpleEggplant, 1 },
                    { ItemID.HeatFruit, 1 },
                    { ItemID.MarshmallowBean, 1 },
                    { ItemID.BrrNana, 1 },
                    { ItemID.StarFruit, 1 },
                    { ItemID.HexagonBerry, 1 },
                    { ItemID.SnowPea, 1 },
                    { ItemID.SnowBallCrop, 1 },
                    { ItemID.BlizzardBerry, 1 },
                    { ItemID.BalloonFruit, 1 },
                    { ItemID.PythagoreanBerry, 1 },
                    { ItemID.BlueMoonFruit, 1 },
                    { ItemID.CandyCane, 1 },
                }
            },
            {
                ItemID.FlowersBundle, new Dictionary<int, int>()
                {
                    { ItemID.HoneyFlower, 1 },
                    { ItemID.RedRose, 1 },
                    { ItemID.BlueRose, 1 },
                    { ItemID.Daisy, 1 },
                    { ItemID.Orchid, 1 },
                    { ItemID.Tulip, 1 },
                    { ItemID.Hibiscus, 1 },
                    { ItemID.Lavender, 1 },
                    { ItemID.Sunflower, 1 },
                    { ItemID.Lily, 1 },
                    { ItemID.Lotus, 1 },
                }
            },
            {
                ItemID.SpringCropsBundle, new Dictionary<int, int>()
                {
                    { ItemID.Grapes, 1 },
                    { ItemID.Wheat, 1 },
                    { ItemID.Tomato, 1 },
                    { ItemID.Corn, 1 },
                    { ItemID.Onion, 1 },
                    { ItemID.Potato, 1 },
                    { ItemID.Greenroot, 1 },
                    { ItemID.Carrot, 1 },
                    { ItemID.Kale, 1 },
                    { ItemID.Lettuce, 1 },
                    { ItemID.Cinnaberry, 1 },
                    { ItemID.Pepper, 1 },
                    { ItemID.Shimmeroot, 1 },
                }
            },
            {
                ItemID.FallCropsBundle, new Dictionary<int, int>()
                {
                    { ItemID.Garlic, 1 },
                    { ItemID.Yam, 1 },
                    { ItemID.SodaPopCrop, 1 },
                    { ItemID.FizzyFruit, 1 },
                    { ItemID.Cranberry, 1 },
                    { ItemID.Barley, 1 },
                    { ItemID.Pumpkin, 1 },
                    { ItemID.GhostPepper, 1 },
                    { ItemID.Butternut, 1 },
                }
            },
            {
                ItemID.NelvariTempleBooksBundle, new Dictionary<int, int>()
                {
                    { ItemID.OriginsoftheGrandTreeandNivaraBookI, 1 },
                    { ItemID.OriginsoftheGrandTreeandNivaraBookII, 1 },
                    { ItemID.OriginsoftheGrandTreeandNivaraBookIII, 1 },
                    { ItemID.OriginsoftheGrandTreeandNivaraBookIV, 1 },
                    { ItemID.OriginsoftheGrandTreeandNivaraBookV, 1 },
                    { ItemID.OriginsofSunHavenandEliosBookI, 1 },
                    { ItemID.OriginsofSunHavenandEliosBookII, 1 },
                    { ItemID.OriginsofSunHavenandEliosBookIII, 1 },
                    { ItemID.OriginsofSunHavenandEliosBookIV, 1 },
                    { ItemID.OriginsofSunHavenandEliosBookV, 1 },
                    { ItemID.OriginsofDynusandShadowsBookI, 1 },
                    { ItemID.OriginsofDynusandShadowsBookII, 1 },
                    { ItemID.OriginsofDynusandShadowsBookIII, 1 },
                    { ItemID.OriginsofDynusandShadowsBookIV, 1 },
                    { ItemID.OriginsofDynusandShadowsBookV, 1 },
                }
            },
            {
                ItemID.SummerCropsBundle, new Dictionary<int, int>()
                {
                    { ItemID.Armoranth, 1 },
                    { ItemID.GuavaBerry, 1 },
                    { ItemID.Beet, 1 },
                    { ItemID.Lemon, 1 },
                    { ItemID.Chocoberry, 1 },
                    { ItemID.Pineapple, 1 },
                    { ItemID.Pepper, 1 },
                    { ItemID.Melon, 1 },
                    { ItemID.Stormelon, 1 },
                    { ItemID.Durian, 1 },
                }
            },
            {
                ItemID.ForagingBundle, new Dictionary<int, int>()
                {
                    { ItemID.Log, 1 },
                    { ItemID.Apple, 1 },
                    { ItemID.Seaweed, 1 },
                    { ItemID.Blueberry, 1 },
                    { ItemID.Mushroom, 1 },
                    { ItemID.Orange, 1 },
                    { ItemID.Strawberry, 1 },
                    { ItemID.Berry, 1 },
                    { ItemID.Raspberry, 1 },
                    { ItemID.Peach, 1 },
                    { ItemID.SandDollar, 1 },
                    { ItemID.Starfish, 1 },
                }
            },
            {
                ItemID.CombatBundle, new Dictionary<int, int>()
                {
                    { ItemID.LeafieTrinket, 1 },
                    { ItemID.EliteLeafieTrinket, 1 },
                    { ItemID.CentipillarTrinket, 1 },
                    { ItemID.PeppinchGreenTrinket, 1 },
                    { ItemID.ScorpepperTrinket, 1 },
                    { ItemID.EliteScorpepperTrinket, 1 },
                    { ItemID.HatCrabTrinket, 1 },
                    { ItemID.FloatyCrabTrinket, 1 },
                    { ItemID.BucketCrabTrinket, 1 },
                    { ItemID.UmbrellaCrabTrinket, 1 },
                    { ItemID.ChimchuckTrinket, 1 },
                    { ItemID.AncientSunHavenSword, 1 },
                    { ItemID.AncientNelVarianSword, 1 },
                    { ItemID.AncientWithergateSword, 1 },

                }
            },
            {
                ItemID.AlchemyBundle, new Dictionary<int, int>()
                {
                    { ItemID.ManaPotion, 1 },
                    { ItemID.HealthPotion, 1 },
                    { ItemID.AttackPotion, 1 },
                    { ItemID.SpeedPotion, 1 },
                    { ItemID.DefensePotion, 1 },
                    { ItemID.AdvancedAttackPotion, 1 },
                    { ItemID.AdvancedDefensePotion, 1 },
                    { ItemID.AdvancedSpellDamagePotion, 1 },
                    { ItemID.IncredibleSpellDamagePotion, 1 },
                    { ItemID.IncredibleAttackPotion, 1 },
                    { ItemID.IncredibleDefensePotion, 1 },
                }
            },
            {
                ItemID.ExplorationBundle, new Dictionary<int, int>()
                {
                    { ItemID.PetrifiedLog, 1 },
                    { ItemID.PhoenixFeather, 1 },
                    { ItemID.FairyWings, 1 },
                    { ItemID.GriffonEgg, 1 },
                    { ItemID.ManaSap, 1 },
                    { ItemID.PumiceStone, 1 },
                    { ItemID.MysteriousAntler, 1 },
                    { ItemID.DragonFang, 1 },
                    { ItemID.MonsterCandy, 1 },
                    { ItemID.UnicornHairTuft, 1 },
                }
            },
            {
                ItemID.WithergateFarmingBundle, new Dictionary<int, int>()
                {

                    { ItemID.KrakenKale, 1 },
                    { ItemID.Tombmelon, 1 },
                    { ItemID.Suckerstem, 1 },
                    { ItemID.Razorstalk, 1 },
                    { ItemID.SnappyPlant, 1 },
                    { ItemID.Moonplant, 1 },
                    { ItemID.Eggplant, 1 },
                    { ItemID.DemonOrb, 1 },
                }
            },
            {
                ItemID.NelvariFarmingBundle, new Dictionary<int, int>()
                {
                    { ItemID.Acorn, 1 },
                    { ItemID.RockFruit, 1 },
                    { ItemID.WaterFruit, 1 },
                    { ItemID.FireFruit, 1 },
                    { ItemID.WalkChoy, 1 },
                    { ItemID.WindChime, 1 },
                    { ItemID.ShiiwalkiMushroom, 1 },
                    { ItemID.DragonFruit, 1 },
                    { ItemID.ManaGem, 1 },
                    { ItemID.CatTail, 1 },
                    { ItemID.Indiglow, 1 },
                }
            },
            {
                ItemID.BundleAquariumSpring, new Dictionary<int, int>()
                {
                    { ItemID.Butterflyfish, 1 },
                    { ItemID.Sunfish, 1 },
                    { ItemID.FlowerFlounder, 1 },
                    { ItemID.RaincloudRay, 1 },
                    { ItemID.FloralTrout, 1 },
                    { ItemID.NeonTetra, 1 },
                    { ItemID.Seahorse, 1 },
                    { ItemID.PaintedEgg, 1 },
                    { ItemID.Tadpole, 1 },
                }
            },
            {
                ItemID.BundleAquariumSummer, new Dictionary<int, int>()
                {
                    { ItemID.Blazeel, 1 },
                    { ItemID.HearthAngler, 1 },
                    { ItemID.ScorchingSquid, 1 },
                    { ItemID.MagmaStar, 1 },
                    { ItemID.TinderTurtle, 1 },
                    { ItemID.Pyrelus, 1 },
                    { ItemID.FlameRay, 1 },
                    { ItemID.MoltenSlug, 1 },
                    { ItemID.Searback, 1 },
                }
            },
            {
                ItemID.BundleAquariumFall, new Dictionary<int, int>()
                {
                    { ItemID.Coducopia, 1 },
                    { ItemID.KingSalmon, 1 },
                    { ItemID.Hayfish, 1 },
                    { ItemID.AcornAnchovy, 1 },
                    { ItemID.VampirePiranha, 1 },
                    { ItemID.Ghostfish, 1 },
                    { ItemID.PumpkinJelly, 1 },
                    { ItemID.PiratePerch, 1 },
                    { ItemID.AutumnLeafSole, 1 },
                }
            },
            {
                ItemID.BundleAquariumWinter, new Dictionary<int, int>()
                {
                    { ItemID.Frostfin, 1 },
                    { ItemID.ChristmasLightfish, 1 },
                    { ItemID.HollyCarp, 1 },
                    { ItemID.JingleBass, 1 },
                    { ItemID.FrozenTuna, 1 },
                    { ItemID.Scarffish, 1 },
                    { ItemID.Heatfin, 1 },
                    { ItemID.IcicleCarp, 1 },
                    { ItemID.BlazingHerring, 1 },
                }
            },
            {
                ItemID.BundleAquariumNelvari, new Dictionary<int, int>()
                {
                    { ItemID.RobedParrotfish, 1 },
                    { ItemID.Axolotl, 1 },
                    { ItemID.FrilledBetta, 1 },
                    { ItemID.Horsefish, 1 },
                    { ItemID.Flamefish, 1 },
                    { ItemID.DragonGulper, 1 },
                    { ItemID.NeapolitanFish, 1 },
                    { ItemID.Snobfish, 1 },
                    { ItemID.KelpEel, 1 },
                    { ItemID.PrincelyFrog, 1 },
                    { ItemID.Angelfin, 1 },
                    { ItemID.Bubblefish, 1 },
                    { ItemID.CrystalTetra, 1 },
                    { ItemID.SkyRay, 1 },
                }
            },
            {
                ItemID.BundleAquariumWithergate, new Dictionary<int, int>()
                {
                    { ItemID.Kraken, 1 },
                    { ItemID.WaterBear, 1 },
                    { ItemID.BonemouthBass, 1 },
                    { ItemID.MummyTrout, 1 },
                    { ItemID.DeadeyeShrimp, 1 },
                    { ItemID.ElectricEel, 1 },
                    { ItemID.BrainJelly, 1 },
                    { ItemID.RedfinnedPincher, 1 },
                    { ItemID.SeaBat, 1 },
                    { ItemID.GhostheadTuna, 1 },
                    { ItemID.Globfish, 1 },
                    { ItemID.LivingJelly, 1 },
                    { ItemID.Purrmaid, 1 },
                    { ItemID.SlimeLeech, 1 },
                    { ItemID.GoblinShark, 1 },
                    { ItemID.Moonfish, 1 },
                    { ItemID.ToothyAngler, 1 },
                    { ItemID.VampireSquid, 1 },
                    { ItemID.Viperfish, 1 },
                    { ItemID.AlbinoSquid, 1 },
                    { ItemID.Devilfin, 1 },
                    { ItemID.ShadowTuna, 1 },
                }
            },
            {
                ItemID.BundleAquariumBigtank, new Dictionary<int, int>()
                {
                    { ItemID.PygmyTuna, 1 },
                    { ItemID.Catfish, 1 },
                    { ItemID.GoldFish, 1 },
                    { ItemID.StreamlineCod, 1 },
                    { ItemID.Salmon, 1 },
                    { ItemID.ClownFish, 1 },
                    { ItemID.BlackBass, 1 },
                    { ItemID.RainbowTrout, 1 },
                    { ItemID.PopeyeGoldfish, 1 },
                    { ItemID.Pufferfish, 1 },
                    { ItemID.IronheadSturgeon, 1 },
                    { ItemID.Cuddlefish, 1 },
                    { ItemID.Lobster, 1 },
                    { ItemID.SilverCarp, 1 },
                    { ItemID.Tuna, 1 },
                    { ItemID.BluntedSwordfish, 1 },
                    { ItemID.RibbonEel, 1 },
                    { ItemID.TigerTrout, 1 },
                    { ItemID.Eel, 1 },
                    { ItemID.RedSnapper, 1 },
                    { ItemID.Carp, 1 },
                    { ItemID.RedeyePiranha, 1 },
                    { ItemID.AngelFish, 1 },
                    { ItemID.WhitebellyShark, 1 },
                    { ItemID.KoiFish, 1 },
                    { ItemID.SandstoneFish, 1 },
                }
            },
            {
                ItemID.FishingBundle, new Dictionary<int, int>()
                {
                    { ItemID.HandmadeBobber, 1 },
                    { ItemID.AncientMagicStaff, 1 },
                    { ItemID.BronzeDragonRelic, 1 },
                    { ItemID.OldSwordHilt, 1 },
                    { ItemID.NelVarianRunestone, 1 },
                    { ItemID.AncientElvenHeaddress, 1 },
                    { ItemID.OldMayoralPainting, 1 },
                    { ItemID.TentacleMonsterEmblem, 1 },
                    { ItemID.AncientAngelQuill, 1 },
                    { ItemID.AncientNagaCrook, 1 },
                    { ItemID.AncientAmariTotem, 1 },
                }
            },
            {
                ItemID.DynusAltarRareitems, new Dictionary<int, int>()
                {
                    { ItemID.AdventureKeepsake, 1 },
                    { ItemID.RichesKeepsake, 1 },
                    { ItemID.RomanceKeepsake, 1 },
                    { ItemID.PeaceKeepsake, 1 },
                }
            },
            {
                ItemID.DynusAltarFruit0, new Dictionary<int, int>()
                {
                    { ItemID.Raspberry, 20 },
                    { ItemID.Peach, 10 },
                    { ItemID.Orange, 20 },
                    { ItemID.Blueberry, 20 },
                    { ItemID.Berry, 10 },
                    { ItemID.Apple, 20 },
                }
            },
            {
                ItemID.DynusAltarForaging0, new Dictionary<int, int>()
                {
                    { ItemID.Log, 300 },
                    { ItemID.FireCrystal, 30 },
                    { ItemID.EarthCrystal, 30 },
                    { ItemID.WaterCrystal, 30 },
                    { ItemID.SandDollar, 10 },
                }
            },
            {
                ItemID.DynusAltarGold0, new Dictionary<int, int>()
                {
                    { ItemID.GoldBar, 10 },
                }
            },
            {
                ItemID.DynusAltarMiscenalleous0, new Dictionary<int, int>() // cooking ??
                {
                    { ItemID.Cheesecake, 1 },
                    { ItemID.SpicyRamen, 1 },
                    { ItemID.SesameRiceBall, 1 },
                    { ItemID.Pizza, 1 },
                    { ItemID.Cookies, 1 },
                    { ItemID.Coffee, 1 },
                    { ItemID.TomatoSoup, 1 },
                    { ItemID.ShimmerootTreat, 1 },
                    { ItemID.EnergySmoothie, 1 },
                }
            },
            {
                ItemID.DynusAltarMining0, new Dictionary<int, int>()
                {
                    { ItemID.Stone, 999 },
                    { ItemID.Coal, 50 },
                    { ItemID.CopperOre, 100 },
                    { ItemID.Sapphire, 10 },
                    { ItemID.Ruby, 10 },
                    { ItemID.Amethyst, 10 },
                    { ItemID.Diamond, 5 },
                    { ItemID.Havenite, 5 },
                }
            },
            {
                ItemID.DynusAltarFarming0, new Dictionary<int, int>()
                {
                    { ItemID.Wheat, 30 },
                    { ItemID.Corn, 30 },
                    { ItemID.Potato, 30 },
                    { ItemID.Tomato, 30 },
                    { ItemID.Carrot, 30 },
                    { ItemID.SugarCane, 30 },
                    { ItemID.Onion, 30 },
                    { ItemID.Greenroot, 30 },
                    { ItemID.HoneyFlower, 30 },
                    { ItemID.Rice, 30 },
                }
            },
            {
                ItemID.DynusAltarFishing0, new Dictionary<int, int>()
                {
                    { ItemID.Dorado, 1 },
                    { ItemID.Duorado, 1 },
                    { ItemID.Crab, 1 },
                    { ItemID.SeaBass, 1 },
                    { ItemID.GoldFish, 1 },
                    { ItemID.BonemouthBass, 1 },
                    { ItemID.Chromafin, 1 },
                    { ItemID.GoldenCarp, 1 },
                    { ItemID.Flamefish, 1 },
                    { ItemID.Purrmaid, 1 },
                    { ItemID.CrystalTetra, 1 },
                    { ItemID.SkyRay, 1 },
                }
            },
        };

        private static Dictionary<(int BundleID, int ItemID), int> DonatedItems = new();

        private static Dictionary<int, List<int>> ReverseLookupTable = new();


        private void Awake()
        {
            logger = this.Logger;
            try
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
                BuildReverseLookupTable();
                harmony.PatchAll();
                ScenePortalManager.onLoadedScene += OnSceneLoaded;
            }
            catch (Exception e)
            {
                logger.LogError("{PluginInfo.PLUGIN_GUID} Awake failed: " + e);
            }
        }

        private void BuildReverseLookupTable()
        {
            foreach (var bundle in Bundles)
            {
                foreach (var itemID in bundle.Value.Keys)
                {
                    if (!ReverseLookupTable.ContainsKey(itemID))
                    {
                        ReverseLookupTable[itemID] = new List<int> { bundle.Key };
                    }
                    else
                    {
                        ReverseLookupTable[itemID].Add(bundle.Key);
                    }
                }   
            }
        }

        public static void OnSceneLoaded()
        {
            foreach (var sceneId in ScenesToCheck)
            {
                logger.LogInfo(sceneId);
                HandleSceneDecorations(sceneId);
            }
        }

        private static void HandleSceneDecorations(int sceneId)
        {
            logger.LogInfo($"Checking scene {sceneId}");
            if (!SingletonBehaviour<GameSave>.Instance.CurrentWorld.decorations.ContainsKey((short)sceneId))
            {
                return;
            }
            
            var decorations = GameSave.Instance.CurrentWorld.decorations[(short)sceneId];
            
            foreach (var decoration in decorations)
            {
                DecorationPositionData decorationPositionData = decoration.Value;

                if (decorationPositionData.id != 0 && Bundles.ContainsKey(decorationPositionData.id))
                {
                    ChestData chestData = null;
                    Decoration.DeserializeMeta<ChestData>(decorationPositionData.meta, ref chestData);
                    
                    foreach (var item in chestData.items.Values)
                    {
                        if (item.Item.ID() > 0)
                        {
                            logger.LogInfo($"{decorationPositionData.id} {item.Item.ID()} {item.Amount}");
                            DonatedItems[(decorationPositionData.id, item.Item.ID())] = item.Amount;
                        }
                    }
                }
            }
        }
        
        public static string GetProgressKey(BundleType bundleType, string bundleId, int itemId)
        {
            return "Mod_MuseumTracker_" + bundleType + bundleId + itemId;
        }

        public static void RecordData(HungryMonster bundle)
        {
            if (bundle.bundleType != BundleType.MuseumBundle && bundle.bundleType != BundleType.DynusAltar)
            {
                return;
            }

            if (!Bundles.ContainsKey(bundle.id))
            {
                return;
            }
            
            // Remove current donation counts
            foreach (var itemID in Bundles[bundle.id].Keys)
            {
                logger.LogInfo($"Remove {bundle.id} {itemID}");
                DonatedItems.Remove((bundle.id, itemID));
            }
            
            foreach (var item in bundle.sellingInventory.Items)
            {
                logger.LogInfo(item.id);
                if (item.slot.itemToAccept.id > 0)
                {
                    logger.LogInfo(item.amount);
                    DonatedItems[(bundle.id, item.slot.itemToAccept.id)] = item.amount;
                }
            }
            
            // BC: remove old progress
            foreach (SlotItemData slotItemData in bundle.sellingInventory.Items)
            {
                var gameSave = SingletonBehaviour<GameSave>.Instance;

                string[] progressIds =
                {
                    GetProgressKey(bundle.bundleType, bundle.progressTokenWhenFull.progressID, slotItemData.slot.itemToAccept.id),
                    GetProgressKey(bundle.bundleType, bundle.progressTokenWhenFull.progressID, slotItemData.slot.itemToAccept.id) + "_string"
                };

                foreach (var progressId in progressIds)
                {
                    if (gameSave.CurrentWorld.progress.ContainsKey(progressId.GetStableHashCode()))
                    {
                        logger.LogInfo($"Found progress for {progressId}");
                        gameSave.CurrentWorld.progress.Remove(progressId.GetStableHashCode());
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(ItemData), nameof(ItemData.FormattedDescription), MethodType.Getter)]
        class HarmonyPatch_ItemData_FormattedDescription
        {
            private static void Postfix(ref ItemData __instance, ref string __result)
            {
                try
                {
                    if (ReverseLookupTable.TryGetValue(__instance.id, out var bundleIds))
                    {
                        foreach (int bundleId in bundleIds)
                        {
                            var requiredAmount = Bundles[bundleId][__instance.id];

                            if (!DonatedItems.TryGetValue((bundleId, __instance.id), out var donated))
                            {
                                donated = 0;
                            }

                            if (donated < requiredAmount)
                            {
                                __result += $"\n<color=#ed77f8><size=65%>Required: {BundleNames[bundleId]} ({donated}/{requiredAmount})</size></color>";
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} error: {e}");
                }
            }
        }

        [HarmonyPatch(typeof(HungryMonster), "SetMeta")]
        class HarmonyPatch_HungryMonster_SetMeta
        {
            private static void Postfix(ref HungryMonster __instance)
            {
                try
                {
                    RecordData(__instance);
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }
            }
        }
        
        [HarmonyPatch(typeof(HungryMonster), "UpdateFullness")]
        class HarmonyPatch_HungryMonster_UpdateFullness
        {
            private static void Postfix(ref HungryMonster __instance)
            {
                try
                {
                    RecordData(__instance);
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }
            }
        }

    }
}
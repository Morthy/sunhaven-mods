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

        public static Dictionary<string, string> BundleNames = new()
        {
            { "BarsBundle", "Bars Bundle (Hall of Gems)"},
            { "GoldenBundle", "Golden Bundle (Hall of Gems)"},
            { "GemBundle", "Gem Bundle (Hall of Gems)"},
            { "NelvariMinesBundle", "Nel'vari Mines Bundle (Hall of Gems)"},
            { "WithergateMinesBundle", "Withergate Mines Bundle (Hall of Gems)"},
            { "ManaBundle", "Mana Bundle (Hall of Gems)"},
            
            { "WinterCropsBundle", "Winter Crops Bundle (Hall of Culture)"},
            { "FlowersBundle", "Flowers Bundle (Hall of Culture)"},
            { "SpringCropsBundle", "Spring Crops Bundle (Hall of Culture)"},
            { "FallCropsBundle", "Fall Crops Bundle (Hall of Culture)"},
            { "NelvariTempleBooks", "Nel'vari Temple Bundle (Hall of Culture)"},
            { "SummerCropsBundle", "Summer Crops Bundle (Hall of Culture)"},
            { "ForagingBundle", "Foraging Bundle (Hall of Culture)"},
            { "CombatBundle", "Combat Bundle (Hall of Culture)"},
            { "AlchemyBundle", "Alchemy Bundle (Hall of Culture)"},
            { "ExplorationBundle", "Exploration Bundle (Hall of Culture)"},
            { "WithergateFarmingBundle", "Withergate Farming Bundle (Hall of Culture)"},
            { "NelvariFarmingBundle", "Nel'vari Farming Bundle (Hall of Culture)"},
            
            { "MuseumAquariumSpring", "Spring Fish Tank (The Aquarium)"},
            { "MuseumAquariumSummer", "Summer Fish Tank (The Aquarium)"},
            { "MuseumAquariumFall", "Fall Fish Tank (The Aquarium)"},
            { "MuseumAquariumWinter", "Winter Fish Tank (The Aquarium)"},
            { "MuseumAquariumNelvari", "Nel'vari Fish Tank (The Aquarium)"},
            { "MuseumAquariumWithergate", "Withergate Fish Tank (The Aquarium)"},
            { "MuseumAquariumBigTank", "Large Tank (The Aquarium)"},
            { "FishingBundle", "Spring Bundle (The Aquarium)"},
            
            { "DynusAltarMining", "Mining Dynus Altar"},
            { "DynusAltarFishing", "Fishing Dynus Altar"},
            { "DynusAltarForaging", "Foraging Dynus Altar"},
            { "DynusAltarCooking", "Cooking Dynus Altar"},
            { "DynusAltarGold", "Gold Dynus Altar"},
            { "DynusAltarFarming", "Farming Dynus Altar"},
            { "DynusAltarFruit", "Fruit Dynus Altar"},
            { "DynusAltarRareItems", "Keepsake Dynus Altar"},
            
        };

        public static Dictionary<int, string[]> MuseumRequirements = new()
        {
            {ItemID.Butterflyfish, new []{ "MuseumAquariumSpring"}},
            {ItemID.Sunfish, new []{"MuseumAquariumSpring"}},
            {ItemID.FlowerFlounder, new []{"MuseumAquariumSpring"}},
            {ItemID.RaincloudRay, new []{"MuseumAquariumSpring"}},
            {ItemID.FloralTrout, new []{"MuseumAquariumSpring"}},
            {ItemID.NeonTetra, new []{"MuseumAquariumSpring"}},
            {ItemID.Seahorse, new []{"MuseumAquariumSpring"}},
            {ItemID.PaintedEgg, new []{"MuseumAquariumSpring"}},
            {ItemID.Tadpole, new []{"MuseumAquariumSpring"}},

            {ItemID.Blazeel, new []{"MuseumAquariumSummer"}},
            {ItemID.HearthAngler, new []{"MuseumAquariumSummer"}},
            {ItemID.ScorchingSquid, new []{"MuseumAquariumSummer"}},
            {ItemID.MagmaStar, new []{"MuseumAquariumSummer"}},
            {ItemID.TinderTurtle, new []{"MuseumAquariumSummer"}},
            {ItemID.Pyrelus, new []{"MuseumAquariumSummer"}},
            {ItemID.FlameRay, new []{"MuseumAquariumSummer"}},
            {ItemID.MoltenSlug, new []{"MuseumAquariumSummer"}},
            {ItemID.Searback, new []{"MuseumAquariumSummer"}},

            {ItemID.Coducopia, new []{"MuseumAquariumFall"}},
            {ItemID.KingSalmon, new []{"MuseumAquariumFall"}},
            {ItemID.Hayfish, new []{"MuseumAquariumFall"}},
            {ItemID.AcornAnchovy, new []{"MuseumAquariumFall"}},
            {ItemID.VampirePiranha, new []{"MuseumAquariumFall"}},
            {ItemID.Ghostfish, new []{"MuseumAquariumFall"}},
            {ItemID.PumpkinJelly, new []{"MuseumAquariumFall"}},
            {ItemID.PiratePerch, new []{"MuseumAquariumFall"}},
            {ItemID.AutumnLeafSole, new []{"MuseumAquariumFall"}},

            {ItemID.Frostfin, new []{"MuseumAquariumWinter"}},
            {ItemID.ChristmasLightfish, new []{"MuseumAquariumWinter"}},
            {ItemID.HollyCarp, new []{"MuseumAquariumWinter"}},
            {ItemID.JingleBass, new []{"MuseumAquariumWinter"}},
            {ItemID.FrozenTuna, new []{"MuseumAquariumWinter"}},
            {ItemID.Scarffish, new []{"MuseumAquariumWinter"}},
            {ItemID.Heatfin, new []{"MuseumAquariumWinter"}},
            {ItemID.IcicleCarp, new []{"MuseumAquariumWinter"}},
            {ItemID.BlazingHerring, new []{"MuseumAquariumWinter"}},

            {ItemID.RobedParrotfish, new []{"MuseumAquariumNelvari"}},
            {ItemID.Axolotl, new []{"MuseumAquariumNelvari"}},
            {ItemID.FrilledBetta, new []{"MuseumAquariumNelvari"}},
            {ItemID.Horsefish, new []{"MuseumAquariumNelvari"}},
            {ItemID.Flamefish, new []{"MuseumAquariumNelvari"}},
            {ItemID.DragonGulper, new []{"MuseumAquariumNelvari"}},
            {ItemID.NeapolitanFish, new []{"MuseumAquariumNelvari"}},
            {ItemID.Snobfish, new []{"MuseumAquariumNelvari"}},
            {ItemID.KelpEel, new []{"MuseumAquariumNelvari"}},
            {ItemID.PrincelyFrog, new []{"MuseumAquariumNelvari"}},
            {ItemID.Angelfin, new []{"MuseumAquariumNelvari"}},
            {ItemID.Bubblefish, new []{"MuseumAquariumNelvari"}},
            {ItemID.CrystalTetra, new []{"MuseumAquariumNelvari"}},
            {ItemID.SkyRay, new []{"MuseumAquariumNelvari"}},

            {ItemID.Kraken, new []{"MuseumAquariumWithergate"}},
            {ItemID.WaterBear, new []{"MuseumAquariumWithergate"}},
            {ItemID.BonemouthBass, new []{"MuseumAquariumWithergate"}},
            {ItemID.MummyTrout, new []{"MuseumAquariumWithergate"}},
            {ItemID.DeadeyeShrimp, new []{"MuseumAquariumWithergate"}},
            {ItemID.ElectricEel, new []{"MuseumAquariumWithergate"}},
            {ItemID.BrainJelly, new []{"MuseumAquariumWithergate"}},
            {ItemID.RedfinnedPincher, new []{"MuseumAquariumWithergate"}},
            {ItemID.SeaBat, new []{"MuseumAquariumWithergate"}},
            {ItemID.GhostheadTuna, new []{"MuseumAquariumWithergate"}},
            {ItemID.Globfish, new []{"MuseumAquariumWithergate"}},
            {ItemID.LivingJelly, new []{"MuseumAquariumWithergate"}},
            {ItemID.Purrmaid, new []{"MuseumAquariumWithergate"}},
            {ItemID.SlimeLeech, new []{"MuseumAquariumWithergate"}},
            {ItemID.GoblinShark, new []{"MuseumAquariumWithergate"}},
            {ItemID.Moonfish, new []{"MuseumAquariumWithergate"}},
            {ItemID.ToothyAngler, new []{"MuseumAquariumWithergate"}},
            {ItemID.VampireSquid, new []{"MuseumAquariumWithergate"}},
            {ItemID.Viperfish, new []{"MuseumAquariumWithergate"}},
            {ItemID.AlbinoSquid, new []{"MuseumAquariumWithergate"}},
            {ItemID.Devilfin, new []{"MuseumAquariumWithergate"}},
            {ItemID.ShadowTuna, new []{"MuseumAquariumWithergate"}},

            {ItemID.PygmyTuna, new []{"MuseumAquariumBigTank"}},
            {ItemID.Catfish, new []{"MuseumAquariumBigTank"}},
            {ItemID.GoldFish, new []{"MuseumAquariumBigTank"}},
            {ItemID.StreamlineCod, new []{"MuseumAquariumBigTank"}},
            {ItemID.Salmon, new []{"MuseumAquariumBigTank"}},
            {ItemID.ClownFish, new []{"MuseumAquariumBigTank"}},
            {ItemID.BlackBass, new []{"MuseumAquariumBigTank"}},
            {ItemID.RainbowTrout, new []{"MuseumAquariumBigTank"}},
            {ItemID.PopeyeGoldfish, new []{"MuseumAquariumBigTank"}},
            {ItemID.Pufferfish, new []{"MuseumAquariumBigTank"}},
            {ItemID.IronheadSturgeon, new []{"MuseumAquariumBigTank"}},
            {ItemID.Cuddlefish, new []{"MuseumAquariumBigTank"}},
            {ItemID.Lobster, new []{"MuseumAquariumBigTank"}},
            {ItemID.SilverCarp, new []{"MuseumAquariumBigTank"}},
            {ItemID.Tuna, new []{"MuseumAquariumBigTank"}},
            {ItemID.BluntedSwordfish, new []{"MuseumAquariumBigTank"}},
            {ItemID.RibbonEel, new []{"MuseumAquariumBigTank"}},
            {ItemID.TigerTrout, new []{"MuseumAquariumBigTank"}},
            {ItemID.Eel, new []{"MuseumAquariumBigTank"}},
            {ItemID.RedSnapper, new []{"MuseumAquariumBigTank"}},
            {ItemID.Carp, new []{"MuseumAquariumBigTank"}},
            {ItemID.RedeyePiranha, new []{"MuseumAquariumBigTank"}},
            {ItemID.AngelFish, new []{"MuseumAquariumBigTank"}},
            {ItemID.WhitebellyShark, new []{"MuseumAquariumBigTank"}},
            {ItemID.KoiFish, new []{"MuseumAquariumBigTank"}},
            {ItemID.SandstoneFish, new []{"MuseumAquariumBigTank"}},

            {ItemID.HandmadeBobber, new []{"FishingBundle"}},
            {ItemID.AncientMagicStaff, new []{"FishingBundle"}},
            {ItemID.BronzeDragonRelic, new []{"FishingBundle"}},
            {ItemID.OldSwordHilt, new []{"FishingBundle"}},
            {ItemID.NelVarianRunestone, new []{"FishingBundle"}},
            {ItemID.AncientElvenHeaddress, new []{"FishingBundle"}},
            {ItemID.OldMayoralPainting, new []{"FishingBundle"}},
            {ItemID.TentacleMonsterEmblem, new []{"FishingBundle"}},
            {ItemID.AncientAngelQuill, new []{"FishingBundle"}},
            {ItemID.AncientNagaCrook, new []{"FishingBundle"}},
            {ItemID.AncientAmariTotem, new []{"FishingBundle"}},

            {ItemID.GoldenMilk, new []{"GoldenBundle"}},
            {ItemID.GoldenEgg, new []{"GoldenBundle"}},
            {ItemID.GoldenWool, new []{"GoldenBundle"}},
            {ItemID.GoldenPomegranate, new []{"GoldenBundle"}},
            {ItemID.GoldenLog, new []{"GoldenBundle"}},
            {ItemID.GoldenFeather, new []{"GoldenBundle"}},
            {ItemID.GoldenSilk, new []{"GoldenBundle"}},
            {ItemID.GoldenApple, new []{"GoldenBundle"}},
            {ItemID.GoldenOrange, new []{"GoldenBundle"}},
            {ItemID.GoldenStrawberry, new []{"GoldenBundle"}},
            {ItemID.GoldenBlueberry, new []{"GoldenBundle"}},
            {ItemID.GoldenPeach, new []{"GoldenBundle"}},
            {ItemID.GoldenRaspberry, new []{"GoldenBundle"}},

            {ItemID.Sapphire, new []{"GemBundle"}},
            {ItemID.Ruby, new []{"GemBundle"}},
            {ItemID.Amethyst, new []{"GemBundle"}},
            {ItemID.Diamond, new []{"GemBundle"}},
            {ItemID.Havenite, new []{"GemBundle"}},
            {ItemID.Dizzite, new []{"GemBundle"}},
            {ItemID.BlackDiamond, new []{"GemBundle"}},

            {ItemID.ManaDrop, new []{"ManaBundle"}},

            {ItemID.CopperBar, new []{"BarsBundle"}},
            {ItemID.IronBar, new []{"BarsBundle"}},
            {ItemID.GoldBar, new []{"BarsBundle"}},
            {ItemID.AdamantBar, new []{"BarsBundle"}},
            {ItemID.MithrilBar, new []{"BarsBundle"}},
            {ItemID.SuniteBar, new []{"BarsBundle"}},
            {ItemID.ElvenSteelBar, new []{"BarsBundle"}},
            {ItemID.GloriteBar, new []{"BarsBundle"}},

            {ItemID.ManaShard, new []{"NelvariMinesBundle"}},
            {ItemID.SparklingDragonScale, new []{"NelvariMinesBundle"}},
            {ItemID.SharpDragonScale, new []{"NelvariMinesBundle"}},
            {ItemID.ToughDragonScale, new []{"NelvariMinesBundle"}},
            {ItemID.CandyCornPieces, new []{"WithergateMinesBundle"}},
            {ItemID.RockCandyGem, new []{"WithergateMinesBundle"}},
            {ItemID.JawbreakerGem, new []{"WithergateMinesBundle"}},
            {ItemID.HardButterscotchGem, new []{"WithergateMinesBundle"}},

            {ItemID.TeaLeaves, new []{"WinterCropsBundle"}},
            {ItemID.Turnip, new []{"WinterCropsBundle"}},
            {ItemID.PurpleEggplant, new []{"WinterCropsBundle"}},
            {ItemID.HeatFruit, new []{"WinterCropsBundle"}},
            {ItemID.MarshmallowBean, new []{"WinterCropsBundle"}},
            {ItemID.BrrNana, new []{"WinterCropsBundle"}},
            {ItemID.StarFruit, new []{"WinterCropsBundle"}},
            {ItemID.HexagonBerry, new []{"WinterCropsBundle"}},
            {ItemID.SnowPea, new []{"WinterCropsBundle"}},
            {ItemID.SnowBallCrop, new []{"WinterCropsBundle"}},
            {ItemID.BlizzardBerry, new []{"WinterCropsBundle"}},
            {ItemID.BalloonFruit, new []{"WinterCropsBundle"}},
            {ItemID.PythagoreanBerry, new []{"WinterCropsBundle"}},
            {ItemID.BlueMoonFruit, new []{"WinterCropsBundle"}},
            {ItemID.CandyCane, new []{"WinterCropsBundle"}},

            {ItemID.HoneyFlower, new []{"FlowersBundle"}},
            {ItemID.RedRose, new []{"FlowersBundle"}},
            {ItemID.BlueRose, new []{"FlowersBundle"}},
            {ItemID.Daisy, new []{"FlowersBundle"}},
            {ItemID.Orchid, new []{"FlowersBundle"}},
            {ItemID.Tulip, new []{"FlowersBundle"}},
            {ItemID.Hibiscus, new []{"FlowersBundle"}},
            {ItemID.Lavender, new []{"FlowersBundle"}},
            {ItemID.Sunflower, new []{"FlowersBundle"}},
            {ItemID.Lily, new []{"FlowersBundle"}},
            {ItemID.Lotus, new []{"FlowersBundle"}},

            {ItemID.Grapes, new []{"SpringCropsBundle"}},
            {ItemID.Wheat, new []{"SpringCropsBundle"}},
            {ItemID.Tomato, new []{"SpringCropsBundle"}},
            {ItemID.Corn, new []{"SpringCropsBundle"}},
            {ItemID.Onion, new []{"SpringCropsBundle"}},
            {ItemID.Potato, new []{"SpringCropsBundle"}},
            {ItemID.Greenroot, new []{"SpringCropsBundle"}},
            {ItemID.Carrot, new []{"SpringCropsBundle"}},
            {ItemID.Kale, new []{"SpringCropsBundle"}},
            {ItemID.Lettuce, new []{"SpringCropsBundle"}},
            {ItemID.Cinnaberry, new []{"SpringCropsBundle"}},
            {ItemID.Shimmeroot, new []{"SpringCropsBundle"}},

            {ItemID.Garlic, new []{"FallCropsBundle"}},
            {ItemID.Yam, new []{"FallCropsBundle"}},
            {ItemID.SodaPopCrop, new []{"FallCropsBundle"}},
            {ItemID.FizzyFruit, new []{"FallCropsBundle"}},
            {ItemID.Cranberry, new []{"FallCropsBundle"}},
            {ItemID.Barley, new []{"FallCropsBundle"}},
            {ItemID.Pumpkin, new []{"FallCropsBundle"}},
            {ItemID.GhostPepper, new []{"FallCropsBundle"}},
            {ItemID.Butternut, new []{"FallCropsBundle"}},

            {ItemID.OriginsoftheGrandTreeandNivaraBookI, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsoftheGrandTreeandNivaraBookII, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsoftheGrandTreeandNivaraBookIII, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsoftheGrandTreeandNivaraBookIV, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsoftheGrandTreeandNivaraBookV, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofSunHavenandEliosBookI, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofSunHavenandEliosBookII, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofSunHavenandEliosBookIII, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofSunHavenandEliosBookIV, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofSunHavenandEliosBookV, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofDynusandShadowsBookI, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofDynusandShadowsBookII, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofDynusandShadowsBookIII, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofDynusandShadowsBookIV, new []{"NelvariTempleBooks"}},
            {ItemID.OriginsofDynusandShadowsBookV, new []{"NelvariTempleBooks"}},

            {ItemID.Armoranth, new []{"SummerCropsBundle"}},
            {ItemID.GuavaBerry, new []{"SummerCropsBundle"}},
            {ItemID.Beet, new []{"SummerCropsBundle"}},
            {ItemID.Lemon, new []{"SummerCropsBundle"}},
            {ItemID.Chocoberry, new []{"SummerCropsBundle"}},
            {ItemID.Pineapple, new []{"SummerCropsBundle"}},
            {ItemID.Pepper, new []{"SummerCropsBundle", "SpringCropsBundle"}},
            {ItemID.Melon, new []{"SummerCropsBundle"}},
            {ItemID.Stormelon, new []{"SummerCropsBundle"}},
            {ItemID.Durian, new []{"SummerCropsBundle"}},

            {ItemID.Log, new []{"ForagingBundle"}},
            {ItemID.Apple, new []{"ForagingBundle"}},
            {ItemID.Seaweed, new []{"ForagingBundle"}},
            {ItemID.Blueberry, new []{"ForagingBundle"}},
            {ItemID.Mushroom, new []{"ForagingBundle"}},
            {ItemID.Orange, new []{"ForagingBundle"}},
            {ItemID.Strawberry, new []{"ForagingBundle"}},
            {ItemID.Berry, new []{"ForagingBundle"}},
            {ItemID.Raspberry, new []{"ForagingBundle"}},
            {ItemID.Peach, new []{"ForagingBundle"}},
            {ItemID.SandDollar, new []{"ForagingBundle"}},
            {ItemID.Starfish, new []{"ForagingBundle"}},

            {ItemID.LeafieTrinket, new []{"CombatBundle"}},
            {ItemID.EliteLeafieTrinket, new []{"CombatBundle"}},
            {ItemID.CentipillarTrinket, new []{"CombatBundle"}},
            {ItemID.PeppinchGreenTrinket, new []{"CombatBundle"}},
            {ItemID.ScorpepperTrinket, new []{"CombatBundle"}},
            {ItemID.EliteScorpepperTrinket, new []{"CombatBundle"}},
            {ItemID.HatCrabTrinket, new []{"CombatBundle"}},
            {ItemID.FloatyCrabTrinket, new []{"CombatBundle"}},
            {ItemID.BucketCrabTrinket, new []{"CombatBundle"}},
            {ItemID.UmbrellaCrabTrinket, new []{"CombatBundle"}},
            {ItemID.ChimchuckTrinket, new []{"CombatBundle"}},
            {ItemID.AncientSunHavenSword, new []{"CombatBundle"}},
            {ItemID.AncientNelVarianSword, new []{"CombatBundle"}},
            {ItemID.AncientWithergateSword, new []{"CombatBundle"}},

            {ItemID.ManaPotion, new []{"AlchemyBundle"}},
            {ItemID.HealthPotion, new []{"AlchemyBundle"}},
            {ItemID.AttackPotion, new []{"AlchemyBundle"}},
            {ItemID.SpeedPotion, new []{"AlchemyBundle"}},
            {ItemID.DefensePotion, new []{"AlchemyBundle"}},
            {ItemID.AdvancedAttackPotion, new []{"AlchemyBundle"}},
            {ItemID.AdvancedDefensePotion, new []{"AlchemyBundle"}},
            {ItemID.AdvancedSpellDamagePotion, new []{"AlchemyBundle"}},
            {ItemID.IncredibleSpellDamagePotion, new []{"AlchemyBundle"}},
            {ItemID.IncredibleAttackPotion, new []{"AlchemyBundle"}},
            {ItemID.IncredibleDefensePotion, new []{"AlchemyBundle"}},

            {ItemID.PetrifiedLog, new []{"ExplorationBundle"}},
            {ItemID.PhoenixFeather, new []{"ExplorationBundle"}},
            {ItemID.FairyWings, new []{"ExplorationBundle"}},
            {ItemID.GriffonEgg, new []{"ExplorationBundle"}},
            {ItemID.ManaSap, new []{"ExplorationBundle"}},
            {ItemID.PumiceStone, new []{"ExplorationBundle"}},
            {ItemID.MysteriousAntler, new []{"ExplorationBundle"}},
            {ItemID.DragonFang, new []{"ExplorationBundle"}},
            {ItemID.MonsterCandy, new []{"ExplorationBundle"}},
            {ItemID.UnicornHairTuft, new []{"ExplorationBundle"}},

            {ItemID.KrakenKale, new []{"WithergateFarmingBundle"}},
            {ItemID.Tombmelon, new []{"WithergateFarmingBundle"}},
            {ItemID.Suckerstem, new []{"WithergateFarmingBundle"}},
            {ItemID.Razorstalk, new []{"WithergateFarmingBundle"}},
            {ItemID.SnappyPlant, new []{"WithergateFarmingBundle"}},
            {ItemID.Moonplant, new []{"WithergateFarmingBundle"}},
            {ItemID.Eggplant, new []{"WithergateFarmingBundle"}},
            {ItemID.DemonOrb, new []{"WithergateFarmingBundle"}},

            {ItemID.Acorn, new []{"NelvariFarmingBundle"}},
            {ItemID.RockFruit, new []{"NelvariFarmingBundle"}},
            {ItemID.WaterFruit, new []{"NelvariFarmingBundle"}},
            {ItemID.FireFruit, new []{"NelvariFarmingBundle"}},
            {ItemID.WalkChoy, new []{"NelvariFarmingBundle"}},
            {ItemID.WindChime, new []{"NelvariFarmingBundle"}},
            {ItemID.ShiiwalkiMushroom, new []{"NelvariFarmingBundle"}},
            {ItemID.DragonFruit, new []{"NelvariFarmingBundle"}},
            {ItemID.ManaGem, new []{"NelvariFarmingBundle"}},
            {ItemID.CatTail, new []{"NelvariFarmingBundle"}},
            {ItemID.Indiglow, new []{"NelvariFarmingBundle"}},

        };

        public static Dictionary<int, string[]> AltarRequirements = new()
        {
            {ItemID.Stone, new []{"DynusAltarMining"}},
            {ItemID.Coal, new []{"DynusAltarMining"}},
            {ItemID.CopperOre, new []{"DynusAltarMining"}},
            {ItemID.Sapphire, new []{"DynusAltarMining"}},
            {ItemID.Ruby, new []{"DynusAltarMining"}},
            {ItemID.Amethyst, new []{"DynusAltarMining"}},
            {ItemID.Diamond, new []{"DynusAltarMining"}},
            {ItemID.Havenite, new []{"DynusAltarMining"}},

            {ItemID.Dorado, new []{"DynusAltarFishing"}},
            {ItemID.Duorado, new []{"DynusAltarFishing"}},
            {ItemID.Crab, new []{"DynusAltarFishing"}},
            {ItemID.SeaBass, new []{"DynusAltarFishing"}},
            {ItemID.GoldFish, new []{"DynusAltarFishing"}},
            {ItemID.BonemouthBass, new []{"DynusAltarFishing"}},
            {ItemID.Chromafin, new []{"DynusAltarFishing"}},
            {ItemID.GoldenCarp, new []{"DynusAltarFishing"}},
            {ItemID.Flamefish, new []{"DynusAltarFishing"}},
            {ItemID.Purrmaid, new []{"DynusAltarFishing"}},
            {ItemID.CrystalTetra, new []{"DynusAltarFishing"}},
            {ItemID.SkyRay, new []{"DynusAltarFishing"}},

            {ItemID.Log, new []{"DynusAltarForaging"}},
            {ItemID.FireCrystal, new []{"DynusAltarForaging"}},
            {ItemID.EarthCrystal, new []{"DynusAltarForaging"}},
            {ItemID.WaterCrystal, new []{"DynusAltarForaging"}},
            {ItemID.SandDollar, new []{"DynusAltarForaging"}},

            {ItemID.Cheesecake, new []{"DynusAltarCooking"}},
            {ItemID.SpicyRamen, new []{"DynusAltarCooking"}},
            {ItemID.SesameRiceBall, new []{"DynusAltarCooking"}},
            {ItemID.Pizza, new []{"DynusAltarCooking"}},
            {ItemID.Cookies, new []{"DynusAltarCooking"}},
            {ItemID.Coffee, new []{"DynusAltarCooking"}},
            {ItemID.TomatoSoup, new []{"DynusAltarCooking"}},
            {ItemID.ShimmerootTreat, new []{"DynusAltarCooking"}},
            {ItemID.EnergySmoothie, new []{"DynusAltarCooking"}},

            {ItemID.GoldBar, new []{"DynusAltarGold"}},

            {ItemID.Wheat, new []{"DynusAltarFarming"}},
            {ItemID.Corn, new []{"DynusAltarFarming"}},
            {ItemID.Potato, new []{"DynusAltarFarming"}},
            {ItemID.Tomato, new []{"DynusAltarFarming"}},
            {ItemID.Carrot, new []{"DynusAltarFarming"}},
            {ItemID.SugarCane, new []{"DynusAltarFarming"}},
            {ItemID.Onion, new []{"DynusAltarFarming"}},
            {ItemID.Greenroot, new []{"DynusAltarFarming"}},
            {ItemID.HoneyFlower, new []{"DynusAltarFarming"}},
            {ItemID.Rice, new []{"DynusAltarFarming"}},

            {ItemID.Raspberry, new []{"DynusAltarFruit"}},
            {ItemID.Peach, new []{"DynusAltarFruit"}},
            {ItemID.Orange, new []{"DynusAltarFruit"}},
            {ItemID.Blueberry, new []{"DynusAltarFruit"}},
            {ItemID.Berry, new []{"DynusAltarFruit"}},
            {ItemID.Apple, new []{"DynusAltarFruit"}},

            {ItemID.AdventureKeepsake, new []{"DynusAltarRareItems"}},
            {ItemID.RichesKeepsake, new []{"DynusAltarRareItems"}},
            {ItemID.RomanceKeepsake, new []{"DynusAltarRareItems"}},
            {ItemID.PeaceKeepsake, new []{"DynusAltarRareItems"}},

        };

        private void Awake()
        {
            logger = this.Logger;
            try
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
                this.harmony.PatchAll();
            }
            catch (Exception e)
            {
                logger.LogError("{PluginInfo.PLUGIN_GUID} Awake failed: " + e);
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
            
            string debug = "";
            
            foreach (SlotItemData slotItemData in bundle.sellingInventory.Items)
            {
                foreach (var field in typeof(ItemID).GetFields())
                {
                    if ((int)field.GetValue(null) == slotItemData.slot.itemToAccept.id)
                    {
                        debug += "{ItemID." + field.Name + ", \"" + bundle.progressTokenWhenFull.progressID + "\"},\n";
                    }
                }
                
                SingletonBehaviour<GameSave>.Instance.SetProgressBoolWorld(GetProgressKey(bundle.bundleType, bundle.progressTokenWhenFull.progressID, slotItemData.slot.itemToAccept.id), slotItemData.amount >= slotItemData.slot.numberOfItemToAccept);
                SingletonBehaviour<GameSave>.Instance.SetProgressStringWorld(GetProgressKey(bundle.bundleType, bundle.progressTokenWhenFull.progressID, slotItemData.slot.itemToAccept.id) + "_string", slotItemData.amount.ToString() + "/" + slotItemData.slot.numberOfItemToAccept.ToString());
            }
            //logger.LogInfo(debug);
        }
        
        [HarmonyPatch(typeof(ItemData), nameof(ItemData.FormattedDescription), MethodType.Getter)]
        class HarmonyPatch_ItemData_FormattedDescription
        {
            private static void Postfix(ref ItemData __instance, ref string __result)
            {
                try
                {
                    if (MuseumRequirements.TryGetValue(__instance.id, out string[] museumBundles))
                    {
                        foreach (string museumBundle in museumBundles)
                        { 
                            bool progress = !SingletonBehaviour<GameSave>.Instance.GetProgressBoolWorld(GetProgressKey(BundleType.MuseumBundle, museumBundle, __instance.id));
                            if (!progress) continue;
                            string amounts = SingletonBehaviour<GameSave>.Instance.GetProgressStringWorld(GetProgressKey(BundleType.MuseumBundle, museumBundle, __instance.id) + "_string");
                            if (amounts.Length > 0)
                            {
                                __result = __result + "\n<color=#ed77f8><size=65%>Required: " + BundleNames[museumBundle] + " ("+ amounts + ")</size></color>";
                            }
                            else
                            {
                                __result = __result + "\n<color=#ed77f8><size=65%>Required: " + BundleNames[museumBundle] + "</size></color>";
                            }
                        }
                    }

                    if (AltarRequirements.TryGetValue(__instance.id, out string[] altarBundles))
                    {
                        foreach (string altarBundle in altarBundles)
                        { 
                            bool progress = !SingletonBehaviour<GameSave>.Instance.GetProgressBoolWorld(GetProgressKey(BundleType.DynusAltar, altarBundle, __instance.id));
                            if (!progress) continue;
                            string amounts = SingletonBehaviour<GameSave>.Instance.GetProgressStringWorld(GetProgressKey(BundleType.DynusAltar, altarBundle, __instance.id) + "_string");
                            if (amounts.Length > 0)
                            {
                                __result = __result + "\n<color=#ed77f8><size=65%>Required: " + BundleNames[altarBundle] + " ("+ amounts + ")</size></color>";
                            }
                            else
                            {
                                __result = __result + "\n<color=#ed77f8><size=65%>Required: " + BundleNames[altarBundle] + "</size></color>";
                            }
                        }
                    }                   
                }
                catch (Exception e)
                {
                    logger.LogError(e);
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
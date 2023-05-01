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

        public static Dictionary<int, string> MuseumRequirements = new()
        {
            {ItemID.Butterflyfish, "MuseumAquariumSpring"},
            {ItemID.Sunfish, "MuseumAquariumSpring"},
            {ItemID.FlowerFlounder, "MuseumAquariumSpring"},
            {ItemID.RaincloudRay, "MuseumAquariumSpring"},
            {ItemID.FloralTrout, "MuseumAquariumSpring"},
            {ItemID.NeonTetra, "MuseumAquariumSpring"},
            {ItemID.Seahorse, "MuseumAquariumSpring"},
            {ItemID.PaintedEgg, "MuseumAquariumSpring"},
            {ItemID.Tadpole, "MuseumAquariumSpring"},
            
            {ItemID.Blazeel, "MuseumAquariumSummer"},
            {ItemID.HearthAngler, "MuseumAquariumSummer"},
            {ItemID.ScorchingSquid, "MuseumAquariumSummer"},
            {ItemID.MagmaStar, "MuseumAquariumSummer"},
            {ItemID.TinderTurtle, "MuseumAquariumSummer"},
            {ItemID.Pyrelus, "MuseumAquariumSummer"},
            {ItemID.FlameRay, "MuseumAquariumSummer"},
            {ItemID.MoltenSlug, "MuseumAquariumSummer"},
            {ItemID.Searback, "MuseumAquariumSummer"},
            
            {ItemID.Coducopia, "MuseumAquariumFall"},
            {ItemID.KingSalmon, "MuseumAquariumFall"},
            {ItemID.Hayfish, "MuseumAquariumFall"},
            {ItemID.AcornAnchovy, "MuseumAquariumFall"},
            {ItemID.VampirePiranha, "MuseumAquariumFall"},
            {ItemID.Ghostfish, "MuseumAquariumFall"},
            {ItemID.PumpkinJelly, "MuseumAquariumFall"},
            {ItemID.PiratePerch, "MuseumAquariumFall"},
            {ItemID.AutumnLeafSole, "MuseumAquariumFall"},
            
            {ItemID.Frostfin, "MuseumAquariumWinter"},
            {ItemID.ChristmasLightfish, "MuseumAquariumWinter"},
            {ItemID.HollyCarp, "MuseumAquariumWinter"},
            {ItemID.JingleBass, "MuseumAquariumWinter"},
            {ItemID.FrozenTuna, "MuseumAquariumWinter"},
            {ItemID.Scarffish, "MuseumAquariumWinter"},
            {ItemID.Heatfin, "MuseumAquariumWinter"},
            {ItemID.IcicleCarp, "MuseumAquariumWinter"},
            {ItemID.BlazingHerring, "MuseumAquariumWinter"},
            
            {ItemID.RobedParrotfish, "MuseumAquariumNelvari"},
            {ItemID.Axolotl, "MuseumAquariumNelvari"},
            {ItemID.FrilledBetta, "MuseumAquariumNelvari"},
            {ItemID.Horsefish, "MuseumAquariumNelvari"},
            {ItemID.Flamefish, "MuseumAquariumNelvari"},
            {ItemID.DragonGulper, "MuseumAquariumNelvari"},
            {ItemID.NeapolitanFish, "MuseumAquariumNelvari"},
            {ItemID.Snobfish, "MuseumAquariumNelvari"},
            {ItemID.KelpEel, "MuseumAquariumNelvari"},
            {ItemID.PrincelyFrog, "MuseumAquariumNelvari"},
            {ItemID.Angelfin, "MuseumAquariumNelvari"},
            {ItemID.Bubblefish, "MuseumAquariumNelvari"},
            {ItemID.CrystalTetra, "MuseumAquariumNelvari"},
            {ItemID.SkyRay, "MuseumAquariumNelvari"},
            
            {ItemID.Kraken, "MuseumAquariumWithergate"},
            {ItemID.WaterBear, "MuseumAquariumWithergate"},
            {ItemID.BonemouthBass, "MuseumAquariumWithergate"},
            {ItemID.MummyTrout, "MuseumAquariumWithergate"},
            {ItemID.DeadeyeShrimp, "MuseumAquariumWithergate"},
            {ItemID.ElectricEel, "MuseumAquariumWithergate"},
            {ItemID.BrainJelly, "MuseumAquariumWithergate"},
            {ItemID.RedfinnedPincher, "MuseumAquariumWithergate"},
            {ItemID.SeaBat, "MuseumAquariumWithergate"},
            {ItemID.GhostheadTuna, "MuseumAquariumWithergate"},
            {ItemID.Globfish, "MuseumAquariumWithergate"},
            {ItemID.LivingJelly, "MuseumAquariumWithergate"},
            {ItemID.Purrmaid, "MuseumAquariumWithergate"},
            {ItemID.SlimeLeech, "MuseumAquariumWithergate"},
            {ItemID.GoblinShark, "MuseumAquariumWithergate"},
            {ItemID.Moonfish, "MuseumAquariumWithergate"},
            {ItemID.ToothyAngler, "MuseumAquariumWithergate"},
            {ItemID.VampireSquid, "MuseumAquariumWithergate"},
            {ItemID.Viperfish, "MuseumAquariumWithergate"},
            {ItemID.AlbinoSquid, "MuseumAquariumWithergate"},
            {ItemID.Devilfin, "MuseumAquariumWithergate"},
            {ItemID.ShadowTuna, "MuseumAquariumWithergate"},
            
            {ItemID.PygmyTuna, "MuseumAquariumBigTank"},
            {ItemID.Catfish, "MuseumAquariumBigTank"},
            {ItemID.GoldFish, "MuseumAquariumBigTank"},
            {ItemID.StreamlineCod, "MuseumAquariumBigTank"},
            {ItemID.Salmon, "MuseumAquariumBigTank"},
            {ItemID.ClownFish, "MuseumAquariumBigTank"},
            {ItemID.BlackBass, "MuseumAquariumBigTank"},
            {ItemID.RainbowTrout, "MuseumAquariumBigTank"},
            {ItemID.PopeyeGoldfish, "MuseumAquariumBigTank"},
            {ItemID.Pufferfish, "MuseumAquariumBigTank"},
            {ItemID.IronheadSturgeon, "MuseumAquariumBigTank"},
            {ItemID.Cuddlefish, "MuseumAquariumBigTank"},
            {ItemID.Lobster, "MuseumAquariumBigTank"},
            {ItemID.SilverCarp, "MuseumAquariumBigTank"},
            {ItemID.Tuna, "MuseumAquariumBigTank"},
            {ItemID.BluntedSwordfish, "MuseumAquariumBigTank"},
            {ItemID.RibbonEel, "MuseumAquariumBigTank"},
            {ItemID.TigerTrout, "MuseumAquariumBigTank"},
            {ItemID.Eel, "MuseumAquariumBigTank"},
            {ItemID.RedSnapper, "MuseumAquariumBigTank"},
            {ItemID.Carp, "MuseumAquariumBigTank"},
            {ItemID.RedeyePiranha, "MuseumAquariumBigTank"},
            {ItemID.AngelFish, "MuseumAquariumBigTank"},
            {ItemID.WhitebellyShark, "MuseumAquariumBigTank"},
            {ItemID.KoiFish, "MuseumAquariumBigTank"},
            {ItemID.SandstoneFish, "MuseumAquariumBigTank"},
            
            {ItemID.HandmadeBobber, "FishingBundle"},
            {ItemID.AncientMagicStaff, "FishingBundle"},
            {ItemID.BronzeDragonRelic, "FishingBundle"},
            {ItemID.OldSwordHilt, "FishingBundle"},
            {ItemID.NelVarianRunestone, "FishingBundle"},
            {ItemID.AncientElvenHeaddress, "FishingBundle"},
            {ItemID.OldMayoralPainting, "FishingBundle"},
            {ItemID.TentacleMonsterEmblem, "FishingBundle"},
            {ItemID.AncientAngelQuill, "FishingBundle"},
            {ItemID.AncientNagaCrook, "FishingBundle"},
            {ItemID.AncientAmariTotem, "FishingBundle"},
            
            {ItemID.GoldenMilk, "GoldenBundle"},
            {ItemID.GoldenEgg, "GoldenBundle"},
            {ItemID.GoldenWool, "GoldenBundle"},
            {ItemID.GoldenPomegranate, "GoldenBundle"},
            {ItemID.GoldenLog, "GoldenBundle"},
            {ItemID.GoldenFeather, "GoldenBundle"},
            {ItemID.GoldenSilk, "GoldenBundle"},
            {ItemID.GoldenApple, "GoldenBundle"},
            {ItemID.GoldenOrange, "GoldenBundle"},
            {ItemID.GoldenStrawberry, "GoldenBundle"},
            {ItemID.GoldenBlueberry, "GoldenBundle"},
            {ItemID.GoldenPeach, "GoldenBundle"},
            {ItemID.GoldenRaspberry, "GoldenBundle"},
            
            {ItemID.Sapphire, "GemBundle"},
            {ItemID.Ruby, "GemBundle"},
            {ItemID.Amethyst, "GemBundle"},
            {ItemID.Diamond, "GemBundle"},
            {ItemID.Havenite, "GemBundle"},
            {ItemID.Dizzite, "GemBundle"},
            {ItemID.BlackDiamond, "GemBundle"},
            
            {ItemID.ManaDrop, "ManaBundle"},
            
            {ItemID.CopperBar, "BarsBundle"},
            {ItemID.IronBar, "BarsBundle"},
            {ItemID.GoldBar, "BarsBundle"},
            {ItemID.AdamantBar, "BarsBundle"},
            {ItemID.MithrilBar, "BarsBundle"},
            {ItemID.SuniteBar, "BarsBundle"},
            {ItemID.ElvenSteelBar, "BarsBundle"},
            {ItemID.GloriteBar, "BarsBundle"},
            
            {ItemID.ManaShard, "NelvariMinesBundle"},
            {ItemID.SparklingDragonScale, "NelvariMinesBundle"},
            {ItemID.SharpDragonScale, "NelvariMinesBundle"},
            {ItemID.ToughDragonScale, "NelvariMinesBundle"},
            {ItemID.CandyCornPieces, "WithergateMinesBundle"},
            {ItemID.RockCandyGem, "WithergateMinesBundle"},
            {ItemID.JawbreakerGem, "WithergateMinesBundle"},
            {ItemID.HardButterscotchGem, "WithergateMinesBundle"},
            
            {ItemID.TeaLeaves, "WinterCropsBundle"},
            {ItemID.Turnip, "WinterCropsBundle"},
            {ItemID.PurpleEggplant, "WinterCropsBundle"},
            {ItemID.HeatFruit, "WinterCropsBundle"},
            {ItemID.MarshmallowBean, "WinterCropsBundle"},
            {ItemID.BrrNana, "WinterCropsBundle"},
            {ItemID.StarFruit, "WinterCropsBundle"},
            {ItemID.HexagonBerry, "WinterCropsBundle"},
            {ItemID.SnowPea, "WinterCropsBundle"},
            {ItemID.SnowBallCrop, "WinterCropsBundle"},
            {ItemID.BlizzardBerry, "WinterCropsBundle"},
            {ItemID.BalloonFruit, "WinterCropsBundle"},
            {ItemID.PythagoreanBerry, "WinterCropsBundle"},
            {ItemID.BlueMoonFruit, "WinterCropsBundle"},
            {ItemID.CandyCane, "WinterCropsBundle"},
            
            {ItemID.HoneyFlower, "FlowersBundle"},
            {ItemID.RedRose, "FlowersBundle"},
            {ItemID.BlueRose, "FlowersBundle"},
            {ItemID.Daisy, "FlowersBundle"},
            {ItemID.Orchid, "FlowersBundle"},
            {ItemID.Tulip, "FlowersBundle"},
            {ItemID.Hibiscus, "FlowersBundle"},
            {ItemID.Lavender, "FlowersBundle"},
            {ItemID.Sunflower, "FlowersBundle"},
            {ItemID.Lily, "FlowersBundle"},
            {ItemID.Lotus, "FlowersBundle"},
            
            {ItemID.Grapes, "SpringCropsBundle"},
            {ItemID.Wheat, "SpringCropsBundle"},
            {ItemID.Tomato, "SpringCropsBundle"},
            {ItemID.Corn, "SpringCropsBundle"},
            {ItemID.Onion, "SpringCropsBundle"},
            {ItemID.Potato, "SpringCropsBundle"},
            {ItemID.Greenroot, "SpringCropsBundle"},
            {ItemID.Carrot, "SpringCropsBundle"},
            {ItemID.Kale, "SpringCropsBundle"},
            {ItemID.Lettuce, "SpringCropsBundle"},
            {ItemID.Cinnaberry, "SpringCropsBundle"},
            //{ItemID.Pepper, "SpringCropsBundle"},
            {ItemID.Shimmeroot, "SpringCropsBundle"},
            
            {ItemID.Garlic, "FallCropsBundle"},
            {ItemID.Yam, "FallCropsBundle"},
            {ItemID.SodaPopCrop, "FallCropsBundle"},
            {ItemID.FizzyFruit, "FallCropsBundle"},
            {ItemID.Cranberry, "FallCropsBundle"},
            {ItemID.Barley, "FallCropsBundle"},
            {ItemID.Pumpkin, "FallCropsBundle"},
            {ItemID.GhostPepper, "FallCropsBundle"},
            {ItemID.Butternut, "FallCropsBundle"},
            
            {ItemID.OriginsoftheGrandTreeandNivaraBookI, "NelvariTempleBooks"},
            {ItemID.OriginsoftheGrandTreeandNivaraBookII, "NelvariTempleBooks"},
            {ItemID.OriginsoftheGrandTreeandNivaraBookIII, "NelvariTempleBooks"},
            {ItemID.OriginsoftheGrandTreeandNivaraBookIV, "NelvariTempleBooks"},
            {ItemID.OriginsoftheGrandTreeandNivaraBookV, "NelvariTempleBooks"},
            {ItemID.OriginsofSunHavenandEliosBookI, "NelvariTempleBooks"},
            {ItemID.OriginsofSunHavenandEliosBookII, "NelvariTempleBooks"},
            {ItemID.OriginsofSunHavenandEliosBookIII, "NelvariTempleBooks"},
            {ItemID.OriginsofSunHavenandEliosBookIV, "NelvariTempleBooks"},
            {ItemID.OriginsofSunHavenandEliosBookV, "NelvariTempleBooks"},
            {ItemID.OriginsofDynusandShadowsBookI, "NelvariTempleBooks"},
            {ItemID.OriginsofDynusandShadowsBookII, "NelvariTempleBooks"},
            {ItemID.OriginsofDynusandShadowsBookIII, "NelvariTempleBooks"},
            {ItemID.OriginsofDynusandShadowsBookIV, "NelvariTempleBooks"},
            {ItemID.OriginsofDynusandShadowsBookV, "NelvariTempleBooks"},
            
            {ItemID.Armoranth, "SummerCropsBundle"},
            {ItemID.GuavaBerry, "SummerCropsBundle"},
            {ItemID.Beet, "SummerCropsBundle"},
            {ItemID.Lemon, "SummerCropsBundle"},
            {ItemID.Chocoberry, "SummerCropsBundle"},
            {ItemID.Pineapple, "SummerCropsBundle"},
            {ItemID.Pepper, "SummerCropsBundle"},
            {ItemID.Melon, "SummerCropsBundle"},
            {ItemID.Stormelon, "SummerCropsBundle"},
            {ItemID.Durian, "SummerCropsBundle"},
            
            {ItemID.Log, "ForagingBundle"},
            {ItemID.Apple, "ForagingBundle"},
            {ItemID.Seaweed, "ForagingBundle"},
            {ItemID.Blueberry, "ForagingBundle"},
            {ItemID.Mushroom, "ForagingBundle"},
            {ItemID.Orange, "ForagingBundle"},
            {ItemID.Strawberry, "ForagingBundle"},
            {ItemID.Berry, "ForagingBundle"},
            {ItemID.Raspberry, "ForagingBundle"},
            {ItemID.Peach, "ForagingBundle"},
            {ItemID.SandDollar, "ForagingBundle"},
            {ItemID.Starfish, "ForagingBundle"},
            
            {ItemID.LeafieTrinket, "CombatBundle"},
            {ItemID.EliteLeafieTrinket, "CombatBundle"},
            {ItemID.CentipillarTrinket, "CombatBundle"},
            {ItemID.PeppinchGreenTrinket, "CombatBundle"},
            {ItemID.ScorpepperTrinket, "CombatBundle"},
            {ItemID.EliteScorpepperTrinket, "CombatBundle"},
            {ItemID.HatCrabTrinket, "CombatBundle"},
            {ItemID.FloatyCrabTrinket, "CombatBundle"},
            {ItemID.BucketCrabTrinket, "CombatBundle"},
            {ItemID.UmbrellaCrabTrinket, "CombatBundle"},
            {ItemID.ChimchuckTrinket, "CombatBundle"},
            {ItemID.AncientSunHavenSword, "CombatBundle"},
            {ItemID.AncientNelVarianSword, "CombatBundle"},
            {ItemID.AncientWithergateSword, "CombatBundle"},
            
            {ItemID.ManaPotion, "AlchemyBundle"},
            {ItemID.HealthPotion, "AlchemyBundle"},
            {ItemID.AttackPotion, "AlchemyBundle"},
            {ItemID.SpeedPotion, "AlchemyBundle"},
            {ItemID.DefensePotion, "AlchemyBundle"},
            {ItemID.AdvancedAttackPotion, "AlchemyBundle"},
            {ItemID.AdvancedDefensePotion, "AlchemyBundle"},
            {ItemID.AdvancedSpellDamagePotion, "AlchemyBundle"},
            {ItemID.IncredibleSpellDamagePotion, "AlchemyBundle"},
            {ItemID.IncredibleAttackPotion, "AlchemyBundle"},
            {ItemID.IncredibleDefensePotion, "AlchemyBundle"},
            
            {ItemID.PetrifiedLog, "ExplorationBundle"},
            {ItemID.PhoenixFeather, "ExplorationBundle"},
            {ItemID.FairyWings, "ExplorationBundle"},
            {ItemID.GriffonEgg, "ExplorationBundle"},
            {ItemID.ManaSap, "ExplorationBundle"},
            {ItemID.PumiceStone, "ExplorationBundle"},
            {ItemID.MysteriousAntler, "ExplorationBundle"},
            {ItemID.DragonFang, "ExplorationBundle"},
            {ItemID.MonsterCandy, "ExplorationBundle"},
            {ItemID.UnicornHairTuft, "ExplorationBundle"},
            
            {ItemID.KrakenKale, "WithergateFarmingBundle"},
            {ItemID.Tombmelon, "WithergateFarmingBundle"},
            {ItemID.Suckerstem, "WithergateFarmingBundle"},
            {ItemID.Razorstalk, "WithergateFarmingBundle"},
            {ItemID.SnappyPlant, "WithergateFarmingBundle"},
            {ItemID.Moonplant, "WithergateFarmingBundle"},
            {ItemID.Eggplant, "WithergateFarmingBundle"},
            {ItemID.DemonOrb, "WithergateFarmingBundle"},
            
            {ItemID.Acorn, "NelvariFarmingBundle"},
            {ItemID.RockFruit, "NelvariFarmingBundle"},
            {ItemID.WaterFruit, "NelvariFarmingBundle"},
            {ItemID.FireFruit, "NelvariFarmingBundle"},
            {ItemID.WalkChoy, "NelvariFarmingBundle"},
            {ItemID.WindChime, "NelvariFarmingBundle"},
            {ItemID.ShiiwalkiMushroom, "NelvariFarmingBundle"},
            {ItemID.DragonFruit, "NelvariFarmingBundle"},
            {ItemID.ManaGem, "NelvariFarmingBundle"},
            {ItemID.CatTail, "NelvariFarmingBundle"},
            {ItemID.Indiglow, "NelvariFarmingBundle"},
            
        };

        public static Dictionary<int, string> AltarRequirements = new()
        {
            {ItemID.Stone, "DynusAltarMining"},
            {ItemID.Coal, "DynusAltarMining"},
            {ItemID.CopperOre, "DynusAltarMining"},
            {ItemID.Sapphire, "DynusAltarMining"},
            {ItemID.Ruby, "DynusAltarMining"},
            {ItemID.Amethyst, "DynusAltarMining"},
            {ItemID.Diamond, "DynusAltarMining"},
            {ItemID.Havenite, "DynusAltarMining"},
            
            {ItemID.Dorado, "DynusAltarFishing"},
            {ItemID.Duorado, "DynusAltarFishing"},
            {ItemID.Crab, "DynusAltarFishing"},
            {ItemID.SeaBass, "DynusAltarFishing"},
            {ItemID.GoldFish, "DynusAltarFishing"},
            {ItemID.BonemouthBass, "DynusAltarFishing"},
            {ItemID.Chromafin, "DynusAltarFishing"},
            {ItemID.GoldenCarp, "DynusAltarFishing"},
            {ItemID.Flamefish, "DynusAltarFishing"},
            {ItemID.Purrmaid, "DynusAltarFishing"},
            {ItemID.CrystalTetra, "DynusAltarFishing"},
            {ItemID.SkyRay, "DynusAltarFishing"},
            
            {ItemID.Log, "DynusAltarForaging"},
            {ItemID.FireCrystal, "DynusAltarForaging"},
            {ItemID.EarthCrystal, "DynusAltarForaging"},
            {ItemID.WaterCrystal, "DynusAltarForaging"},
            {ItemID.SandDollar, "DynusAltarForaging"},
            
            {ItemID.Cheesecake, "DynusAltarCooking"},
            {ItemID.SpicyRamen, "DynusAltarCooking"},
            {ItemID.SesameRiceBall, "DynusAltarCooking"},
            {ItemID.Pizza, "DynusAltarCooking"},
            {ItemID.Cookies, "DynusAltarCooking"},
            {ItemID.Coffee, "DynusAltarCooking"},
            {ItemID.TomatoSoup, "DynusAltarCooking"},
            {ItemID.ShimmerootTreat, "DynusAltarCooking"},
            {ItemID.EnergySmoothie, "DynusAltarCooking"},
            
            {ItemID.GoldBar, "DynusAltarGold"},
            
            {ItemID.Wheat, "DynusAltarFarming"},
            {ItemID.Corn, "DynusAltarFarming"},
            {ItemID.Potato, "DynusAltarFarming"},
            {ItemID.Tomato, "DynusAltarFarming"},
            {ItemID.Carrot, "DynusAltarFarming"},
            {ItemID.SugarCane, "DynusAltarFarming"},
            {ItemID.Onion, "DynusAltarFarming"},
            {ItemID.Greenroot, "DynusAltarFarming"},
            {ItemID.HoneyFlower, "DynusAltarFarming"},
            {ItemID.Rice, "DynusAltarFarming"},
            
            {ItemID.Raspberry, "DynusAltarFruit"},
            {ItemID.Peach, "DynusAltarFruit"},
            {ItemID.Orange, "DynusAltarFruit"},
            {ItemID.Blueberry, "DynusAltarFruit"},
            {ItemID.Berry, "DynusAltarFruit"},
            {ItemID.Apple, "DynusAltarFruit"},
            
            {ItemID.AdventureKeepsake, "DynusAltarRareItems"},
            {ItemID.RichesKeepsake, "DynusAltarRareItems"},
            {ItemID.RomanceKeepsake, "DynusAltarRareItems"},
            {ItemID.PeaceKeepsake, "DynusAltarRareItems"},
            
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
                    if (MuseumRequirements.TryGetValue(__instance.id, out string where) && !SingletonBehaviour<GameSave>.Instance.GetProgressBoolWorld(GetProgressKey(BundleType.MuseumBundle, where, __instance.id)))
                    {
                        string amounts = SingletonBehaviour<GameSave>.Instance.GetProgressStringWorld(GetProgressKey(BundleType.MuseumBundle, where, __instance.id) + "_string");
                        
                        if (amounts.Length > 0)
                        {
                            __result = __result + "\n<color=#ed77f8><size=65%>Required: " + BundleNames[where] + " ("+ amounts + ")</size></color>";
                        }
                        else
                        {
                            __result = __result + "\n<color=#ed77f8><size=65%>Required: " + BundleNames[where] + "</size></color>";
                        }
                        
                    }
                    
                    if (AltarRequirements.TryGetValue(__instance.id, out string where2) && !SingletonBehaviour<GameSave>.Instance.GetProgressBoolWorld(GetProgressKey(BundleType.DynusAltar, where2, __instance.id)))
                    {
                        string amounts = SingletonBehaviour<GameSave>.Instance.GetProgressStringWorld(GetProgressKey(BundleType.DynusAltar, where2, __instance.id) + "_string");
                        
                        if (amounts.Length > 0)
                        {
                            __result = __result + "\n<color=#ed77f8><size=65%>Required: " + BundleNames[where2] + " ("+ amounts + ")</size></color>";
                        }
                        else
                        {
                            __result = __result + "\n<color=#ed77f8><size=65%>Required: " + BundleNames[where2] + "</size></color>";
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
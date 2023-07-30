using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Morthy.Util;
using UnityEngine;
using Wish;
using fastJSON;
using Object = UnityEngine.Object;

namespace CustomItems;

public class CustomItems
{
    private static readonly string[] RequiredItemKeys = 
    {
        "name",
        "description",
        "useDescription",
        "helpDescription",
        "stackSize",
        "goldSellPrice",
        "orbSellPrice",
        "ticketSellPrice",
        "category",
        "rarity",
    };

    public static Dictionary<string, List<ShopItemInfo>> MerchantAdditions = new ();
    private static RecipeList[] _recipeLists = { };

    public static void AddItems()
    {
        MerchantAdditions.Clear();
        
        foreach (var path in GetItemDefinitionPaths(""))
        {
            try
            {
                var definition = new ItemDefinition();
                JSON.FillObject(definition, File.ReadAllText(path));

                var folder = Path.GetDirectoryName(path);

                if (definition.type == "decoration")
                {
                    CustomFurniture.CreateFurniture(folder, definition);
                }
                else
                {
                    CreateItem(folder, definition);
                }

            }
            catch (Exception e)
            {
                Plugin.logger.LogError(e);
            }
        }

        ItemDatabase.GetItemData(30019).useItem = new GameObject("Scrubber Useable").AddComponent<DecorationScrubber>();
        Object.DontDestroyOnLoad(ItemDatabase.GetItemData(30019).useItem);
    }

    public static void AddItemsToShops()
    {
        foreach (var merchantAddition in MerchantAdditions)
        {
            foreach (var info in merchantAddition.Value)
            {
                var table = SingletonBehaviour<SaleManager>.Instance.merchantTables.FirstOrDefault(t => t.name.Equals(merchantAddition.Key));

                if (!table)
                {
                    Plugin.logger.LogError($"Could not find merchant table with name ${merchantAddition.Key}");
                    return;
                }
                
                if (table.startingItems.Exists(i => i.item.id == info.item.id))
                {
                    return;
                }
                
                table.startingItems.Add(info);
            }
        }
    }

    private static void RegisterShopAddition(ItemData item, ShopItemDefinition definition)
    {
        if (!MerchantAdditions.ContainsKey(definition.shop))
        {
            MerchantAdditions.Add(definition.shop, new List<ShopItemInfo>());
        }

        var info = new ShopItemInfo()
        {
            amount = definition.amount,
            item = item,
            price = definition.costGold,
            orbs = definition.costOrbs,
            tickets = definition.costTickets,
            characterProgressIDs = new List<Progress>(),
            worldProgressIDs = new List<Progress>(),
        };
        
        MerchantAdditions[definition.shop].Add(info);
        Plugin.logger.LogDebug($"Registered {item.name} to be added to {definition.shop}");
    }
    
    private static void ValidateItemData(ItemDefinition data)
    {

        if (data.id is < 1 or > 65534)
        {
            throw new Exception("Item field \"id\" must be present, an integer, and between 1 and 65534");
        }

        foreach (var requiredKey in RequiredItemKeys)
        {
            if (data.GetType().GetField(requiredKey) == null)
            {
                throw new Exception($"Item field \"{requiredKey}\" must be present for item {data.id}.");
            }
        }
    }

    public static ItemData CreateItem(string folder, ItemDefinition data)
    {
        var item = ScriptableObject.CreateInstance<ItemData>();

        ValidateItemData(data);

        item.id = data.id;
        item.name = data.name;
        item.description = data.description;
        item.useDescription = data.useDescription;
        item.helpDescription = data.helpDescription;
        item.stackSize = data.stackSize;
        item.canSell = data.canSell;
        item.canTrade = data.canTrade;
        item.canTrash = data.canTrash;
        item.sellPrice = data.goldSellPrice;
        item.orbsSellPrice = data.orbSellPrice;
        item.ticketSellPrice = data.ticketSellPrice;
        item.category = (ItemCategory)data.category;
        item.rarity = (ItemRarity)data.rarity;
        
        var itemId = item.id;

        if (ItemDatabase.GetItemData(itemId) && !ItemDatabase.GetItemData(itemId).name.Equals(item.name))
        {
            throw new Exception($"Failed to create modded item with ID {itemId} because an existing item exists with that ID.");
        }
        
        var iconPath = GetFilePath(Path.Combine(folder, $"{itemId}.png"));
        if (!File.Exists(iconPath))
        {
            throw new Exception($"Failed to create modded item with ID {itemId} because {iconPath} does not exist.");
        }
        
        item.icon = SpriteUtil.CreateSprite(File.ReadAllBytes(iconPath), $"Modded item icon {itemId}");
        
        Plugin.logger.LogDebug($"Created custom item {itemId} with name {item.name}");
        ItemDatabase.items[item.id] = item;
        ItemDatabase.ids[item.name.RemoveWhitespace().ToLower()] = item.id;

        if (data.shopEntries is not null)
        {
            foreach (var entry in data.shopEntries)
            {
                RegisterShopAddition(item, entry);
            }
        }

        if (data.recipes is not null)
        {
            foreach (var entry in data.recipes)
            {
                AddItemToRecipeList(item, entry);
            }
        }
        
        return item;
    }

    public static string GetFilePath(string path)
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), path);
    }
    
    private static IEnumerable<string> GetItemDefinitionPaths(string folder)
    {
        return Directory.GetFiles(GetFilePath(folder), "*.json", SearchOption.AllDirectories);
    }
    
    private static void AddItemToRecipeList(ItemData item, RecipeDefinition recipeDefinition)
    {
        if (_recipeLists.Length == 0)
        {
            _recipeLists = Resources.FindObjectsOfTypeAll<RecipeList>();
        }

        foreach (var rl in _recipeLists)
        {
            if (!rl.name.Equals(recipeDefinition.list)) continue;
            
            if (rl.craftingRecipes.Any(r => r.output.item.id == item.id))
            {
                return;
            }
            
            var recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.output = new ItemInfo { item = item, amount = 1 };
            recipe.input = recipeDefinition.inputs.Select(inputDefinition => new ItemInfo() { item = ItemDatabase.GetItemData(inputDefinition.id), amount = inputDefinition.amount }).ToList();
            recipe.worldProgressTokens = new List<Progress>();
            recipe.characterProgressTokens = new List<Progress>();
            recipe.questProgressTokens = new List<QuestAsset>();
            recipe.hoursToCraft = recipeDefinition.hours;
                
            rl.craftingRecipes.Add(recipe);
            Plugin.logger.LogDebug($"Added item {item.id} to {recipeDefinition.list}");
        }
    }

    public static void RemoveNonExistingDecorations(GameSave save)
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        var remove = new Dictionary<Vector3Int, short>();
        
        foreach (var scene in save.CurrentWorld.decorations)
        {
            foreach (var decoration in scene.Value.Where(decoration => ItemDatabase.GetItemData(decoration.Value.id) == null))
            {
                if (decoration.Value.id > 0)
                {
                    Plugin.logger.LogDebug($"Removing decoration with ID {decoration.Value.id} from scene {scene.Key}");
                    remove.Add(new Vector3Int(decoration.Value.x, decoration.Value.y, decoration.Value.z), scene.Key);
                }
                else
                {
                    //Plugin.logger.LogDebug($"Found decoration with ID {decoration.Value.id} from scene {scene.Key}");
                }
            }
        }

        foreach (var dec in remove)
        {
            save.RemoveDecorationFromSave(dec.Key, dec.Value);
        }
        
        Plugin.logger.LogInfo($"Checked for defunct decorations in {watch.ElapsedMilliseconds}ms");
    }
}
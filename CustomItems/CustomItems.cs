using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Morthy.Util;
using UnityEngine;
using Wish;
using HarmonyLib;
using PSS;
using TinyJson;
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

    private static Dictionary<int, ItemData> ItemDB = new();
    private static Dictionary<string, int> ItemNames = new();

    public static Dictionary<string, List<ShopItemInfo2>> MerchantAdditions = new ();
    public static Dictionary<string, Dictionary<int, RecipeDefinition>> RecipeAdditions = new();

    public static bool HasCustomItemWithID(int id)
    {
        return ItemDB.ContainsKey(id);
    }

    public static int? IDForCustomItemName(string name)
    {
        if (ItemNames.TryGetValue(name, out int r))
        {
            return r;
        }

        return null;
    }
    
    public static ItemData GetCustomItem(int id)
    {
        return ItemDB[id];
    }
    
    public static void AddItems()
    {
        MerchantAdditions.Clear();
        
        foreach (var path in GetItemDefinitionPaths())
        {
            try
            {
                var json = File.ReadAllText(path);
                var definition = json.FromJson<ItemDefinition>();

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
        
        GetCustomItem(61000).useItem = new GameObject("Scrubber Useable").AddComponent<DecorationScrubber>();
        Object.DontDestroyOnLoad(GetCustomItem(61000).useItem);
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
                
                if (table.startingItems2.Exists(i => i.id == info.id))
                {
                    return;
                }
                
                table.startingItems2.Add(info);
            }
        }
    }

    private static void RegisterShopAddition(ItemData item, ShopItemDefinition definition)
    {
        if (!MerchantAdditions.ContainsKey(definition.shop))
        {
            MerchantAdditions.Add(definition.shop, new List<ShopItemInfo2>());
        }

        var info = new ShopItemInfo2()
        {
            amount = definition.amount,
            id = item.id,
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

    private static bool IsExistingInternalItem(int itemId)
    {
        var ids = Traverse.Create(Database.Instance).Field("validIDs").GetValue<HashSet<int>>();

        return ids.Contains(itemId);
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

        if (IsExistingInternalItem(item.id))
        {
            throw new Exception($"Failed to create modded item with ID {itemId} because the ID is in use by Sun Haven itself.");
        }

        if (HasCustomItemWithID(item.id) && !GetCustomItem(itemId).name.Equals(item.name))
        {
            throw new Exception($"Failed to create modded item with ID {itemId} because an existing modded item exists with that ID.");
        }
        
        var iconPath = GetFilePath(Path.Combine(folder, data.image ?? $"{itemId}.png"));
        if (!File.Exists(iconPath))
        {
            throw new Exception($"Failed to create modded item with ID {itemId} because {iconPath} does not exist.");
        }

        item.icon = SpriteUtil.CreateSprite(File.ReadAllBytes(iconPath), $"Modded item icon {itemId}");
        ItemDB[item.id] = item;
        ItemNames[item.name.RemoveWhitespace().ToLower()] = item.id;
        SingletonBehaviour<ItemInfoDatabase>.Instance.allItemSellInfos[item.id] = new ItemSellInfo()
        {
            name = item.name,
            keyName = item.name,
            sellPrice = item.sellPrice,
            orbSellPrice = item.orbsSellPrice,
            ticketSellPrice = item.ticketSellPrice,
            stackSize = item.stackSize,
            isMeal = false,
            rarity = item.rarity,
            decorationType = item.decorationType,
        };

        Plugin.logger.LogDebug($"Created custom item {itemId} with name {item.name}");

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
                if (!RecipeAdditions.ContainsKey(entry.list))
                {
                    RecipeAdditions[entry.list] = new Dictionary<int, RecipeDefinition>();
                }

                RecipeAdditions[entry.list][item.id] = entry;
                Plugin.logger.LogDebug($"Registered {item.name} to be added to crafting table {entry.list}");
                
            }
        }
        
        return item;
    }

    public static string GetFilePath(string path)
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), path);
    }
    
    private static IEnumerable<string> GetItemDefinitionPaths()
    {
        var customItemsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        var one = Directory.GetFiles(customItemsFolder!, "*.json", SearchOption.AllDirectories);
        var two  = Directory.GetFiles(Path.Combine(customItemsFolder, ".."), "*.item.json", SearchOption.AllDirectories);
        return one.Concat(two).ToArray();
    }

    public static void AddItemsToRecipeList(CraftingTable table)
    {
        var recipeList = Traverse.Create(table).Field("recipeList").GetValue<RecipeList>();
        
        if (recipeList == null || !RecipeAdditions.ContainsKey(recipeList.name))
        {
            return;
        }

        foreach (var entry in RecipeAdditions[recipeList.name])
        {
            if (recipeList.craftingRecipes.Any(r => r.output2.id == entry.Key))
            {
                continue;
            }
            
            foreach (var inputDefinition in entry.Value.inputs)
            {
                Database.GetData<ItemData>(inputDefinition.id, null);
            }
            
            var recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.worldProgressTokens = new List<Progress>();
            recipe.characterProgressTokens = new List<Progress>();
            recipe.questProgressTokens = new List<QuestAsset>();
            recipe.hoursToCraft = entry.Value.hours;
            
            Database.GetData<ItemData>(entry.Key, data =>
            {
                recipe.output2 = new SerializedItemDataNamedAmount() { id = entry.Key, name = data.name, amount = 1 };
            });

            recipe.input2 = new List<SerializedItemDataNamedAmount>();

            foreach (var inputDefinition in entry.Value.inputs)
            {
                Database.GetData<ItemData>(inputDefinition.id, data =>
                {
                    recipe.input2.Add(new SerializedItemDataNamedAmount() { id = inputDefinition.id, name = data.name, amount = inputDefinition.amount });
                });
            }

            recipeList.craftingRecipes.Add(recipe);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using Morthy.Util;
using UnityEngine;
using Wish;
using HarmonyLib;
using JetBrains.Annotations;
using PSS;
using TinyJson;
using Object = UnityEngine.Object;

namespace CustomItems;

public class CustomItems
{
    public static Action OnCustomItemsAdded;
    
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

    public static Dictionary<string, List<ShopItemInfo2>> MerchantAdditions = new ();
    public static Dictionary<string, Dictionary<int, RecipeDefinition>> RecipeAdditions = new();

    public static bool HasCustomItemWithID(int id)
    {
        return ItemDB.ContainsKey(id);
    }

    public static ItemData GetCustomItem(int id)
    {
        return ItemDB[id];
    }

    public static void TryAddItems()
    {
        if (CustomFurniture.ExampleFurniture && CustomFurniture.ExampleChest)
        {
            AddItems();
        }
    }
    
    public static void AddItems()
    {
        MerchantAdditions.Clear();
        RecipeAdditions.Clear();
        ItemDB.Clear();
        
        foreach (var path in GetItemDefinitionPaths())
        {
            try
            {
                var json = File.ReadAllText(path);
                
                // backwards compatibility
                json = json.Replace("\n", string.Empty).Replace("\r", string.Empty);

                if (json.EndsWith("],}"))
                {
                    json = json.Substring(0, json.Length - 3) + "]}";
                    Plugin.logger.LogWarning($"This JSON file at {path} is invalid (the trailing , on the penultimate line). CustomItems is loading the item anyway, but this may cease to work in the future.");
                }

                var definition = json.FromJson<ItemDefinition>();

                var folder = Path.GetDirectoryName(path);

                if (definition.type == "decoration")
                {
                    CustomFurniture.CreateFurniture(folder, definition);
                }
                else if (!definition.type.IsNullOrWhiteSpace())
                {
                    CreateItem(folder, definition, definition.type);
                }
                else
                {
                    CreateItem(folder, definition, null);
                }

            }
            catch (Exception e)
            {
                Plugin.logger.LogError($"An error occurred creating the item from the JSON file at {path}: {e}");
            }
        }
        
        GetCustomItem(61000).useItem = new GameObject("Scrubber Useable").AddComponent<DecorationScrubber>();
        Object.DontDestroyOnLoad(GetCustomItem(61000).useItem);
        
        OnCustomItemsAdded?.Invoke();
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
                    continue;
                }
                
                if (table.startingItems2.Exists(i => i.id == info.id))
                {
                    continue;
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

        if (definition.costItemId != 0)
        {
            Database.GetData<ItemData>(definition.costItemId, data => info.itemToUseAsCurrency = data );
        }
        
        MerchantAdditions[definition.shop].Add(info);
        Plugin.logger.LogInfo($"Registered {item.name} to be added to {definition.shop}");
    }
    
    private static void ValidateItemData(ItemDefinition data)
    {
        if (data.id is < 1 or > 65534)
        {
            throw new Exception("Item field \"id\" must be present, an integer, and between 1 and 65534. If your ID field is present and fine, this likely means your JSON has a formatting error somewhere.");
        }

        foreach (var requiredKey in RequiredItemKeys)
        {
            if (data.GetType().GetField(requiredKey) == null)
            {
                throw new Exception($"Item field \"{requiredKey}\" must be present for item {data.id}.");
            }
        }
    }

    private static bool IsExistingInternalItem(int itemId, string customName)
    {
        return ItemInfoDatabase.Instance.allItemSellInfos.ContainsKey(itemId) && !ItemInfoDatabase.Instance.allItemSellInfos[itemId].name.Equals(customName);
    }

    public static ItemData CreateItem(string folder, ItemDefinition data, [CanBeNull] string useItemType)
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

        if (IsExistingInternalItem(item.id, item.name))
        {
            throw new Exception($"Failed to create modded item with ID {itemId} because the ID is in use by another item.");
        }

        var iconPath = GetFilePath(Path.Combine(folder, data.image ?? $"{itemId}.png"));
        if (!File.Exists(iconPath))
        {
            throw new Exception($"Failed to create modded item with ID {itemId} because {iconPath} does not exist.");
        }

        item.icon = SpriteUtil.CreateSprite(File.ReadAllBytes(iconPath), $"Modded item icon {itemId}");

        var lowerName = item.name.RemoveWhitespace().ToLower();

        if (Database.Instance.ids.ContainsKey(lowerName) && Database.Instance.ids[lowerName] != item.id)
        {
            Plugin.logger.LogWarning($"Custom item {itemId}: Another item exists with the name \"{lowerName}\", so you won't be able to add this item via the /additem command. Consider renaming your custom item.");
        }
        else
        {
            Database.Instance.ids[lowerName] = item.id;
        }

        Database.Instance.validIDs.Add(item.id);
        Database.Instance.types[item.id] = typeof(ItemData);

        var node = Database.Instance.lruList.AddFirst(new Database.CacheItem(item.id, item));

        if (!Database.Instance.cache.ContainsKey(item.GetType()))
        {
            Database.Instance.cache[item.GetType()] = new Dictionary<object, LinkedListNode<Database.CacheItem>>();
        }
        
        Database.Instance.cache[item.GetType()][item.id] = node;
   
        ItemDB[item.id] = item;
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

        Plugin.logger.LogInfo($"Created custom item {itemId} with name {item.name}");

        if (useItemType != null)
        {
            AddCustomUseType(data, useItemType, item);
        }

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
                Plugin.logger.LogInfo($"Registered {item.name} to be added to crafting table {entry.list}");
            }
        }
        
        return item;
    }

    private static void AddCustomUseType(ItemDefinition data, string useItemType, ItemData item)
    {
        var useItemTypeType = Type.GetType(useItemType);

        if (useItemTypeType == null)
        {
            throw new Exception($"Failed to find type {useItemType}");
        }

        item.useItem = (UseItem)new GameObject($"{item.name} useitem").AddComponent(useItemTypeType);
        Object.DontDestroyOnLoad(item.useItem);
        
        if (data.useItemProps == null) return;
        
        var traverse = Traverse.Create(item.useItem);

        foreach (var dataUseItemProp in data.useItemProps)
        {
            if (dataUseItemProp.Value is Dictionary<string, object> dataInfo && dataInfo.TryGetValue("type", out var type) && dataInfo.TryGetValue("args", out var args))
            {
                if (type is not string typeString)
                {
                    throw new Exception("Type must be a string");
                }

                if (args is not List<object> argsList)
                {
                    throw new Exception("Args must be an array");
                }

                var typeType = Type.GetType(typeString);

                if (typeType == null)
                {
                    throw new Exception($"Failed to find type {type}");
                }

                argsList = argsList.Select(o => o is double ? Convert.ToSingle(o) : o).ToList();
                var x = Activator.CreateInstance(typeType, argsList.ToArray());
                traverse.Field(dataUseItemProp.Key).SetValue(x);
            }
            else
            {
                traverse.Field(dataUseItemProp.Key).SetValue(dataUseItemProp.Value is double ? Convert.ToSingle(dataUseItemProp.Value) : dataUseItemProp.Value);
            }
        }
    }

    public static string GetFilePath(string path)
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), path);
    }
    
    private static IEnumerable<string> GetItemDefinitionPaths()
    {
        var customItemsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        var one = Directory.GetFiles(customItemsFolder!, "*.json", SearchOption.AllDirectories);
        var two  = Directory.GetFiles(BepInEx.Paths.PluginPath, "*.item.json", SearchOption.AllDirectories);
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
                recipe.output2 = new SerializedItemDataNamedAmount() { id = entry.Key, name = data.name, amount = entry.Value.amount == 0 ? 1 : entry.Value.amount };
            });

            recipe.input2 = new List<SerializedItemDataNamedAmount>();
            var tmpItems = new Dictionary<int, ItemData>();

            foreach (var inputDefinition in entry.Value.inputs)
            {
                Database.GetData<ItemData>(inputDefinition.id, data =>
                {
                    tmpItems[inputDefinition.id] = data;

                    if (tmpItems.Count != entry.Value.inputs.Count) return;
                    
                    foreach (var inputDefinition in entry.Value.inputs)
                    {
                        recipe.input2.Add(new SerializedItemDataNamedAmount() { id = inputDefinition.id, name = tmpItems[inputDefinition.id].name, amount = inputDefinition.amount == 0 ? 1 : inputDefinition.amount });
                    }
                });
            }

            recipeList.craftingRecipes.Add(recipe);
        }
    }
}
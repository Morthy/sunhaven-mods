using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Morthy.Util;
using UnityEngine;
using Wish;
using Object = UnityEngine.Object;

namespace Sprinklers;

public static class ItemHandler
{
    private const int BaseSprinklerId = 10535;
    
    private const int SmallSprinklerId = 30011;
    private const int LargeSprinklerId = 30012;
    private const int NelvariSprinklerId = 30013;
    private const int WithergateSprinklerId = 30014;
    
    private static void CreateSprinklerItem(int id, int range)
    {
        var original = ItemDatabase.GetItemData(BaseSprinklerId);

        if (!original)
        {
            Plugin.logger.LogError("Cannot create modded Sprinkler because the original sprinkler no longer exists.");
            return;
        }

        var item = ScriptableObject.CreateInstance<ItemData>();
        JsonUtility.FromJsonOverwrite(FileLoader.LoadFile(Assembly.GetExecutingAssembly(), $"data.{id}.json"), item);
        
        if (ItemDatabase.GetItemData(id) && !ItemDatabase.GetItemData(id).name.Equals(item.name))
        {
            Plugin.logger.LogError($"Cannot create modded item with ID {id} because it is already in use by a different item.");
            return;
        }

        
        item.icon = SpriteUtil.CreateSprite(FileLoader.LoadFileBytes(Assembly.GetExecutingAssembly(), $"img.{id}.png"), $"Modded item icon {id}");

        var useItem = Object.Instantiate(original.useItem) as Placeable;

        if (!useItem)
        {
            Plugin.logger.LogError("Original sprinkler has no useItem");
            return;
        }
        
        Object.Destroy(useItem.GetComponent<PlaceableSpriteLoader>());
        useItem._previewSprite = SpriteUtil.CreateSpriteDecoration(FileLoader.LoadFileBytes(Assembly.GetExecutingAssembly(), $"img.{id}.png"), $"Modded item decoration {id}");
        useItem.previewOffset = new Vector2(-0.08f, 0);
        useItem._itemData = item;

        var p = 0.5f - (1f / (range * 2 + 1) / 2);
        useItem._secondaryPreviewSprite = SpriteUtil.CreateSprite(FileLoader.LoadFileBytes(Assembly.GetExecutingAssembly(), $"preview_{range}.png"), new Vector2(p, p), $"Modded item secondary preview {id}");
        
        item.useItem = useItem;

        var sprinkler = Object.Instantiate(useItem._decoration) as Sprinkler;

        if (!sprinkler)
        {
            Plugin.logger.LogError("Original sprinkler has no decoration of type Sprinkler");
            return;
        }

        var go = sprinkler.gameObject;
        Object.Destroy(go.GetComponent<Sprinkler>());
        var customSprinkler = go.AddComponent<CustomSprinkler>();
        useItem._decoration = customSprinkler;
        customSprinkler.range = range;
        
        var graphics = sprinkler.transform.Find("Graphics");

        Object.Destroy(graphics.gameObject.GetComponent<MeshGenSpriteLoader>());
        var mg = graphics.gameObject.GetComponent<MeshGenerator>();
        mg.sprite = useItem._previewSprite;
        mg.SetDefault();

        graphics.gameObject.GetComponent<MeshRenderer>().enabled = true;
        useItem.gameObject.SetActive(false);
        
        Object.DontDestroyOnLoad(useItem);
        Object.DontDestroyOnLoad(sprinkler);

        ItemDatabase.itemDatas[item.id] = item;
        ItemDatabase.ids[item.name.RemoveWhitespace().ToLower()] = item.id;
        Plugin.logger.LogDebug($"Created item {item.id} with name {item.name}");
    }

    private static void AddItemToRecipeList(int id, string recipeList, List<ItemInfo> input)
    {
        
        foreach (var rl in Resources.FindObjectsOfTypeAll<RecipeList>())
        {
            if (!rl.name.Equals(recipeList)) continue;
            
            if (rl.craftingRecipes.Any(r => r.output.item.id == id))
            {
                return;
            }
                
            var recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.output = new ItemInfo { item = ItemDatabase.GetItemData(id), amount = 1 };
            recipe.input = input;
            recipe.worldProgressTokens = new List<Progress>();
            recipe.characterProgressTokens = new List<Progress>();
            recipe.questProgressTokens = new List<QuestAsset>();
            recipe.hoursToCraft = 1f;
                
            rl.craftingRecipes.Add(recipe);
            Plugin.logger.LogDebug($"Added item {id} to {recipeList}");
        }
    }

    public static void CreateSprinklerItems()
    {
        CreateSprinklerItem(SmallSprinklerId, 1);
        CreateSprinklerItem(LargeSprinklerId, 2);
        CreateSprinklerItem(NelvariSprinklerId, 3);
        CreateSprinklerItem(WithergateSprinklerId, 4);
        
        AddItemToRecipeList(30011, "RecipeList_Anvil", new List<ItemInfo>
        {
            new() { item = ItemDatabase.GetItemData(ItemID.GoldBar), amount = 5 },
            new() { item = ItemDatabase.GetItemData(ItemID.WaterCrystal), amount = 1 }
        });
        
        AddItemToRecipeList(30012, "RecipeList_Anvil", new List<ItemInfo>
        {
            new() { item = ItemDatabase.GetItemData(ItemID.SuniteBar), amount = 5 },
            new() { item = ItemDatabase.GetItemData(ItemID.Havenite), amount = 1 }
        });
        
        AddItemToRecipeList(30013, "RecipeList_Mana Anvil", new List<ItemInfo>
        {
            new() { item = ItemDatabase.GetItemData(ItemID.EnchantedMithrilBar), amount = 5 },
            new() { item = ItemDatabase.GetItemData(ItemID.WaterRune), amount = 1 }
        });
        
        AddItemToRecipeList(30014, "RecipeList_Monster Anvil", new List<ItemInfo>
        {
            new() { item = ItemDatabase.GetItemData(ItemID.EnhancedMithrilBar), amount = 5 },
            new() { item = ItemDatabase.GetItemData(ItemID.RefinedGlass), amount = 1 }
        });
    }
}
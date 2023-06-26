using System;
using System.IO;
using JetBrains.Annotations;
using Morthy.Util;
using UnityEngine;
using Wish;
using Object = UnityEngine.Object;

namespace CustomItems;

public class CustomFurniture
{
    private static readonly string[] RequiredDecorationKeys =
    {
        "size",
        "placeableOnTables",
        "placeableOnWalls",
        "placeableAsRug",
    };
    
    
    private static void ValidateItemData(ItemDefinition definition)
    {
        if (definition.decoration == null)
        {
            throw new Exception("Item field \"decoration\" must be present");
        }

        var furnitureData = definition.decoration;

        foreach (var requiredKey in RequiredDecorationKeys)
        {
            if (furnitureData.GetType().GetField(requiredKey) == null)
            {
                throw new Exception($"Decoration field \"{requiredKey}\" must be present.");
            }
        }
    }

    private static Type DecideComponent(string functionality)
    {
        return functionality switch
        {
            null => typeof(Decoration),
            "bed" => typeof(Bed),
            "table" => typeof(Table),
            _ => typeof(Decoration)
        };
    }

    private static void SetVector2IntFromDefinition(ref Vector2Int value, [CanBeNull] int[] definition)
    {
        if (definition is null || definition.Length != 2)
        {
            return;
        }

        value = new Vector2Int(definition[0], definition[1]);
    }
    
    private static void SetVector2FromDefinition(ref Vector2 value, [CanBeNull] float[] definition)
    {
        if (definition is null || definition.Length != 2)
        {
            return;
        }

        value = new Vector2(definition[0], definition[1]);
    }
    
    public static ItemData CreateFurniture(string folder, ItemDefinition definition)
    {
        ValidateItemData(definition);
        
        var item = CustomItems.CreateItem(folder, definition);
        var furnitureData = definition.decoration;
        
        var useItem = new GameObject(item.name + " Placeable").AddComponent<Placeable>();
        useItem.gameObject.SetActive(false);
        useItem._previewSprite = SpriteUtil.CreateSpriteDecoration(File.ReadAllBytes(Path.Combine(folder, $"{item.id}.png")), $"{item.name} Decoration");
        useItem._itemData = item;
        Object.DontDestroyOnLoad(useItem);

        var decoration = (Decoration)new GameObject(item.name + " Decoration").AddComponent(DecideComponent(furnitureData.functionality));
        decoration.gameObject.SetActive(false);
        SetVector2IntFromDefinition(ref decoration.size, furnitureData.size);
        decoration.placementSize = decoration.size;
        // todo: try actually really figure out what placementSize does
        // SetVector2IntFromDefinition(ref decoration.placementSize, furnitureData.placementSize); 
        decoration.placeableOnTables = furnitureData.placeableOnTables;
        decoration.placeableOnWalls = furnitureData.placeableOnWalls;
        decoration.placeableAsRug = furnitureData.placeableAsRug;
        decoration.placeable = useItem;
        Object.DontDestroyOnLoad(decoration);

        switch (decoration)
        {
            case Table table:
                SetVector2IntFromDefinition(ref table.tableOffset, furnitureData.tableOffset);
                SetVector2IntFromDefinition(ref table.tableSize, furnitureData.tableSize);
                break;
            case Bed bed:
                SetVector2FromDefinition(ref bed.bedSleepOffset, furnitureData.bedSleepOffset);
                break;
        }

        var graphics = new GameObject("Graphics");
        graphics.transform.SetParent(decoration.transform);
        graphics.AddComponent<MeshRenderer>();
        graphics.AddComponent<MeshFilter>();
        var mg = graphics.AddComponent<MeshGenerator>();
        
        mg.shader = (ItemDatabase.GetItemData(ItemID.CowPlushie).useItem as Placeable)?.Decoration.transform.Find("Graphics").GetComponent<MeshGenerator>().shader;
        mg.sprite = useItem._previewSprite;
        mg.SetDefault();
        if (decoration is Table)
        {
            mg.Table(decoration.size.x-1); // i don't really know how to properly determine this but it kinda works
        }

        var collider = decoration.gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(decoration.size.x / 6f, decoration.size.y / 6f);
        collider.offset = new Vector2(collider.size.x / 2, collider.size.y / 2);
        
        
        useItem._decoration = decoration;
        item.useItem = useItem;
        
        decoration.gameObject.SetActive(true);
        
        return item;
    }
}
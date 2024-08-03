using System;
using System.IO;
using HarmonyLib;
using I2.Loc;
using JetBrains.Annotations;
using Morthy.Util;
using PSS;
using TMPro;
using UnityEngine;
using Wish;
using Object = UnityEngine.Object;

namespace CustomItems;

public class CustomFurniture
{
    public static ItemData ExampleChest;
    public static ItemData ExampleFurniture;
    
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
                throw new Exception($"Decoration field \"{requiredKey}\" must be present for item {definition.id}.");
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
            "chest" => typeof(Chest),
            _ => functionality.Length > 0 ? Type.GetType(functionality) : typeof(Decoration)
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
        
        var item = CustomItems.CreateItem(folder, definition, null);
        var furnitureData = definition.decoration;
        
        var useItem = new GameObject(item.name + " Placeable").AddComponent<Placeable>();
        useItem.gameObject.SetActive(false);
        useItem._previewSprite = SpriteUtil.CreateSpriteDecoration(File.ReadAllBytes(Path.Combine(folder, definition.image ?? $"{item.id}.png")), $"{item.name} Decoration");
        useItem._itemData = item;
        Object.DontDestroyOnLoad(useItem);

        var go = new GameObject(item.name + " Decoration");
        go.SetActive(false);
        
        var decoration = (Decoration)go.AddComponent(DecideComponent(furnitureData.functionality));
        decoration.gameObject.SetActive(false);
        SetVector2IntFromDefinition(ref decoration.size, furnitureData.size);
        decoration.placementSize = decoration.size;

        if (decoration.placementSize != null)
        {
            SetVector2IntFromDefinition(ref decoration.placementSize, furnitureData.placementSize); 
        }
        
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

        if (furnitureData!.offset != null)
        {
            useItem.previewOffset = new Vector2(furnitureData.offset[0]/6f, furnitureData.offset[1]/6f);
            graphics.transform.localPosition = new Vector3(useItem.previewOffset.x, useItem.previewOffset.y, 0);
        }


        graphics.AddComponent<MeshRenderer>();
        graphics.AddComponent<MeshFilter>();
        var mg = graphics.AddComponent<MeshGenerator>();
        mg.shader = (ExampleFurniture.useItem as Placeable)?.Decoration.transform.Find("Graphics").GetComponent<MeshGenerator>().shader;
        mg.sprite = useItem._previewSprite;
        mg.SetDefault();
        if (decoration is Table)
        {
            mg.Table(decoration.size.y*3); // i don't really know how to properly determine this but it kinda works
        }

        if (decoration.placeableAsRug)
        {
            mg.Table(0);
        }
        else if (decoration is Bed && furnitureData.bedSheetImage != null)
        {
            mg.Table(11);
        }

        if (!decoration.placeableAsRug)
        {
            var collider = decoration.gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(decoration.size.x / 6f, decoration.size.y / 6f);
            collider.offset = new Vector2(collider.size.x / 2, collider.size.y / 2);
            collider.isTrigger = decoration is Bed && furnitureData.bedSheetImage != null;
        }

        if (decoration is Bed && furnitureData.bedSheetImage != null)
        {
            var colliderBottom = decoration.gameObject.AddComponent<BoxCollider2D>();
            colliderBottom.size = new Vector2(decoration.size.x / 6f, 2 / 6f);
            colliderBottom.offset = new Vector2(colliderBottom.size.x / 2, 0);
            
            var colliderTop = decoration.gameObject.AddComponent<BoxCollider2D>();
            colliderTop.size = new Vector2(decoration.size.x / 6f, 3 / 6f);
            colliderTop.offset = new Vector2(colliderBottom.size.x / 2, (decoration.size.y+3) / 6f);

            var sheetGraphics = Object.Instantiate(graphics, decoration.transform);
            var sheetMG = sheetGraphics.GetComponent<MeshGenerator>();
            sheetMG.sprite = SpriteUtil.CreateSpriteDecoration(File.ReadAllBytes(Path.Combine(folder, furnitureData.bedSheetImage)), $"{item.name} Decoration Sheet");
            sheetMG.SetDefault();
        }

        useItem._decoration = decoration;
        item.useItem = useItem;
        
        if (furnitureData.functionality is "chest")
        {
            decoration.gameObject.AddComponent<Animator>();
            
            var theirDecoration = ((Placeable)ExampleChest.useItem)._decoration;
            var ourCanvas = Object.Instantiate(theirDecoration.transform.Find("PlayerChestCanvas"), decoration.transform);
            var ourChest = decoration.GetComponent<Chest>();

            var title = ourCanvas.Find("UI/ExternalInventory/Panel/ExternalInventoryTitle/Text (TMP)");
            Object.Destroy(title.GetComponent<Localize>());
            title.GetComponent<TextMeshProUGUI>().text = item.name;

            Traverse.Create(ourChest).Field("interactText").SetValue(item.name);
   
            var chestFixer = ourCanvas.Find("UI").gameObject.AddComponent<ChestFixer>();
            chestFixer.chest = ourChest;

            var nameInput = ourCanvas.Find("UI/ExternalInventory/Panel/ExternalInventoryTitle/InputField (TMP)").GetComponent<SunHavenInputField>();
            nameInput.onEndEdit = new SunHavenInputField.SubmitEvent();

            ourChest.playerInventory = ourCanvas.Find("UI/PlayerInventory").GetComponent<Inventory>();
            ourChest.sellingInventory = ourCanvas.Find("UI/ExternalInventory").GetComponent<Inventory>();
            Traverse.Create(ourChest).Field("chestName").SetValue(nameInput);
            Traverse.Create(ourChest).Field("ui").SetValue(ourCanvas.Find("UI").gameObject);
            
            decoration.gameObject.SetActive(true);
            ChestManager.RemoveInventory(ourChest.sellingInventory, ourChest);
        }
        else
        {
            decoration.gameObject.SetActive(true);
        }

        return item;
    }
}
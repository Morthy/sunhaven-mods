using System;
using System.Reflection;
using Morthy.Util;
using PSS;
using UnityEngine;
using Wish;

namespace Sprinklers;

public static class ItemHandler
{
    public const int SmallSprinklerId = 61011;
    public const int LargeSprinklerId = 61012;
    public const int NelvariSprinklerId = 61013;
    public const int WithergateSprinklerId = 61014;

    private static void EnableSprinkler(ItemData item, int range)
    {
        if (!item.name.Contains("Sprinkler"))
        {
            throw new Exception($"Item {item.id} does not appear to be a sprinkler.");
        }

        var placeable = (Placeable)item.useItem;
        placeable.snapToTile = true;
        var p = 0.5f - (1f / (range * 2 + 1) / 2);
        placeable._secondaryPreviewSprite = SpriteUtil.CreateSprite(FileLoader.LoadFileBytes(Assembly.GetExecutingAssembly(), $"preview_{range}.png"), new Vector2(p, p), $"Sprinkler preview");
        placeable.previewOffset = new Vector2(-0.1f, 0);
        placeable._decoration.transform.Find("Graphics").localPosition = new Vector3(-0.1f, 0, 0);
    }
    
    public static void CreateSprinklerItems()
    {
        Database.GetData<ItemData>(SmallSprinklerId, data => EnableSprinkler(data, 1));
        Database.GetData<ItemData>(LargeSprinklerId, data => EnableSprinkler(data, 2));
        Database.GetData<ItemData>(NelvariSprinklerId, data => EnableSprinkler(data, 3));
        Database.GetData<ItemData>(WithergateSprinklerId, data => EnableSprinkler(data, 4));
    }
}
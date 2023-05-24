using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Wish;
using Object = UnityEngine.Object;

namespace CreativeMode;

public static class ItemHandler
{
    public const int DecorationCatalogueId = 30010;
    
    public static void CreateDecorationCatalogueItem()
    {
        
        if (ItemDatabase.GetItemData(DecorationCatalogueId) && ItemDatabase.GetItemData(DecorationCatalogueId).name != "Decoration Catalogue")
        {
            Plugin.logger.LogInfo(ItemDatabase.GetItemData(DecorationCatalogueId).name);
            Plugin.logger.LogError("Cannot create decoration catalogue, an item already exists with this ID.");
            return;
        }
        
        var item = ScriptableObject.CreateInstance<ItemData>();
        JsonUtility.FromJsonOverwrite(LoadFile("data.30010.json"), item);
        
        var texture = CreateTexture(LoadFileBytes("img.30010.png"));
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 24);
        item.icon = sprite;

        item.useItem = new GameObject("Decoration Catalogue Useable").AddComponent<DecorationCatalogue>();
        Object.DontDestroyOnLoad(item.useItem);

        ItemDatabase.items[item.id] = item;
        ItemDatabase.ids[item.name.RemoveWhitespace().ToLower()] = item.id;
    }
    

    private static string GetResourcePath(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourcePath = name;
        if (!name.StartsWith(nameof(CreativeMode)))
        {
            resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(name));
        }

        return resourcePath;
    }
    
    private static string LoadFile(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using (var stream = assembly.GetManifestResourceStream(GetResourcePath(name)))
        using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
        {
            return reader.ReadToEnd();
        }
    }
    
    private static byte[] LoadFileBytes(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using (var stream = assembly.GetManifestResourceStream(GetResourcePath(name)))
        using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                reader.BaseStream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return bytes;
        }
    }
    
    private static Texture2D CreateTexture(byte[] data)
    {
        var texture = new Texture2D(1, 1);
        texture.LoadImage(data);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.wrapModeU = TextureWrapMode.Clamp;
        texture.wrapModeV = TextureWrapMode.Clamp;
        texture.wrapModeW = TextureWrapMode.Clamp;
        return texture;
    }
}
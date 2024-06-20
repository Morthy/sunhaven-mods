using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using PSS;
using UnityEngine;
using Wish;
using Object = UnityEngine.Object;

namespace CreativeMode;

public static class ItemHandler
{
    public const int DecorationCatalogueId = 61010;
    public static bool IsLoaded;
    public static bool IsLoading;
    private static Dictionary<int, ItemData> ItemCache = new();

    public static void SetupDecorationCatalogueItem()
    {
        Database.GetData<ItemData>(DecorationCatalogueId, item =>
        {
            if (!item.name.Equals("Decoration Catalogue"))
            {
                throw new Exception("Refusing to setup decoration catalogue because another item exists with its ID");
            }
            
            item.useItem = new GameObject("Decoration Catalogue Useable").AddComponent<DecorationCatalogue>();
            Object.DontDestroyOnLoad(item.useItem);
            Plugin.logger.LogDebug("Setup catalogue useItem");
        });
    }

    public static ItemData GetItem(int itemId)
    {
        return ItemCache[itemId];
    }

    public static IEnumerator LoadAllItems(Action onDone)
    {
        NotificationStack.Instance.SendNotification("<color=orange>Creative Mode</color>: Loading item data (this may take a few seconds, but only has to be done once)");

        var lastPercent = 0;
        var start = new Stopwatch();
        start.Start();
        IsLoading = true;
        var error = false;
        
        foreach (var itemId in DecorationCategorization.GetAllItems())
        {
            Database.GetData<ItemData>(itemId, data => ItemCache[itemId] = data, () => error = true);          
        }
        
        while (ItemCache.Count != DecorationCategorization.GetAllItems().Count)
        {
            var percentDone = (int)Math.Floor((double)ItemCache.Count / DecorationCategorization.GetAllItems().Count * 100);
            Plugin.logger.LogInfo(percentDone);

            if (lastPercent != percentDone && percentDone is > 0 and (25 or 50 or 75))
            {
                NotificationStack.Instance.SendNotification($"<color=orange>Creative Mode</color>: Loading item data ({percentDone}% complete)");
            }

            lastPercent = percentDone;
            
                if (error)
                {
                    IsLoading = false;
                    yield break;
                }
                
            yield return new WaitForSecondsRealtime(0.1f);
        }

        Plugin.logger.LogDebug($"Loaded all items in {start.ElapsedMilliseconds}ms");
        IsLoaded = true;
        IsLoading = false;
        onDone();

        yield return null;
    }
}
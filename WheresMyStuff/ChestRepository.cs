using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using PSS;
using UnityEngine.SceneManagement;
using Wish;

namespace WheresMyStuff;

public class ChestRepository
{
    private List<int> _allowedChests = new();
    private Dictionary<int, List<ChestLocation>> _chestLocations = new();

    public List<ChestLocation> GetChestsWithItem(int itemId)
    {
        return (from l in _chestLocations from chestLocation in l.Value where chestLocation.GetCountOfItem(itemId) > 0 select chestLocation).ToList();
    }

    private void DetermineAllowedChests()
    {
        if (_allowedChests.Count > 0)
        {
            return;
        }
        
        foreach (var field in typeof(ItemID).GetFields())
        {
            if (!field.Name.EndsWith("Chest") && !field.Name.EndsWith("Wardrobe") && !field.Name.EndsWith("Fridge")) continue;
            
            var id = (int)field.GetValue(null);
                
            Plugin.logger.LogDebug($"Registered allowed chest {field.Name} #{id} ");
            _allowedChests.Add(id);
        }
        
        _allowedChests.Add(ItemID.AutomaticCollector);
        _allowedChests.Add(ItemID.AutomaticFeeder);
    }

    public void UpdateChestDirect(int sceneId, Chest chest)
    {
        ChestLocation found = _chestLocations[sceneId].FirstOrDefault(chestLocation => chestLocation.x == chest._position.x && chestLocation.y == chest._position.y);

        if (found == null)
        {
            return;
        }
        
        found.ResetItems();
        foreach (var item in chest.data.items.Values.Where(item => item.Item.ID() > 0))
        {
            found.AddItemCount(item.Item.ID(), item.Amount);
        }
    }

    public void UpdateChestLocations()
    {
        DetermineAllowedChests();

        var sw = new Stopwatch();
        sw.Start();
        
        for (var sceneId = 0; sceneId < SceneManager.sceneCountInBuildSettings; sceneId++)
        {
            _chestLocations[sceneId] = new List<ChestLocation>();
            UpdateChestLocations(sceneId);
        }
        
        sw.Stop();
        Plugin.logger.LogDebug($"updated chests in {sw.ElapsedMilliseconds}ms");
    }

    public void UpdateChestLocations(int sceneId, bool withItems = true)
    {
        if (!SingletonBehaviour<GameSave>.Instance.CurrentWorld.decorations.ContainsKey((short)sceneId))
        {
            return;
        }

        if (!_chestLocations.ContainsKey(sceneId))
        {
            _chestLocations[sceneId] = new List<ChestLocation>();
        }

        var decorations = GameSave.Instance.CurrentWorld.decorations[(short)sceneId];
            
        foreach (var decoration in decorations)
        {
            var decorationPositionData = decoration.Value;

            if (decorationPositionData.id == 0 || !_allowedChests.Contains(decorationPositionData.id)) continue;
            
            ChestData chestData = null;
            Decoration.DeserializeMeta(decorationPositionData.meta, ref chestData);

            if (chestData == null || chestData.items == null)
            {
                continue;
            }
            
            ChestLocation chestLocation = _chestLocations[sceneId].FirstOrDefault(chestLocation => chestLocation.x == decorationPositionData.x && chestLocation.y == decorationPositionData.y);

            if (chestLocation == null)
            {
                chestLocation = new ChestLocation(sceneId, decorationPositionData.id, decorationPositionData.x, decorationPositionData.y, chestData.color);
                _chestLocations[sceneId].Add(chestLocation);
                Plugin.logger.LogDebug($"Registered chest with id {decorationPositionData.id} in scene {sceneId}");
            }

            if (!chestData.name.IsNullOrWhiteSpace())
            {
                chestLocation.friendlyName = chestData.name;
            }
            else
            {
                Database.GetData<ItemData>(decorationPositionData.id, data => chestLocation.friendlyName = data.name );
            }

            if (withItems)
            {
                chestLocation.ResetItems();
                
                foreach (var item in chestData.items.Values.Where(item => item.Item.ID() > 0))
                {
                    chestLocation.AddItemCount(item.Item.ID(), item.Amount);
                }
            }
        }
    }
}
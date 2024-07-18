using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace WheresMyStuff;

public class ChestLocation
{
    private static Dictionary<int, string> _colors = new()
    {
        { 0, "#ffffff" },
        { 1, "#e03700" },
        { 2, "#e07b00" },
        { 3, "#dfe000" },
        { 4, "#318c00" },
        { 5, "#1c89e0" },
        { 6, "#8e19ba" },
        { 7, "#e067b8" },
        { 8, "#e0e0e0" },
        { 9, "#909090" },
        { 10, "#4e4e4e" },
    };

    private static Dictionary<string, string> _friendlySceneNames = new()
    {
        { "2playerfarm", "Sun Haven Farm" },
        { "Tier1House0", "Sun Haven House" },
        { "Tier2House0", "Sun Haven House" },
        { "Tier3House0", "Sun Haven House" },
        { "Tier1House1", "Sun Haven House 2" },
        { "Tier2House1", "Sun Haven House 2" },
        { "Tier3House1", "Sun Haven House 2" },
        { "Tier1House2", "Sun Haven House 3" },
        { "Tier2House2", "Sun Haven House 3" },
        { "Tier3House2", "Sun Haven House 3" },
        { "Tier1House3", "Sun Haven House 4" },
        { "Tier2House3", "Sun Haven House 4" },
        { "Tier3House3", "Sun Haven House 4" },
        { "Tier1House4", "Sun Haven House 5" },
        { "Tier2House4", "Sun Haven House 5" },
        { "Tier3House4", "Sun Haven House 5" },
        { "Tier1House5", "Sun Haven House 6" },
        { "Tier2House5", "Sun Haven House 6" },
        { "Tier3House5", "Sun Haven House 6" },
        { "Tier1House6", "Sun Haven House 7" },
        { "Tier2House6", "Sun Haven House 7" },
        { "Tier3House6", "Sun Haven House 7" },
        { "Tier1House7", "Sun Haven House 8" },
        { "Tier2House7", "Sun Haven House 8" },
        { "Tier3House7", "Sun Haven House 8" },
        { "WithergateRooftopFarm", "Withgate Rooftop Farm"},
        { "NelvariFarm", "Nel'Vari Farm"},
        { "Tier1Barn0", "Sun Haven Barn 1"},
        { "Tier1Barn1", "Sun Haven Barn 2"},
        { "Tier1Barn2", "Sun Haven Barn 3"},
        { "Tier1Barn3", "Sun Haven Barn 4"},
        { "Tier1Shed0", "Sun Haven Shed 1"},
        { "Tier1Shed1", "Sun Haven Shed 2"},
        { "Tier1Shed2", "Sun Haven Shed 3"},
        { "Tier1Shed3", "Sun Haven Shed 4"},
        { "Tier1Shed4", "Sun Haven Shed 5"},
        { "Tier1Shed5", "Sun Haven Shed 6"},
        { "Tier1Shed6", "Sun Haven Shed 7"},
        { "Tier1Shed7", "Sun Haven Shed 8"},
        { "Tier1Shed8", "Sun Haven Shed 9"},
        { "Tier1Shed9", "Sun Haven Shed 10"},
        { "Tier1Shed10", "Sun Haven Shed 11"},
        { "Tier1Shed11", "Sun Haven Shed 12"},
        { "Tier1Nel'VariBarn0", "Nel'Vari Barn 1"},
        { "Tier1Nel'VariBarn1", "Nel'Vari Barn 2"},
        { "Tier1Nel'VariBarn2", "Nel'Vari Barn 3"},
        { "Tier1Nel'VariBarn3", "Nel'Vari Barn 4"},
        { "WithergateBarn", "Withergate Barn"},
        { "Tier1Coop0", "Chicken Coop 1"},
        { "Tier1Coop1", "Chicken Coop 2"},
        { "Tier1Coop2", "Chicken Coop 3"},
        { "Tier1Coop3", "Chicken Coop 4"},
        { "Tier1Garden0", "Garden 1"},
        { "Tier1Garden1", "Garden 2"},
        { "Tier1Garden2", "Garden 3"},
        { "Tier1Garden3", "Garden 4"},
        { "Tier1Workshop0", "Crafting Workshop 1"},
        { "Tier1Workshop1", "Crafting Workshop 2"},
        { "Tier1Workshop2", "Crafting Workshop 3"},
        { "Tier1Workshop3", "Crafting Workshop 4"},
        { "Tier1WithergateWorkshop0", "Withergate Workshop 1"},
        { "Tier1WithergateWorkshop1", "Withergate Workshop 2"},
        { "Tier1WithergateWorkshop2", "Withergate Workshop 3"},
        { "Tier1WithergateWorkshop3", "Withergate Workshop 4"},
        { "Tier1NelvariWorkshop0", "Nel'Vari Workshop 1"},
        { "Tier1NelvariWorkshop1", "Nel'Vari Workshop 2"},
        { "Tier1NelvariWorkshop2", "Nel'Vari Workshop 3"},
        { "Tier1NelvariWorkshop3", "Nel'Vari Workshop 4"},
        { "GreenHouse_Spring" , "Greenhouse" },
        { "GreenHouse_Summer" , "Greenhouse" },
        { "GreenHouse_Fall" , "Greenhouse" },
        { "GreenHouse_Winter" , "Greenhouse" },
        { "Tier1Warehouse0", "Warehouse 1" },
        { "Tier1Warehouse1", "Warehouse 2" },
        { "Tier1Warehouse2", "Warehouse 3" },
        { "Tier1Warehouse3", "Warehouse 4" },
    };

    public int sceneId;
    public int itemId;
    public int color;
    public int x;
    public int y;
    public string friendlyName;
    private Dictionary<int, int> items = new();

    public ChestLocation(int sceneId, int itemId, int x, int y, int color)
    {
        this.sceneId = sceneId;
        this.itemId = itemId;
        this.color = color;
        this.x = x;
        this.y = y;
    }

    public void ResetItems()
    {
        this.items.Clear();
    }
    
    public void AddItemCount(int id, int amount)
    {
        if (!items.ContainsKey(id))
        {
            items[id] = 0;
        }
        
        items[id] += amount;
    }

    public int GetCountOfItem(int id)
    {
        return items.TryGetValue(id, out var item) ? item : 0;
    }

    public string GetHexColor()
    {
        return _colors.TryGetValue(color, out var hex) ? hex : "#ffffff";
    }
    
    public string GetLocationName()
    {
        var path = SceneUtility.GetScenePathByBuildIndex(sceneId);
        var sceneName = "";

        if (!string.IsNullOrEmpty(path))
        {
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        return _friendlySceneNames.TryGetValue(sceneName, out var name) ? name : sceneName;
    }
}
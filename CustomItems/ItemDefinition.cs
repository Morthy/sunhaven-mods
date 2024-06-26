using System.Collections.Generic;
using JetBrains.Annotations;

namespace CustomItems;


public class ItemDefinition
{
    public int id;
    public string type;
    public string name;
    [CanBeNull] public string image;
    public string description;
    public string useDescription;
    public string helpDescription;
    public int stackSize;
    public bool canSell;
    public bool canTrade;
    public bool canTrash;
    public int goldSellPrice;
    public int orbSellPrice;
    public int ticketSellPrice;
    public int category;
    public int rarity;
    public Dictionary<string, object> useItemProps;
    [CanBeNull] public DecorationDefinition decoration;
    [CanBeNull] public List<ShopItemDefinition> shopEntries;
    [CanBeNull] public List<RecipeDefinition> recipes;
}
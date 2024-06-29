using System.Collections.Generic;

namespace CustomItems;

public class RecipeDefinition
{
    public string list;
    public float hours;
    public List<RecipeInputDefinition> inputs;
    public int amount = 1;
}
using JetBrains.Annotations;
using UnityEngine;

namespace CustomItems;

public class DecorationDefinition
{
    public int[] size;
    public int[] placementSize;
    public int[] offset;
    [CanBeNull] public string functionality;
    public bool placeableOnTables;
    public bool placeableOnWalls;
    public bool placeableAsRug;

    [CanBeNull] public int[] tableOffset;
    [CanBeNull] public int[] tableSize;
    [CanBeNull] public float[] bedSleepOffset;
    [CanBeNull] public string bedSheetImage;
}
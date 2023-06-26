using JetBrains.Annotations;
using UnityEngine;

namespace CustomItems;

public class DecorationDefinition
{
    public int[] size;
    [CanBeNull] public string functionality;
    public bool placeableOnTables;
    public bool placeableOnWalls;
    public bool placeableAsRug;

    [CanBeNull] public int[] tableOffset;
    [CanBeNull] public int[] tableSize;
    [CanBeNull] public float[] bedSleepOffset;
}
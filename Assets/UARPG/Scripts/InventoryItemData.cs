using UnityEngine;
using System.Collections;

[System.Serializable] public struct InventoryItemData
{
    public Object item;
    [Min(1)] public int count;
    [Range(0f, 100f)] public float dropChance;
    /// <summary> x - drop quantity, y - chance </summary>
    public Vector2Int[] dropQuantitiesSpread;
    public bool hidden;

    public InventoryItemData(Object item, int count = 1, float dropChance = 100, Vector2Int[] dropQuantitiesSpread = null, bool hidden = false)
    {
        this.item = item;
        this.count = count;
        this.dropChance = dropChance;
        this.dropQuantitiesSpread = dropQuantitiesSpread;
        this.hidden = hidden;
    }
}

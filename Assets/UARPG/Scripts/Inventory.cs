using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0649

public class Inventory : MonoBehaviour, IGUIDataList
{
    [SerializeField] List<InventoryItemData> inventory = new List<InventoryItemData>();
    [SerializeField][Min(0)] float Money = 0;

    public float money => Money;

    public List<GUIData> guiData { get; private set; } = new List<GUIData>();

    void Start()
    {
        List<InventoryItemData> toRemove = new List<InventoryItemData>();    

        for (int i = 0; i < inventory.Count; i++)
        {
            #if !DEBUG
            if (inventory[i].hidden)
            {
                toRemove.Add(inventory[i]);
                continue;
            }
            #endif

            IInventoryItem iitem = inventory[i].item as IInventoryItem;
            if (iitem == null)
            {
                toRemove.Add(inventory[i]);
                Debug.LogError(inventory[i].item + " at index " + i + " is not an IInventoyItem, removed");
            }
            else guiData.Add(MakeItemGUIDate(iitem, inventory[i].count));
        }
        foreach (var item in toRemove) inventory.Remove(item);
        InventoryChange += UpdateGUIData;
        InventoryChange?.Invoke(this, null, -1, false, false, -1, -1, -1f, -1f);
        GUIDataChanged?.Invoke();
    }


    public void AddMoney(float money)
    {
        Money += money;
        InventoryChange?.Invoke(this, null, -1, true, false, -1, -1, money, Money);
    }
    public float TakeMoney(float amount)
    {
        Money -= amount;
        if (Money < 0)
        {
            try { return amount + Money; }
            finally
            {
                Money = 0;
                InventoryChange?.Invoke(this, null, -1, false, false, -1, -1, amount + Money, 0f);
            }
        }
        else
        {
            InventoryChange?.Invoke(this, null, -1, false, false, -1, -1, amount, Money);
            return amount;
        }
    }

    public void AddInventoryItems(Inventory inventory) => AddInventoryItems(inventory.inventory);
    public void AddInventoryItems(IEnumerable<InventoryItemData> items) { foreach (InventoryItemData data in items) AddInventoryItems(data.item, data.count, data.dropChance); }
    public void AddInventoryItems(InventoryItemData[] data) { for (int i = 0; i < data.Length; i++) AddInventoryItems(data[i].item, data[i].count, data[i].dropChance); }
    public void AddInventoryItems(InventoryItemData data) => AddInventoryItems(data.item, data.count, data.dropChance);
    public void AddInventoryItems(Object[] items)
    {
        if (items is IInventoryItem[]) for (int i = 0; i < items.Length; i++) AddInventoryItems(items[i], 1);
        else Debug.LogError("Item array is not IInventoryItem[]");
    }
    public void AddInventoryItems(Object obj, int count = 1, float dropChance = 100)
    { 
        if (obj is IInventoryItem item)
        {
            if (count > 0)
            {
                int index = inventory.FindIndex((data_) => (data_.item as IInventoryItem).Equals(item));
                InventoryItemData data;
                bool newItem = false;
                if (index < 0)
                {
                    inventory.Add(new InventoryItemData(obj, 0, dropChance));
                    index = inventory.Count - 1;
                    newItem = true;
                }
                data = inventory[index];
                data.count += count;
                inventory[index] = data;
                InventoryChange?.Invoke(this, item, index, true, newItem, count, data.count, -1f, -1f);
            }
        }
        else Debug.LogError(obj + " object is not IInventoryItem");
    }

    public int RemoveInventoryItems(Object obj, int count = 1)
    {
        if (obj is IInventoryItem item)
        {
            int index = inventory.FindIndex((data) => (data.item as IInventoryItem).Equals(item));
            if (index >= 0)
            {
                int ret;
                InventoryItemData data = inventory[index];
                data.count -= count;
                if (data.count <= 0)
                {
                    ret = count + data.count;
                    data.count = 0;
                    inventory.RemoveAt(index);
                    (data.item as IInventoryItem).Destroy();
                }
                else ret = count;
                InventoryChange?.Invoke(this, item, index, false, false, ret, data.count, -1, -1);
                return ret;
            }
        }
        else Debug.LogError(obj + " object is not IInventoryItem");
        return 0;
    }
    public void RemoveInventoryItems(Object[] items)
    {
        if (items is IInventoryItem[]) for (int i = 0; i < items.Length; i++) RemoveInventoryItems(items[i]);
        else Debug.LogError("Item array is not IInventoryItem[]");
    }

    public int MoveInventoryItems(Inventory destination, Object item, int count = 1, float dropChance = 100)
    {
        if (item is IInventoryItem)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].item == item)
                {
                    int moved = RemoveInventoryItems(item, count);
                    destination.AddInventoryItems(item, moved, dropChance);
                    return moved;
                }
            }
        }
        else Debug.LogError(item + " object is not IInventoryItem");
        return 0;
    }
    public int MoveInventoryItems(Inventory destination, InventoryItemData data) => MoveInventoryItems(destination, data.item, data.count, data.dropChance);
    public void MoveInventoryItems(Inventory destination, InventoryItemData[] data) { for (int i = 0; i < data.Length; i++) MoveInventoryItems(destination, data[i]); }
    public void MoveInventoryItems(Inventory destination)
    {
        while (inventory.Count > 0) MoveInventoryItems(destination, inventory[0]);
        destination.Money = money;
        Money = 0;
    }

    public InventoryItemData[] GetInventory() => inventory.ToArray();

    public int Find(IInventoryItem item) => inventory.FindIndex((InventoryItemData data) => (data.item as IInventoryItem).Equals(item));
    public int Find(IInventoryItem item, out InventoryItemData data)
    {
        data = new InventoryItemData();
        int i = Find(item);
        if (i != -1) data = inventory[i];
        return i;
    }

    /// <summary> params: inventory, item, index, added/removed, new item, item amount change, item amount, money change, money amount </summary>
    public event System.Action<Inventory, IInventoryItem, int, bool, bool, int, int, float, float> InventoryChange;
    public event System.Action GUIDataChanged;

    private void UpdateGUIData(Inventory inventory, IInventoryItem item, int index, bool added, bool newItem, int amountChange, int totalAmount, float moneyChange, float totalMoney)
    {
        if (item != null)
        {
            if (added)
            {
                if (newItem) guiData.Add(MakeItemGUIDate(item, totalAmount));
                else guiData[index].texts[1] = totalAmount.ToString();
            }
            else
            {
                if (totalAmount <= 0) guiData.RemoveAt(index);
                else guiData[index].texts[1] = totalAmount.ToString();
            }
            GUIDataChanged?.Invoke();
        }
    }

    public void Drop(ItemContainer container)
    {
        bool containerCreated = false;
        ItemContainer containerInstance = null;
        Inventory containerInventory = null;
        InventoryItemData data;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (Random.Range(0, 100) <= inventory[i].dropChance)
            {
                data = inventory[i];
                if (containerCreated == false)
                {
                    containerCreated = true;
                    containerInstance = Instantiate(container, transform.position, transform.rotation);
                    containerInventory = containerInstance.GetComponent<Inventory>();
                }
                if (data.dropQuantitiesSpread != null && data.dropQuantitiesSpread.Length > 0)
                {
                    float rand = Random.Range(0, 100);
                    int count = 1;
                    for (int ii = 0; ii < data.dropQuantitiesSpread.Length; ii++)
                    {
                        if (rand < data.dropQuantitiesSpread[ii].y) count = data.dropQuantitiesSpread[ii].x;
                    }
                    containerInventory.AddInventoryItems(data.item, count, data.dropChance);
                }
                else containerInventory.AddInventoryItems(data);
            }
        }
    }

    private GUIData MakeItemGUIDate(IInventoryItem item, int amount) => new GUIData(new Sprite[] { item.icon }, new string[] { "<color=#" + item.rarityColor.ToHexString() + ">" + item.UIName + "</color>", amount.ToString() }, item);
}
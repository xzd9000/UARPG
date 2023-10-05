using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class GUIInventoryControl : GUIObject
{
    [SerializeField] GUIContainer inventory;
    [SerializeField] GUIContainer equipment;
    [SerializeField] GUIContainer weapons;
    [SerializeField] Character character;
    [SerializeField] Text moneyGui;
    [SerializeField] LocalizedTextBlock moneyText;

    public int inventorySelection { get; private set; } = -1;
    public int equipmentSelection { get; private set; } = -1;
    public int weaponSelection { get; private set; } = -1;

    protected override void AwakeCustom() => inventory.ChangeGUIDataSource(character.inventory);

    private void OnEnable()
    {
        inventory.SelectionChange += UpdateInventorySelection;
        equipment.SelectionChange += UpdateEquipmentSelection;
        weapons.SelectionChange += UpdateWeaponSelection;
        equipment.ItemInteraction += OnEquipmentInteraction;
        weapons.ItemInteraction += OnWeaponInteraction;
        inventory.ItemInteraction += OnInventoryInteraction;
        character.inventory.InventoryChange += UpdateMoney;
    }
    private void OnDisable()
    {
        inventory.SelectionChange -= UpdateInventorySelection;
        equipment.SelectionChange -= UpdateEquipmentSelection;
        weapons.SelectionChange -= UpdateWeaponSelection;
        equipment.ItemInteraction -= OnEquipmentInteraction;
        weapons.ItemInteraction -= OnWeaponInteraction;
        inventory.ItemInteraction -= OnInventoryInteraction;
    }

    private void UpdateMoney(Inventory inventory, IInventoryItem item, int index, bool added, bool newItem, int amountChange, int totalAmount, float moneyChange, float totalMoney) { if (totalMoney >= 0) moneyGui.text = moneyText.text + ": " + totalMoney; }

    private void UpdateInventorySelection(int i) => inventorySelection = i;
    private void UpdateEquipmentSelection(int i) => equipmentSelection = i;
    private void UpdateWeaponSelection(int i) => weaponSelection = i;

    private void OnEquipmentInteraction(int i)
    {
        if (inventorySelection >= 0)
        {           
            EquipItem(equipment, i, i);           
        }
        else if (weaponSelection > 0) weapons.ResetSelection();
    }
    private void OnWeaponInteraction(int i)
    {
        if (inventorySelection >= 0)
        {
            EquipItem(weapons, i, equipment.totalCount + i);
        }
        else if (equipmentSelection > 0) equipment.ResetSelection();
    }
    private void EquipItem(GUIContainer container, int uiIndex, int slotIndex)
    {
        if (inventorySelection < character.inventory.GetInventory().Length)
        {
            EquippableItem item = character.inventory.GetInventory()[inventorySelection].item as EquippableItem;
            if (item != null)
            {
                if (character.EquipItem(item, slotIndex))
                {
                    character.inventory.RemoveInventoryItems(item);
                    container.items[uiIndex].SetImage(item.icon);
                    container.items[uiIndex].SetText(item.UIName);
                    ResetSelections();
                    return;
                }
            }
        }
        container.ResetSelection();
    }
    private void OnInventoryInteraction(int i)
    {
        if (equipmentSelection >= 0)
        {
            UnequipItem(equipment, equipmentSelection, equipmentSelection);

        }
        else if (weaponSelection >= 0) UnequipItem(weapons, equipment.totalCount + weaponSelection, weaponSelection);
    }
    private void UnequipItem(GUIContainer container, int index, int uiIndex)
    {
        if (character.GetEquippedItem(index) != null)
        {
            character.UnequipItem(index);
            container.items[uiIndex].SetImageAll(null);
            container.items[uiIndex].SetTextAll("");
            ResetSelections();
        }
    }
    private void ResetSelections()
    {
        inventory.ResetSelection();
        equipment.ResetSelection();
        weapons.ResetSelection();
    }

}

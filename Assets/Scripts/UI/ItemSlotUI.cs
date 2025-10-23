using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TextMeshProUGUI itemNameText;
    
    private Item currentItem;
    private Inventory playerInventory;
    private EquipmentManager equipmentManager;
    private bool isEquipmentSlot;
    private EquipmentType? equipmentType;

    public void Initialize(Inventory inventory, EquipmentManager manager, bool isEquipSlot = false, EquipmentType? type = null)
    {
        playerInventory = inventory;
        equipmentManager = manager;
        isEquipmentSlot = isEquipSlot;
        equipmentType = type;
    }

    public void SetItem(Item item)
    {
        currentItem = item;
        
        if (item == null)
        {
            iconImage.enabled = false;
            itemNameText.text = "";
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = item.icon;
        itemNameText.text = item.itemName;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (isEquipmentSlot)
        {
            // 장비 해제
            if (equipmentType.HasValue)
            {
                equipmentManager.Unequip(equipmentType.Value, playerInventory);
            }
        }
        else if (currentItem.isEquipable)
        {
            // 장비 착용
            equipmentManager.Equip(currentItem, playerInventory);
        }
    }
}
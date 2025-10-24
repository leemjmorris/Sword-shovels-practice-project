using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 장비 슬롯: 장착된 아이템
    public Dictionary<EquipmentType, Item> equipped = new Dictionary<EquipmentType, Item>();

    // 장착 시 인벤토리에서 제거하고 장착된 슬롯에 넣기
    public bool Equip(Item item, Inventory inventory)
    {
        if (item == null || !item.isEquipable) return false;

        EquipmentType slot = item.equipmentType;

        // 이미 장착된 아이템이 있으면 인벤토리에 되돌리기
        if (equipped.ContainsKey(slot) && equipped[slot] != null)
        {
            Item previous = equipped[slot];
            if (!inventory.AddItem(previous))
            {
                // 인벤토리 공간 부족
                return false;
            }
        }

        // 장착
        equipped[slot] = item;
        // 인벤토리에서 제거
        inventory.RemoveItem(item);
        return true;
    }

    public bool Unequip(EquipmentType slot, Inventory inventory)
    {
        if (!equipped.ContainsKey(slot) || equipped[slot] == null) return false;
        Item item = equipped[slot];
        if (!inventory.AddItem(item)) return false; // 인벤토리 풀
        equipped[slot] = null;
        return true;
    }

    public Item GetEquipped(EquipmentType slot)
    {
        if (equipped.ContainsKey(slot)) return equipped[slot];
        return null;
    }
}

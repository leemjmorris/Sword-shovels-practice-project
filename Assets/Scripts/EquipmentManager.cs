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

    // ��� ����: ������ ������
    public Dictionary<EquipmentType, Item> equipped = new Dictionary<EquipmentType, Item>();

    // ���� �� �κ��丮���� �����ϰ� ������ ���Կ� �ֱ�
    public bool Equip(Item item, Inventory inventory)
    {
        if (item == null || !item.isEquipable) return false;

        EquipmentType slot = item.equipmentType;

        // �̹� ������ �������� ������ �κ��丮�� �ǵ�����
        if (equipped.ContainsKey(slot) && equipped[slot] != null)
        {
            Item previous = equipped[slot];
            if (!inventory.AddItem(previous))
            {
                // �κ��丮 ���� ����
                return false;
            }
        }

        // ����
        equipped[slot] = item;
        // �κ��丮���� ����
        inventory.RemoveItem(item);
        return true;
    }

    public bool Unequip(EquipmentType slot, Inventory inventory)
    {
        if (!equipped.ContainsKey(slot) || equipped[slot] == null) return false;
        Item item = equipped[slot];
        if (!inventory.AddItem(item)) return false; // �κ��丮 Ǯ
        equipped[slot] = null;
        return true;
    }

    public Item GetEquipped(EquipmentType slot)
    {
        if (equipped.ContainsKey(slot)) return equipped[slot];
        return null;
    }
}

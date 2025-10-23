using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    public bool isEquipable = false;
    public EquipmentType equipmentType;

    // 추가 데이터(스탯 등)는 여기에
}

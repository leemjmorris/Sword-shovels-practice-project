using UnityEngine;

public class InventoryButtonActions : MonoBehaviour
{
    [Tooltip("드래그로 할당할 Inventory 컴포넌트")]
    public Inventory playerInventory;

    [Tooltip("추가/제거 테스트용 Item ScriptableObject")]
    public Item testItem;

    public void AddItem()
    {
        if (playerInventory == null || testItem == null)
        {
            Debug.LogWarning("Inventory 또는 TestItem이 할당되지 않았습니다.");
            return;
        }

        bool ok = playerInventory.AddItem(testItem);
        Debug.Log(ok ? $"아이템 추가: {testItem.itemName}" : "인벤토리 가득 참");
    }

    public void RemoveItem()
    {
        if (playerInventory == null || testItem == null)
        {
            Debug.LogWarning("Inventory 또는 TestItem이 할당되지 않았습니다.");
            return;
        }

        bool ok = playerInventory.RemoveItem(testItem);
        Debug.Log(ok ? $"아이템 제거: {testItem.itemName}" : "아이템을 찾을 수 없음");
    }

    // 버튼에서 직접 수를 입력해 호출할 수 있도록 인수형 메서드도 제공
    public void AddMultiple(int count)
    {
        if (playerInventory == null || testItem == null) return;
        int added = 0;
        for (int i = 0; i < count; i++) if (playerInventory.AddItem(testItem)) added++;
        Debug.Log($"AddMultiple: {added} items added");
    }

    public void ClearAll()
    {
        if (playerInventory == null) return;
        for (int i = playerInventory.items.Count - 1; i >= 0; i--)
        {
            playerInventory.items.RemoveAt(i);
        }
        Debug.Log("Inventory cleared");
    }

    // 버튼에서 런타임으로 생성된 슬롯만 정리하려면 이 메서드를 사용하세요.
    public void ClearCreatedSlots(InventoryUI ui)
    {
        if (ui == null)
        {
            Debug.LogWarning("InventoryUI 레퍼런스가 필요합니다.");
            return;
        }
        ui.ClearCreatedSlots();
    }
}
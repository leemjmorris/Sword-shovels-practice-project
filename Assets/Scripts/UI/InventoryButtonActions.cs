using UnityEngine;

public class InventoryButtonActions : MonoBehaviour
{
    [Tooltip("�巡�׷� �Ҵ��� Inventory ������Ʈ")]
    public Inventory playerInventory;

    [Tooltip("�߰�/���� �׽�Ʈ�� Item ScriptableObject")]
    public Item testItem;

    public void AddItem()
    {
        if (playerInventory == null || testItem == null)
        {
            Debug.LogWarning("Inventory �Ǵ� TestItem�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        bool ok = playerInventory.AddItem(testItem);
        Debug.Log(ok ? $"������ �߰�: {testItem.itemName}" : "�κ��丮 ���� ��");
    }

    public void RemoveItem()
    {
        if (playerInventory == null || testItem == null)
        {
            Debug.LogWarning("Inventory �Ǵ� TestItem�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        bool ok = playerInventory.RemoveItem(testItem);
        Debug.Log(ok ? $"������ ����: {testItem.itemName}" : "�������� ã�� �� ����");
    }

    // ��ư���� ���� ���� �Է��� ȣ���� �� �ֵ��� �μ��� �޼��嵵 ����
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

    // ��ư���� ��Ÿ������ ������ ���Ը� �����Ϸ��� �� �޼��带 ����ϼ���.
    public void ClearCreatedSlots(InventoryUI ui)
    {
        if (ui == null)
        {
            Debug.LogWarning("InventoryUI ���۷����� �ʿ��մϴ�.");
            return;
        }
        ui.ClearCreatedSlots();
    }
}
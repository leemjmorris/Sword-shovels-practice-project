using UnityEngine;
using UnityEngine.UI;

public class InventoryTestUI : MonoBehaviour
{
    public Inventory playerInventory;
    public Item testItem; // �׽�Ʈ�� ����� ScriptableObject ������
    public Button addButton;
    public Button removeButton;
    public Text statusText;

    void Start()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<Inventory>();

        if (addButton != null) addButton.onClick.AddListener(OnAdd);
        if (removeButton != null) removeButton.onClick.AddListener(OnRemove);

        UpdateStatus();
    }

    void OnAdd()
    {
        if (playerInventory == null || testItem == null) return;
        bool ok = playerInventory.AddItem(testItem);
        statusText.text = ok ? "Item added" : "Inventory full";
    }

    void OnRemove()
    {
        if (playerInventory == null || testItem == null) return;
        bool ok = playerInventory.RemoveItem(testItem);
        statusText.text = ok ? "Item removed" : "Item not found";
    }

    void UpdateStatus()
    {
        if (statusText == null) return;
        statusText.text = $"Items: { (playerInventory != null ? playerInventory.items.Count.ToString() : "-") }/{ (playerInventory != null ? playerInventory.capacity.ToString() : "-") }";
    }

    void Update()
    {
        // ���¸� ���� ������Ʈ
        UpdateStatus();
    }
}

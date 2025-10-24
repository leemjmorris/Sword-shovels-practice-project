using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;                  // CancellationToken
using Cysharp.Threading.Tasks;           // ? UniTask

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform inventoryContent;   // Scroll View > Viewport > Content
    public Transform equipmentContent;

    [Header("Optional")]
    public ScrollRect scrollRect;
    public bool scrollToBottomOnAdd = true;

    private Inventory playerInventory;
    private EquipmentManager equipmentManager;
    private readonly List<ItemSlotUI> inventorySlots = new();
    private readonly Dictionary<EquipmentType, ItemSlotUI> equipmentSlots = new();

    CancellationToken destroyToken;      // ? OnDestroy �ڵ� ���

    void Start()
    {
        destroyToken = this.GetCancellationTokenOnDestroy();

        playerInventory  = FindFirstObjectByType<Inventory>();
        equipmentManager = EquipmentManager.instance;

        if (playerInventory == null) { Debug.LogError("Inventory ����"); return; }
        if (itemSlotPrefab == null)   { Debug.LogError("itemSlotPrefab ������"); return; }
        if (inventoryContent == null) { Debug.LogError("inventoryContent ������"); return; }
        if (equipmentContent == null) { Debug.LogWarning("equipmentContent ������(��� ���� ����)"); }

        // �̺�Ʈ ����
        playerInventory.OnItemAdded   += HandleItemAdded;
        playerInventory.OnItemRemoved += HandleItemRemoved;
        playerInventory.OnChanged     += Refresh;

        InitializeInventorySlots();
        InitializeEquipmentSlots();

        if (playerInventory.items.Count > inventorySlots.Count)
            EnsureInventorySlotCount(playerInventory.items.Count);

        Refresh();
    }

    void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnItemAdded   -= HandleItemAdded;
            playerInventory.OnItemRemoved -= HandleItemRemoved;
            playerInventory.OnChanged     -= Refresh;
        }
    }

    // -------------------- init --------------------
    void InitializeInventorySlots()
    {
        inventorySlots.Clear();
        if (inventoryContent != null)
            foreach (Transform child in inventoryContent) child.gameObject.SetActive(false);
    }

    void InitializeEquipmentSlots()
    {
        equipmentSlots.Clear();
        if (equipmentContent != null)
            foreach (Transform child in equipmentContent) child.gameObject.SetActive(false);
    }

    void EnsureInventorySlotCount(int needCount)
    {
        if (itemSlotPrefab == null || inventoryContent == null) return;

        while (inventorySlots.Count < needCount)
        {
            var go   = Instantiate(itemSlotPrefab, inventoryContent);
            var slot = go.GetComponent<ItemSlotUI>();
            if (slot == null)
            {
                Debug.LogWarning("itemSlotPrefab�� ItemSlotUI ����");
                Destroy(go);
                return;
            }
            slot.Initialize(playerInventory, equipmentManager);
            if (go.GetComponent<CreatedSlotMarker>() == null) go.AddComponent<CreatedSlotMarker>();
            inventorySlots.Add(slot);
        }
    }

    // -------------------- �̺�Ʈ �ڵ鷯 --------------------
    void HandleItemAdded(Item item)
    {
        int need = Mathf.Max(playerInventory.capacity, playerInventory.items.Count);
        EnsureInventorySlotCount(need);
        Refresh();

        if (scrollRect && scrollToBottomOnAdd)
            ScrollToBottomNextFrameAsync().Forget();   // ? UniTask ���
    }

    void HandleItemRemoved(Item item)
    {
        int needed = playerInventory.items.Count;
        for (int i = inventorySlots.Count - 1; i >= 0 && inventorySlots.Count > needed; i--)
        {
            var slot   = inventorySlots[i];
            if (slot == null) continue;
            var marker = slot.GetComponent<CreatedSlotMarker>();
            if (marker != null && inventorySlots.Count > needed)
            {
                inventorySlots.RemoveAt(i);
                Destroy(marker.gameObject);
            }
        }
        Refresh();
    }

    // -------------------- UniTask �񵿱� ��ũ�� --------------------
    async UniTask ScrollToBottomNextFrameAsync()
    {
        // 1�� ���̾ƿ� ����
        if (inventoryContent is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        // ���� �����ӱ��� ��� (�ı� �� �ڵ����)
        await UniTask.Yield(cancellationToken: destroyToken);

        Canvas.ForceUpdateCanvases();
        ScrollToBottom();
    }

    void ScrollToBottom()
    {
        if (scrollRect == null) return;

        bool isTopToBottom = true;
        if (scrollRect.verticalScrollbar != null)
            isTopToBottom = (scrollRect.verticalScrollbar.direction == Scrollbar.Direction.TopToBottom);

        // TopToBottom: 0 = �ٴ� / BottomToTop: 1 = �ٴ�
        scrollRect.verticalNormalizedPosition = isTopToBottom ? 0f : 1f;
    }

    // -------------------- ȭ�� ���� --------------------
    public void Refresh()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < playerInventory.items.Count)
                inventorySlots[i].SetItem(playerInventory.items[i]);
            else
                inventorySlots[i].SetItem(null);
        }

        foreach (var kvp in equipmentSlots)
        {
            var equippedItem = equipmentManager.GetEquipped(kvp.Key);
            kvp.Value.SetItem(equippedItem);
        }

        if (inventoryContent is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Canvas.ForceUpdateCanvases();
    }

    public void ClearCreatedSlots()
    {
        if (playerInventory == null) return;

        // �̺�Ʈ ��� ����(������ ����)
        playerInventory.OnItemAdded -= HandleItemAdded;
        playerInventory.OnItemRemoved -= HandleItemRemoved;
        playerInventory.OnChanged      -= Refresh;    
        // �ڿ��� ������ �����ϰ� ��ȸ
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            var marker = slot.GetComponent<CreatedSlotMarker>();
            if (marker != null)
            {
                // ���� �ε����� �����ϴ� �������� ������ ���� (�̺�Ʈ�� UI�� ������)
                if (i < playerInventory.items.Count)
                {
                    var itemToRemove = playerInventory.items[i];
                    if (itemToRemove != null)
                        playerInventory.RemoveItem(itemToRemove);
                }

                inventorySlots.RemoveAt(i);
                Destroy(marker.gameObject);
            }
        }

        // �̺�Ʈ �籸��
        playerInventory.OnItemAdded += HandleItemAdded;
        playerInventory.OnItemRemoved += HandleItemRemoved;
        playerInventory.OnChanged     += Refresh; 

        Refresh();
    }
}

// ������ ��Ÿ�� ���������� ǥ���ϴ� ��Ŀ


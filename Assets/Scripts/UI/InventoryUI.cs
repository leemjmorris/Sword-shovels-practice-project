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

    public Inventory playerInventory;
    private EquipmentManager equipmentManager;
    private readonly List<ItemSlotUI> inventorySlots = new();
    private readonly Dictionary<EquipmentType, ItemSlotUI> equipmentSlots = new();

    CancellationToken destroyToken;      // ? OnDestroy 자동 취소

    void Start()
    {
        destroyToken = this.GetCancellationTokenOnDestroy();

        playerInventory  = FindObjectOfType<Inventory>();
        equipmentManager = EquipmentManager.instance;

        if (playerInventory == null) { Debug.LogError("Inventory 없음"); return; }
        if (itemSlotPrefab == null)   { Debug.LogError("itemSlotPrefab 미지정"); return; }
        if (inventoryContent == null) { Debug.LogError("inventoryContent 미지정"); return; }
        if (equipmentContent == null) { Debug.LogWarning("equipmentContent 미지정(장비 슬롯 생략)"); }

        // 이벤트 구독
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
        {
            foreach (Transform child in inventoryContent)
            {
                
            }
        }
    }

    void InitializeEquipmentSlots()
    {
        equipmentSlots.Clear();
        if (equipmentContent == null) return;
        
             foreach (Transform child in equipmentContent)
                {
                    child.gameObject.SetActive(true); 
                }
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
                Debug.LogWarning("itemSlotPrefab에 ItemSlotUI 없음");
                Destroy(go);
                return;
            }
            slot.Initialize(playerInventory, equipmentManager);
            if (go.GetComponent<CreatedSlotMarker>() == null) go.AddComponent<CreatedSlotMarker>();
            inventorySlots.Add(slot);
        }
    }

    // -------------------- 이벤트 핸들러 --------------------
    void HandleItemAdded(Item item)
    {
        //int need = Mathf.Max(playerInventory.capacity, playerInventory.items.Count);
        int need = playerInventory.items.Count;  
        EnsureInventorySlotCount(need);
        Refresh();

        if (scrollRect && scrollToBottomOnAdd)
            ScrollToBottomNextFrameAsync().Forget();   // ? UniTask 사용
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

    // -------------------- UniTask 비동기 스크롤 --------------------
    async UniTask ScrollToBottomNextFrameAsync()
    {
        
        if (inventoryContent is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        // 다음 프레임까지 대기 (파괴 시 자동취소)
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

        // TopToBottom: 0 = 바닥 / BottomToTop: 1 = 바닥
        scrollRect.verticalNormalizedPosition = isTopToBottom ? 0f : 1f;
    }

    // -------------------- 화면 갱신 --------------------
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

        // 이벤트 잠시 해제(재진입 방지)
        playerInventory.OnItemAdded -= HandleItemAdded;
        playerInventory.OnItemRemoved -= HandleItemRemoved;
        playerInventory.OnChanged      -= Refresh;    
        // 뒤에서 앞으로 안전하게 순회
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            var marker = slot.GetComponent<CreatedSlotMarker>();
            if (marker != null)
            {
                // 슬롯 인덱스에 대응하는 아이템이 있으면 제거 (이벤트로 UI도 정리됨)
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

        // 이벤트 재구독
        playerInventory.OnItemAdded += HandleItemAdded;
        playerInventory.OnItemRemoved += HandleItemRemoved;
        playerInventory.OnChanged     += Refresh; 

        Refresh();
    }
}




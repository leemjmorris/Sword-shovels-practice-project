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

    CancellationToken destroyToken;      // ? OnDestroy ï¿½Úµï¿½ ï¿½ï¿½ï¿?

    void Start()
    {
        destroyToken = this.GetCancellationTokenOnDestroy();

        playerInventory  = FindFirstObjectByType<Inventory>();
        equipmentManager = EquipmentManager.instance;

        if (playerInventory == null) { Debug.LogError("Inventory ï¿½ï¿½ï¿½ï¿½"); return; }
        if (itemSlotPrefab == null)   { Debug.LogError("itemSlotPrefab ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½"); return; }
        if (inventoryContent == null) { Debug.LogError("inventoryContent ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½"); return; }
        if (equipmentContent == null) { Debug.LogWarning("equipmentContent ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½(ï¿½ï¿½ï¿? ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½)"); }

        // ï¿½Ìºï¿½Æ® ï¿½ï¿½ï¿½ï¿½
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
                    
                    var slot = child.GetComponent<ItemSlotUI>();
                    if (slot == null) continue;

                    var tag = child.GetComponent<EquipmentSlotTag>();
                    // ÅÂ±×°¡ ¾øÀ¸¸é Å¸ÀÔÀ» ¸ð¸§ ¡æ UI °»½Å ´ë»ó¿¡¼­ Á¦¿Ü
                    if (tag == null) continue;

                    // ÀÌ ½½·ÔÀº "Àåºñ ½½·Ô"ÀÌ´Ù ¶ó°í ¾Ë·ÁÁÖ°í Å¸ÀÔµµ ³Ñ°ÜÁÜ
                    slot.Initialize(playerInventory, equipmentManager, true, tag.type);

                    // Refresh()¿¡¼­ Ã¤¿ï ¼ö ÀÖµµ·Ï ¸ÅÇÎ µî·Ï
                    equipmentSlots[tag.type] = slot;        
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
                Debug.LogWarning("itemSlotPrefabï¿½ï¿½ ItemSlotUI ï¿½ï¿½ï¿½ï¿½");
                Destroy(go);
                return;
            }
            slot.Initialize(playerInventory, equipmentManager);
            if (go.GetComponent<CreatedSlotMarker>() == null) go.AddComponent<CreatedSlotMarker>();
            inventorySlots.Add(slot);
        }
    }

    // -------------------- ï¿½Ìºï¿½Æ® ï¿½Úµé·¯ --------------------
    void HandleItemAdded(Item item)
    {
        //int need = Mathf.Max(playerInventory.capacity, playerInventory.items.Count);
        int need = playerInventory.items.Count;  
        EnsureInventorySlotCount(need);
        Refresh();

        if (scrollRect && scrollToBottomOnAdd)
            ScrollToBottomNextFrameAsync().Forget();   // ? UniTask ï¿½ï¿½ï¿?
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

    // -------------------- UniTask ï¿½ñµ¿±ï¿½ ï¿½ï¿½Å©ï¿½ï¿½ --------------------
    async UniTask ScrollToBottomNextFrameAsync()
    {
        
        if (inventoryContent is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½Ó±ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿? (ï¿½Ä±ï¿½ ï¿½ï¿½ ï¿½Úµï¿½ï¿½ï¿½ï¿?)
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

        // TopToBottom: 0 = ï¿½Ù´ï¿½ / BottomToTop: 1 = ï¿½Ù´ï¿½
        scrollRect.verticalNormalizedPosition = isTopToBottom ? 0f : 1f;
    }

    // -------------------- È­ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ --------------------
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

        // ï¿½Ìºï¿½Æ® ï¿½ï¿½ï¿? ï¿½ï¿½ï¿½ï¿½(ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½)
        playerInventory.OnItemAdded -= HandleItemAdded;
        playerInventory.OnItemRemoved -= HandleItemRemoved;
        playerInventory.OnChanged      -= Refresh;    
        // ï¿½Ú¿ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½Ï°ï¿½ ï¿½ï¿½È¸
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            var slot = inventorySlots[i];
            if (slot == null) continue;

            var marker = slot.GetComponent<CreatedSlotMarker>();
            if (marker != null)
            {
                // ï¿½ï¿½ï¿½ï¿½ ï¿½Îµï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½Ï´ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ (ï¿½Ìºï¿½Æ®ï¿½ï¿½ UIï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½)
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

        // ï¿½Ìºï¿½Æ® ï¿½ç±¸ï¿½ï¿½
        playerInventory.OnItemAdded += HandleItemAdded;
        playerInventory.OnItemRemoved += HandleItemRemoved;
        playerInventory.OnChanged     += Refresh; 

        Refresh();
    }
}




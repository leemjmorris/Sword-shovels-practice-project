using System;                   
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int capacity = 20;
    public List<Item> items = new List<Item>();
   public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action OnChanged; // CBL: General Change Event
    /// <summary>
    /// CBL: add item
    /// </summary>
    public bool AddItem(Item item)
    {
          if (item == null)
        {
            Debug.LogWarning("[Inventory] null 아이템은 추가할 수 없습니다.");
            return false;
        }

        if (items.Count >= capacity)
        {
            Debug.Log("[Inventory] 용량 초과");
            return false;
        }

        items.Add(item);

        // ? 이벤트 발행 (UI가 자동으로 Refresh)
        OnItemAdded?.Invoke(item);
        OnChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(Item item)
    {
         bool removed = items.Remove(item);
        if (removed)
        {
            // ? 이미 호출하던 제거 이벤트 + 전체 변경 이벤트
            OnItemRemoved?.Invoke(item);
            OnChanged?.Invoke();
        }
        return removed;
    }

    public bool HasItem(Item item)
    {
        return item != null && items.Contains(item);
    }
    public void Clear()
    {
        if (items.Count == 0) return;

        //CBL: all clear event
        for (int i = items.Count - 1; i >= 0; --i)
            OnItemRemoved?.Invoke(items[i]);

        items.Clear();
        OnChanged?.Invoke();
    }
    /// <summary>
    /// 최대 수용량 변경. (슬롯 재생성은 UI 쪽에서 처리)
    /// </summary>
    public void SetCapacity(int newCapacity)
    {
        if (newCapacity < 0) newCapacity = 0;

        capacity = newCapacity;

        OnChanged?.Invoke();
    }
}

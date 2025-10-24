using System;                   
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public bool infiniteCapacity = true;
    public int capacity = 20;
    public List<Item> items = new();
   public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action OnChanged; // CBL: General Change Event
    /// <summary>
    /// CBL: add item
    /// </summary>
    public bool AddItem(Item item)
    {
        
       items.Add(item); OnItemAdded?.Invoke(item); OnChanged?.Invoke(); return true;
    }

    public bool RemoveItem(Item item)
    {
        var ok = items.Remove(item);
        if (ok)
        {
            OnItemRemoved?.Invoke(item);
            OnChanged?.Invoke();
        }
        
        return ok;
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

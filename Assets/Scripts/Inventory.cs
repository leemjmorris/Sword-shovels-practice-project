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
            Debug.LogWarning("[Inventory] null �������� �߰��� �� �����ϴ�.");
            return false;
        }

        if (items.Count >= capacity)
        {
            Debug.Log("[Inventory] �뷮 �ʰ�");
            return false;
        }

        items.Add(item);

        // ? �̺�Ʈ ���� (UI�� �ڵ����� Refresh)
        OnItemAdded?.Invoke(item);
        OnChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(Item item)
    {
         bool removed = items.Remove(item);
        if (removed)
        {
            // ? �̹� ȣ���ϴ� ���� �̺�Ʈ + ��ü ���� �̺�Ʈ
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
    /// �ִ� ���뷮 ����. (���� ������� UI �ʿ��� ó��)
    /// </summary>
    public void SetCapacity(int newCapacity)
    {
        if (newCapacity < 0) newCapacity = 0;

        capacity = newCapacity;

        OnChanged?.Invoke();
    }
}

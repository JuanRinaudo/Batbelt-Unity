using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeightedItem<T>
{
    [Min(0f)]
    public float weight = 1f;
    public T value;

    public WeightedItem() { }

    public WeightedItem(T value, float weight)
    {
        this.value = value;
        this.weight = Mathf.Max(0f, weight);
    }
}

[Serializable]
public class WeightedList<T>
{
    [SerializeField]
    private List<WeightedItem<T>> items = new List<WeightedItem<T>>();

    public List<WeightedItem<T>> Items => items;
    public int Count => items.Count;

    public float TotalWeight
    {
        get
        {
            float total = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                total += Mathf.Max(0f, items[i].weight);
            }
            return total;
        }
    }

    public T GetRandom()
    {
        if (items == null || items.Count == 0)
        {
            throw new InvalidOperationException("Cannot pick from an empty WeightedList.");
        }

        float totalWeight = TotalWeight;
        if (totalWeight <= 0f)
            return items[UnityEngine.Random.Range(0, items.Count)].value;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            cumulative += Mathf.Max(0f, items[i].weight);
            if (roll < cumulative)
                return items[i].value;
        }

        return items[items.Count - 1].value;
    }

    public void Add(T value, float weight = 1f)
    {
        items.Add(new WeightedItem<T>(value, weight));
    }

    public void RemoveAt(int index)
    {
        items.RemoveAt(index);
    }

    public void Clear()
    {
        items.Clear();
    }
}
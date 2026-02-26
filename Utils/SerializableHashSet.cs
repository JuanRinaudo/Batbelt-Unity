using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableHashSet<TValue> : HashSet<TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TValue> values = new List<TValue>();

    // save the hashset to list
    public void OnBeforeSerialize()
    {
        values.Clear();
        foreach (var v in this)
        {
            values.Add(v);
        }
    }

    // load hashset from list
    public void OnAfterDeserialize()
    {
        Clear();
        
        foreach (var v in values)
        {
            Add(v);
        }
    }

}
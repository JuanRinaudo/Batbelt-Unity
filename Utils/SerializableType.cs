using System;
using UnityEngine;

[Serializable]
public class SerializableType
{
    [SerializeField]
    private string typeName;

    public Type Type
    {
        get
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            return Type.GetType(typeName);
        }
        set
        {
            typeName = value != null ? value.AssemblyQualifiedName : null;
        }
    }

    public SerializableType() { }

    public SerializableType(Type type)
    {
        Type = type;
    }

    public override string ToString()
    {
        return Type != null ? Type.FullName : "(null)";
    }

    public static implicit operator Type(SerializableType serializableType)
    {
        return serializableType?.Type;
    }

    public static implicit operator SerializableType(Type type)
    {
        return new SerializableType(type);
    }

    public static bool operator ==(SerializableType a, SerializableType b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.typeName == b.typeName;
    }

    public static bool operator !=(SerializableType a, SerializableType b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj is SerializableType other)
            return this == other;
        if (obj is Type type)
            return Type == type;
        return false;
    }

    public override int GetHashCode()
    {
        return typeName != null ? typeName.GetHashCode() : 0;
    }
}
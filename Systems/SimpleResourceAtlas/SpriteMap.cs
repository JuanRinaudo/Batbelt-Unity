using System;
using UnityEngine;

[Serializable]
public class MappedSpritesDictionary : SerializableDictionary<string, MappedSprite> { }

public class SpriteMap : ScriptableObject
{
	public MappedSpritesDictionary mapping;
}
using System;
using UnityEngine;

[Serializable]
public class SpriteReference {

	public string key;
	public Sprite sprite;

	public void SetSprite(Sprite sprite)
	{
		this.sprite = sprite;
	}

	public static implicit operator Sprite(SpriteReference reference)
	{
		if(reference.sprite == null) {
			reference.sprite = SpriteManager.Instance.GetSprite(reference.key);
		}
		return reference.sprite;
	}

}
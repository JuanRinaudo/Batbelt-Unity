using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureUtils
{

    public static Texture2D CreateTexture2D(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int index = 0; index < pix.Length; index++)
        {
            pix[index] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pix);
        texture.Apply();

        return texture;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TextModifier
{
    NONE,
    LOWER,
    UPPER,
    UP_FIRST_LETTER
}

public static class StringExtension
{    

    public static string ApplyModifier(this string value, TextModifier modifier)
    {
        switch (modifier)
        {
            case TextModifier.LOWER:
                return value.ToLower();
            case TextModifier.UPPER:
                return value.ToUpper();
            case TextModifier.UP_FIRST_LETTER:
                if (value.Length == 0) { return ""; }

                char[] letters = value.ToCharArray();
                letters[0] = char.ToUpper(letters[0]);
                return new string(letters);
            default:
                return value;
        }
    }

}

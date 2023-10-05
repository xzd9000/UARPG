using System;
using UnityEngine;

#pragma warning disable 0649


[CreateAssetMenu] public class LocalizedTextBlock : ScriptableObject
{
    [Serializable] private class LocalizedText
    {
        [HideUnless("useTextAsset",  true)] public TextAsset textAsset;
        [HideUnless("useTextAsset", false)] public string stringText;
        public bool useTextAsset;
        public Localization localization;
    }

    [SerializeField] LocalizedText[] textBlock;

    public string text
    {
        get
        {
            Localization localization;
            try { localization = GameSettings.instance.localization; }
            catch { return ""; }
            LocalizedText text;
            for (int i = 0; i < textBlock.Length; i++)
            {
                text = textBlock[i];
                if (text.localization == localization)
                {
                    if (text.useTextAsset) return text.textAsset.text;
                    else return text.stringText;
                }
            }
            return "";
        }
    }

    static public implicit operator string(LocalizedTextBlock lText) => lText.text;
}
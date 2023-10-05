using UnityEngine;

[System.Serializable] public struct GUIData
{
    public object source;
    public Sprite[] images;
    public string[] texts;

    public GUIData(Sprite image, string text, object source = null)
    {
        this.source = source;
        images = new Sprite[1];
        images[0] = image;
        texts = new string[1];
        texts[0] = text;
    }
    public GUIData(Sprite[] images, string[] texts, object source = null)
    {
        this.source = source;
        this.images = images;
        this.texts = texts;
    }
}
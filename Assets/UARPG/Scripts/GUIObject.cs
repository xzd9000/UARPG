using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

[RequireComponent(typeof(RectTransform))]
public class GUIObject : MonoBehaviour
{
    public const string baseElementExceptionMessage = "No base element was found in children and base element field is not assigned";

    [SerializeField] protected TextAnchor anchor = TextAnchor.UpperLeft;
    [SerializeField] bool hiddenOnStart = false;

    public Player player { get; private set; }
    public Control control { get; private set; }
    public RectTransform rect { get; private set; }
    public Canvas canvas { get; private set; }
    public RectTransform canvasRect { get; private set; }
    public bool hasImages { get; private set; }

    public Image[] images { get; private set; } = new Image[0];
    public Text[] texts { get; private set; } = new Text[0];

    private void Awake()
    {
        player = Global.player;
        control = Global.control;
        if (hiddenOnStart) Hide();
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        images = GetComponentsInChildren<Image>();
        texts = GetComponentsInChildren<Text>();
        AwakeCustom();
    }
    protected virtual void AwakeCustom() { }

    public virtual void Show() { if (!gameObject.activeInHierarchy) gameObject.SetActive(true); }
    public virtual void Hide() { if (gameObject.activeInHierarchy) gameObject.SetActive(false); }

    public void SetColorAll(Color color) { for (int i = 0; i < images.Length; i++) images[i].color = color; }
    public void SetTextAll(string text) { for (int i = 0; i < texts.Length; i++) texts[i].text = text; }
    public void SetText(string text, int index = 0) { if (index >= 0 && index < texts.Length) texts[index].text = text; }
    public void SetTexts(string[] texts) { for (int i = 0; i < this.texts.Length && i < texts.Length; i++) this.texts[i].text = texts[i]; }

    public void SetImageAll(Sprite image) 
    { 
        for (int i = 0; i < images.Length; i++) images[i].sprite = image;
        CheckImages();
    }
    public void SetImage(Sprite image, int index = 0) 
    { 
        if (index >= 0 && index < images.Length) images[index].sprite = image;
        CheckImages();
    }
    public void SetImages(Sprite[] images) 
    { 
        for (int i = 0; i < this.images.Length && i < images.Length; i++) this.images[i].sprite = images[i];
        CheckImages();
    }

    public void SetImageEnabledAll(bool enabled) { for (int i = 0; i < images.Length; i++) images[i].enabled = enabled; }
    public void SetTextEnabledAll(bool enabled) { for (int i = 0; i < texts.Length; i++) texts[i].enabled = enabled; }

    private void CheckImages()
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].sprite != null)
            {
                hasImages = true;
                return;
            }
        }
        hasImages = false;
    }

    public bool IsWithinWindow(Vector3 mouseCoordinates, TextAnchor windowAnchor = TextAnchor.UpperLeft)
    {
        Vector3 scale = ScaleFromAllParents();
        Vector2 windowCenter = new Vector2(rect.position.x, rect.position.y);
        Vector2 size = new Vector2(rect.sizeDelta.x * scale.x, rect.sizeDelta.y * scale.y);
        switch (windowAnchor)
        {
            case TextAnchor.LowerCenter: return window(new Vector2(windowCenter.x - (size.x / 2f), windowCenter.y));
            case TextAnchor.LowerLeft: return window(windowCenter);
            case TextAnchor.LowerRight: return window(new Vector2(windowCenter.x - size.x, windowCenter.y));
            case TextAnchor.MiddleCenter: return window(new Vector2(windowCenter.x - (size.x / 2), windowCenter.y - (size.y / 2)));
            case TextAnchor.MiddleLeft: return window(new Vector2(windowCenter.x, windowCenter.y - (size.y / 2)));
            case TextAnchor.MiddleRight: return window(new Vector2(windowCenter.x - size.x, windowCenter.y - (size.y / 2)));
            case TextAnchor.UpperCenter: return window(new Vector2(windowCenter.x - (size.x / 2), windowCenter.y - size.y));
            case TextAnchor.UpperLeft: return window(new Vector2(windowCenter.x, windowCenter.y - size.y));
            case TextAnchor.UpperRight: return window(new Vector2(windowCenter.x - size.y, windowCenter.y - size.y));
            default: throw new System.Exception("how did this even happen");
        }
        bool window(Vector2 center)
        {
            return (mouseCoordinates.x >= center.x &&
                    mouseCoordinates.x <= center.x + size.x &&
                    mouseCoordinates.y >= center.y &&
                    mouseCoordinates.y <= center.y + size.y);
        }
    }
    public bool IsWithinWindow(Vector3 mouseCoordinates) => IsWithinWindow(mouseCoordinates, anchor);
    public bool IsWithinWindow() => IsWithinWindow(Input.mousePosition, anchor);

    private Vector3 ScaleFromAllParents()
    {
        Vector3 ret = new Vector3(1f, 1f, 1f);
        Transform next = rect.parent;
        while (next is Transform)
        {
            ret.Scale(next.localScale);
            if (next != canvasRect) next = next.parent;
            else break;
        }
        return ret;
    }
}
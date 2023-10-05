using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public class GUIScrollBar : GUIInteractiveObject
{
    [SerializeField] bool horizontal;
    [SerializeField] float indent;

    [SerializeField] GameObject scrollButton;

    private RectTransform scrollButtonRect;
    private Vector3 mouseCoordinates;
    private bool mouseHold;

    protected override void AwakeCustom()
    {
        if (scrollButton == null)
        {
            if ((scrollButton = transform.GetChild(0).gameObject) == null)
            {
                GameObject obj = new GameObject();
                obj.transform.SetParent(transform);
                obj.AddComponent<Image>().color = Global.instance.defaultUIColor;
                scrollButtonRect = obj.GetComponent<RectTransform>();
                scrollButtonRect.anchoredPosition = Vector3.zero;
                scrollButtonRect.localEulerAngles = Vector3.zero;
                float size;
                if (horizontal) size = rect.sizeDelta.y - indent * 2;
                else size = rect.sizeDelta.x - indent * 2;
                scrollButtonRect.sizeDelta = new Vector2(size, size);
                scrollButton = obj;
                return;
            }
        }
        scrollButtonRect = scrollButton.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (mouseHold)
        {
            float position;
            if (horizontal)
            {
                position = Mathf.Clamp(Input.mousePosition.x, rect.position.x + indent, rect.position.x + rect.sizeDelta.x * canvasRect.localScale.x - scrollButtonRect.sizeDelta.x * canvasRect.localScale.x - indent);
                scrollButtonRect.position = new Vector3(position, scrollButtonRect.position.y);
                Scroll?.Invoke(scrollButtonRect.anchoredPosition.x / (rect.sizeDelta.x - indent * 2));
            }
            else
            {
                position = Mathf.Clamp(Input.mousePosition.y, rect.position.y - rect.sizeDelta.y * canvasRect.localScale.y + indent + scrollButtonRect.sizeDelta.y * canvasRect.localScale.y, rect.position.y - indent);
                scrollButtonRect.position = new Vector3(scrollButtonRect.position.x, position);
                Scroll?.Invoke(-scrollButtonRect.anchoredPosition.y / (rect.sizeDelta.y - indent * 2));
            }
        }
    }

    public event System.Action<float> Scroll;

    protected override void OnEnable()
    {
        base.OnEnable();
        control.MouseRelease += MouseRelease;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        control.MouseRelease -= MouseRelease;
        mouseHold = false;
    }

    public void SetPosition(int index, int max)
    {
        if (horizontal) scrollButtonRect.anchoredPosition = new Vector3((int)((rect.sizeDelta.x - indent * 2) / max) * index + indent, scrollButtonRect.anchoredPosition.y);
        else scrollButtonRect.anchoredPosition = new Vector3(scrollButtonRect.anchoredPosition.x, -((int)((rect.sizeDelta.y - indent * 2) / max) * index + indent));
    }

    public override void Interact(Vector3 mouseCoordinates) { if (IsWithinWindow(mouseCoordinates, TextAnchor.UpperLeft)) mouseHold = true; }
    private void MouseRelease() => mouseHold = false;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIBarsDiscrete : GUIBars
{
    [SerializeField] int step;
    [SerializeField] Vector2 offset;
    [SerializeField] int maxObjects;
    [SerializeField] RectTransform firstItem;

    private RectTransform[] items;

    protected override void AwakeCustom2()
    {
        items = new RectTransform[maxObjects];
        for (int i = 0; i < maxObjects; i++)
        {
            if (i > 0)
            {
                items[i] = Instantiate(firstItem.gameObject, firstItem.parent).GetComponent<RectTransform>();
                items[i].anchoredPosition = firstItem.anchoredPosition + offset * i;
            }
            else items[i] = firstItem;
        }
    }

    protected override void SetSizes(float value, float maxValue)
    {
        int count = (int)value / step;

        for (int i = 0; i < maxObjects; i++) items[i].gameObject.SetActive(i < count);        
    }
}

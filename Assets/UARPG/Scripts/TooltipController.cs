using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipController : Singleton<TooltipController>
{
    [SerializeField] GUIObject Tooltip;
    [SerializeField] bool showTooltip;

    public GUIObject tooltip => Tooltip;

    public void ShowTooltip(Vector3 position)
    {
        showTooltip = true;
        Tooltip.rect.position = position;
    }

    private void Update()
    {
        if (showTooltip) Tooltip.Show();
        else Tooltip.Hide();

        showTooltip = false;
    }
}

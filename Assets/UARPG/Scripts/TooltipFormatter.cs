using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TooltipFormatter : ScriptableObject
{
    public abstract void FormatTooltip(GUIData data, GUIObject tooltip);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSource : MonoBehaviour
{
    [SerializeField] MonoBehaviour dataObject;
    [SerializeField] GUIObject source;
    [SerializeField] TooltipFormatter formatter;

    private TooltipController tooltip;
    private bool tooltipExists;
    private IGUIDataProvider dataProvider;

    private bool init = false;

    public void Init(IGUIDataProvider dataProvider, GUIObject source, TooltipFormatter formatter)
    {
        if (!init)
        {
            this.dataProvider = dataProvider;
            this.source = source;
            this.formatter = formatter;

            init = true;
        }
    }

    private void Awake()
    {
        tooltip = FindObjectOfType<TooltipController>();
        tooltipExists = tooltip != null;
        dataProvider = dataObject as IGUIDataProvider;
    }

    private void Update()
    {
        if (tooltipExists && source.IsWithinWindow() && dataProvider.guiData.source != null)
        {
            formatter.FormatTooltip(dataProvider.guiData, tooltip.tooltip);
            tooltip.ShowTooltip(Input.mousePosition);
        }
    }
}

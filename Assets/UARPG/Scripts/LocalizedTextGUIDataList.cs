using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizedTextGUIDataList : GUIDataListComponent<LocalizedTextBlock>
{
    protected override string MakeString(LocalizedTextBlock value) => value.text;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringGUIDataList : GUIDataListComponent<string>
{
    protected override string MakeString(string value) => value;
}

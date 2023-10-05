using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VectorGUIDataList<T> : GUIDataListComponent<T>
{
    [SerializeField] string format;

    protected override string MakeString(T value) => Format(format, value);

    public string Format<I>(string format, params I[] vals)
    {
        string ret = format;
        for (int i = 0; i < vals.Length; i++) ret = ret.Replace("{" + (i + 1).ToString() + "}", vals[i].ToString());
        return ret;
    }
    public abstract string Format(string format, T value);
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GUIDataListComponent<T> : MonoBehaviour, IGUIDataList
{
    [SerializeField] T[] values;

    private void Awake()
    {
        List<GUIData> data = new List<GUIData>(values.Length);
        for (int i = 0; i < data.Capacity; i++) data.Add(new GUIData(null, MakeString(values[i])));
        guiData = data;
        GUIDataChanged?.Invoke();
    }

    public List<GUIData> guiData { get; private set; }

    public event Action GUIDataChanged;

    public T Get(int i) => values[i];

    protected abstract string MakeString(T value);
}

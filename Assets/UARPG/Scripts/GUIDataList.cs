using System;
using System.Collections.Generic;

public class GUIDataList<T> : IGUIDataList where T : IGUIDataProvider
{
    public List<T> objects { get; private set; } = new List<T>();
    public List<GUIData> guiData { get; private set; } = new List<GUIData>();

    public event Action GUIDataChanged;
    public void AddObject(T obj)
    {
        objects.Add(obj);
        guiData.Add(obj.guiData);
        GUIDataChanged?.Invoke();
    }
    public int RemoveObject(T obj, Predicate<T> match = null)
    {
        int i;
        if (match != null) i = objects.FindIndex((t) => t.Equals(obj));
        else i = objects.FindIndex(match);
        RemoveObject(i);
        return i;
    }
    public void RemoveObject(int index)
    {
        objects.RemoveAt(index);
        guiData.RemoveAt(index);
        GUIDataChanged?.Invoke();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomTypeLocalization<T> : ScriptableObject where T : Enum
{
    protected abstract class TypeAndBlock<T2>
    {
        public T2 type;
        public LocalizedTextBlock localizedText;
    }

    protected TypeAndBlock<T>[] array;

    public abstract void Initialize();

    public string GetLocalization(T type)
    {
        int index;
        if ((index = array.FindIndex((t) => t.type.Equals(type))) >= 0) return array[index].localizedText;
        else return type.ToString();
    }
}

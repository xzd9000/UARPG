using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class HideUnlessAttribute : PropertyAttribute
{

    public string conditionalField { get; private set; }
    public int[] intValues { get; private set; }
    public bool boolValue { get; private set; }    
    public Type type { get; private set; }

    public HideUnlessAttribute(string fieldName, bool boolValue)
    {
        conditionalField = fieldName;
        this.boolValue = boolValue;
        type = typeof(bool);
    }

    public HideUnlessAttribute(string fieldName,Type type, params int[] enumValues)
    {
        conditionalField = fieldName;
        intValues = enumValues;
        this.type = type;
    }
}

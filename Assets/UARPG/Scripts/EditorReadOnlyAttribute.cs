using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class EditorReadOnlyAttribute : PropertyAttribute
{
    public EditorReadOnlyAttribute() {  }
}

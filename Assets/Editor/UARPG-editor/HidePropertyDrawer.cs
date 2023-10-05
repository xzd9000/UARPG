using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideUnlessAttribute))]
public class HidePropertyDrawer : PropertyDrawer
{
    private bool defaultHeight;

    private Rect position;
    private SerializedProperty property;
    private GUIContent label;
    private HideUnlessAttribute hideAttribute;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        #region fields
        this.position = position;
        this.property = property;
        this.label = label;
        #endregion

        #region finding path
        hideAttribute = (HideUnlessAttribute)attribute;
        string path = "";
        string[] arr = property.propertyPath.Split('.');
        if (arr.Length > 1)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                path += arr[i] += ".";
            }
        }
        path += hideAttribute.conditionalField;
        SerializedProperty conditionalProperty = property.serializedObject.FindProperty(path);
        if (conditionalProperty == null)
        {

            Debug.LogWarning("Property named \"" + path + "\" was not found");
            DrawPropertyByDefault();
            return;
        }
        #endregion

        HandleHidingByAttributesType(conditionalProperty, hideAttribute.type);
    }

    private void HandleHidingByAttributesType(SerializedProperty property,System.Type type)
    {
        bool condition = false;
        if (type == typeof(int))
        {
            foreach (int num in hideAttribute.intValues)
                if (property.intValue == num)
                    condition = true;
        }
        else if (type == typeof(System.Enum))
        {
            foreach (int num in hideAttribute.intValues)
                if (property.enumValueIndex == num)
                    condition = true;
        }
        else if (type == typeof(bool))
        {
            if (property.boolValue == hideAttribute.boolValue)
                condition = true;
        }

        DrawPropertyConditionally(condition);
    }

    private void DrawPropertyByDefault()
    {
        EditorGUI.PropertyField(position, property, label , true);
        defaultHeight = true;
    }
    private void DrawPropertyConditionally(bool condition)
    {
        if (condition)
        {
            DrawPropertyByDefault();
        }
        else defaultHeight = false;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (defaultHeight == false)
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
        else
        {
            return EditorGUI.GetPropertyHeight(property,label,true);
        }
    }
}

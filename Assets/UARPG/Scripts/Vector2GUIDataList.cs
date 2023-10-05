using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2GUIDataList : VectorGUIDataList<Vector2>
{
    public override string Format(string format, Vector2 value) => Format(format, value.x, value.y);
}

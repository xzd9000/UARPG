using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3GUIDataList : VectorGUIDataList<Vector3>
{
    public override string Format(string format, Vector3 value) => Format(format, value.x, value.y, value.z);
}

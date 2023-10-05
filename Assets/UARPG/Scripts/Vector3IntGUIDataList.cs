using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3IntGUIDataList : VectorGUIDataList<Vector3Int>
{
    public override string Format(string format, Vector3Int value) => Format(format, value.x, value.y, value.z);
}

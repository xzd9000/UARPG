using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2IntGUIDataList : VectorGUIDataList<Vector2Int>
{
    public override string Format(string format, Vector2Int value) => Format(format, value.x, value.y);
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AreaType
{
    circle = 0,
    sphere = 1,
    box = 2,
    cylinder = 3
}

[Serializable] public struct AttackArea
{
    public AreaType type;
    public Vector3 offset;
    public float angle;
    [HideUnless("type", typeof(Enum), 0, 1, 3)] public float radius;
    [HideUnless("type", typeof(Enum),    2   )] public Vector3 halfDimensions;
    [HideUnless("type", typeof(Enum),    2   )] public Vector3 rotation;
    [HideUnless("type", typeof(Enum),    2   )] public bool globalRotation;
    [HideUnless("type", typeof(Enum),    3   )] public float height;
}

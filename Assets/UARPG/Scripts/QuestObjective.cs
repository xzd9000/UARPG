using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Objective
{
    kill = 0,
    obtain = 1,
    move = 2,
    interact = 3
}


[Serializable] public struct QuestObjective
{
                                               public Objective objective;
    [HideUnless("objective", typeof(Enum), 0)] public Character target;
    [HideUnless("objective", typeof(Enum), 1)] public InventoryItemData item;
    [HideUnless("objective", typeof(Enum), 2)] public Vector3 position;
    [HideUnless("objective", typeof(Enum), 2)] public float reachDistance;
    [HideUnless("objective", typeof(Enum), 3)] public InteractiveObject interactiveObject;
                                               /// <summary> y - required, x - completed </summary>
                                               public Vector2 count;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum ExitCondition
{
    time,
    healthPercentage,
    healthAmount,
}

#pragma warning disable 0649

[System.Serializable] public class CombatPhase
{
    public float exitTime;
    public float exitHealth;
    public float newHealthBorder;
    public int[] allowedAttacks;
    public Vector3 startingPosition = Vector3.negativeInfinity;
    public Vector3 startingAngles = Vector3.negativeInfinity;
}
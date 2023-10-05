using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] public struct AttackVariant
{
    public AnimatorOverrideController overrideController;
    public AttackPhase[] phases;
    public HittingObject[] hitObjects;
    public bool targetedHitObjects;
}
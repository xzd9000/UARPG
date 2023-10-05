using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackActivation
{
    no,
    onStart,
    onEndBeforeHold,
    onEndAfterHold
}


[System.Serializable] public struct AttackPhase
{
    public bool targeting;
    public bool hold;
    public bool animationEnd;
    public float holdDuration;
    public Vector3 movement;
    public bool ignoreGravity;
    public bool scaleByMoveSpeed;
    public AttackActivation activation;
    public AttackActivation deactivation;
    public bool allowNextAttack;
}

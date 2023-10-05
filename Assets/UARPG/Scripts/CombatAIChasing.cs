using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class CombatAIChasing : CombatAI
{
    [SerializeField] bool guarding;
    [SerializeField][HideUnless("guarding", true)] float guardingMaxRange;
    [SerializeField] Vector3 startingPosition;
    [SerializeField] Vector3 startingAngles;

    protected override void Awake()
    {
        base.Awake();
        startingAngles = transform.eulerAngles;
        startingPosition = transform.position;
    }

    public override void BaseAction()
    {
        if (!character.currentState.HasFlag(StateFlags.stunned))
        {
            if (aiController.targetExists)
            {
                if (targetLossTime >= 0) targetLossTime = -1;
                if (character.activeAttack == null)
                {
                    if (!(guarding && Vector3.Distance(transform.position, startingPosition) > guardingMaxRange)) RandomAttackOrMove(aiController.target.transform.position);
                }
                else if (character.activePhase.HasValue) AttackMovementAndTargeting();
            }
            else if (guarding)
            {
                if (Vector3.Distance(transform.position, startingPosition) > aiController.destinationReachDistance || Vector3.Angle(transform.eulerAngles, startingAngles) > character.targetingAngle) character.MoveTo(startingPosition);
                else aiController.combat = false;
            }
            else TargetLossTimer();            
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class DirectlyControlledCharacter : Character
{
    [SerializeField] bool _3dTargeting;

    protected override void SetMoveAnimationSpeed()
    {
        if (CurrentMovement > 0)
        {
            if (!scaleMovementAnimationSpeed) anim.SetParameter(Constants.Animator.floatSpeed, 1f);
            else anim.SetParameter(Constants.Animator.floatSpeed, CurrentMovement * movementAnimationScale);
        } 
        else anim.SetParameter(Constants.Animator.floatSpeed, 0f);
    }

    protected override void MoveCustom(Vector3 movement)
    {
        controller.Move(transform.TransformDirection(movement));
        CurrentMovement = Mathf.Max(Mathf.Abs(movement.x), movement.y, Mathf.Abs(movement.z));
    }
    protected override void MoveToCustom(Vector3 position, Vector3 movement)
    {
        transform.RotateToTarget(position, stats.values[rotationSpeedStatIndex].value, false, !_3dTargeting);
        MoveCustom(movement);
    }

    public override bool CanReach(Vector3 position) => true;
    public override float PathLengthTo(Vector3 position) => Vector3.Distance(transform.position, position);
}

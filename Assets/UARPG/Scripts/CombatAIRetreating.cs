using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAIRetreating : CombatAI
{
    private enum RetreatStatus
    {
        readyToAttack = 0,
        backpointSearch = 1,
        anypointSearch = 2,
        retreating = 3,
        retreatImpossible = -1
    }

    [SerializeField] float attackMinDistance = 10f;
    [SerializeField] Vector2 backpointVector = new Vector2(0f, 50f);
    [SerializeField] Vector2 anypointVector = new Vector2(100f, 100f);
    [SerializeField] float backpointSearchTimeout = 1.5f;
    [SerializeField] float anypointSearchTimeout = 3f;
    [SerializeField][EditorReadOnly] RetreatStatus retreatStatus;
    [SerializeField][EditorReadOnly] Vector3 destinationPoint = Vector3.negativeInfinity;
    [SerializeField][EditorReadOnly] float backpointSearchStarted = -1;
    [SerializeField][EditorReadOnly] float anypointSearchStarted = -1;

    public override void BaseAction() => DefaultCheckBaseAction(Action);

    private Ray ray;
    private Vector3 right;
    private float angle;

    private void Action()
    {
             if (retreatStatus == RetreatStatus.readyToAttack)
        {
            destinationPoint = Vector3.negativeInfinity;
            backpointSearchStarted = -1;
            anypointSearchStarted = -1;
            if (!character.activePhase.HasValue)
            {
                if (Vector3.Distance(transform.position, aiController.target.transform.position) >= attackMinDistance) RandomAttackOrMove(aiController.target.transform.position);
                else retreatStatus = RetreatStatus.backpointSearch;
            }
            else AttackMovementAndTargeting();
        }
        else if (retreatStatus == RetreatStatus.backpointSearch)
        {
            if (backpointSearchStarted < 0) backpointSearchStarted = Time.time;
            if (Time.time - backpointSearchStarted <= backpointSearchTimeout)
            {
                if (float.IsNegativeInfinity(destinationPoint.x))
                {
                    ray.origin = transform.position + character.controller.center;
                    ray.direction = transform.DirectionToTarget(aiController.target.transform.position + character.controller.center, false);
                    if (!Physics.SphereCast(ray,
                                            character.controller.radius,
                                            character.controller.radius * 5f,
                                            Physics.DefaultRaycastLayers & ~(1 << gameObject.layer)))
                    {
                        right = Vector3.Cross(ray.direction, Vector3.down);
                        angle = Vector3.SignedAngle(right, Vector3.right, Vector3.up) * Mathf.Deg2Rad;
                        destinationPoint = RandomPoint(Vector3.zero, -backpointVector.x, backpointVector.x, 0f, 0f, -backpointVector.y, 0f, 0f, 0f, 0f);

                        destinationPoint = new Vector3
                        (
                            destinationPoint.x * Mathf.Cos(angle) - destinationPoint.z * Mathf.Sin(angle) + transform.position.x + character.controller.center.x,
                            transform.position.y + character.controller.center.y,
                            destinationPoint.x * Mathf.Sin(angle) + destinationPoint.z * Mathf.Cos(angle) + transform.position.z + character.controller.center.z
                        );

                        destinationPoint = FindValidPont(destinationPoint);
                    }
                    else anypoint();
                }
                else checkPoint();
            }
            else anypoint();
        }
        else if (retreatStatus == RetreatStatus.anypointSearch)
        {
            if (anypointSearchStarted < 0) anypointSearchStarted = Time.time;
            if (Time.time - anypointSearchStarted <= anypointSearchTimeout)
            {
                if (float.IsNegativeInfinity(destinationPoint.x)) destinationPoint = FindValidRandomPoint(anypointVector.x, 0, anypointVector.y);
                else checkPoint();
            }
            else retreatStatus = RetreatStatus.retreatImpossible;
        }
        else if (retreatStatus == RetreatStatus.retreating)
        {
            if (Vector3.Distance(transform.position, destinationPoint) > aiController.destinationReachDistance) character.MoveTo(destinationPoint);
            else retreatStatus = RetreatStatus.readyToAttack;
        }
        else if (retreatStatus == RetreatStatus.retreatImpossible)
        {
            if (!character.activePhase.HasValue) RandomAttackOrMove(aiController.target.transform.position);
            else AttackMovementAndTargeting();
        }

        void checkPoint()
        {
            if (Vector3.Distance(destinationPoint, aiController.target.transform.position) >= attackMinDistance + 1) retreatStatus = RetreatStatus.retreating;
            else destinationPoint = Vector3.negativeInfinity;
        }

        void anypoint()
        {
            retreatStatus = RetreatStatus.anypointSearch;
            destinationPoint = Vector3.negativeInfinity;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAIRelocating : CombatAI
{
    [SerializeField] float relocationMaxDistance = 10;
    [SerializeField] bool readyToAttack = true;
    [SerializeField] bool attackStarted;
    [SerializeField] float relocationPointSearchTimeout = 1.5f;
    [SerializeField][EditorReadOnly] Vector3 relocationPoint = Vector3.negativeInfinity;

    private float relocationPointSearchStarted = -1;

    public override void BaseAction() => DefaultCheckBaseAction(Action);  

    private void Action()
    {
        if (readyToAttack)
        {
            relocationPointSearchStarted = -1;
            if (character.activeAttack == null)
            {
                if (attackStarted)
                {
                    readyToAttack = false;
                    attackStarted = false;
                }
                else RandomAttackOrMove(aiController.target.transform.position);
            }
            else if (character.activePhase.HasValue)
            {
                if (!attackStarted) attackStarted = true;
                AttackMovementAndTargeting();
            }
        }
        else
        {
            if (relocationPointSearchStarted < 0)
            {
                relocationPointSearchStarted = Time.time;
                relocationPoint = Vector3.negativeInfinity;
            }
            if (float.IsNegativeInfinity(relocationPoint.x))
            {
                if (Time.time - relocationPointSearchStarted <= relocationPointSearchTimeout) relocationPoint = FindValidPont(RandomPoint(relocationMaxDistance, 0f, relocationMaxDistance, yOffset: 1f));               
                else readyToAttack = true;
            }
            else
            {
                if (Vector3.Distance(transform.position, relocationPoint) > aiController.destinationReachDistance) character.MoveTo(relocationPoint);
                else readyToAttack = true;
            }
        }
    }
}

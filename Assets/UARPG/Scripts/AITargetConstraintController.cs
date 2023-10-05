using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(LookAtConstraint))]
public class AITargetConstraintController : MonoBehaviour
{
    private AIController ai;
    private bool aiExists;
    private LookAtConstraint constraint;

    private void Awake()
    {
        ai = GetComponentInParent<AIController>();
        aiExists = ai != null;
        constraint = GetComponent<LookAtConstraint>();
    }

    private void OnEnable() { if (aiExists) ai.TargetChange += AddConstraint; }
    private void OnDisable() { if (aiExists) ai.TargetChange -= AddConstraint; }

    private void AddConstraint(AIController controller, Character character, bool exists)
    {
        if (exists)
        {
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = character.transform;
            source.weight = 1;
            constraint.AddSource(source);
            constraint.constraintActive = true;
        }
        else
        {
            if (constraint.sourceCount > 0) constraint.RemoveSource(0);
            constraint.constraintActive = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

#pragma warning disable 0649

[RequireComponent(typeof(ParentConstraint))]
public class ArmorPiece : MonoBehaviour
{
    [SerializeField] PhysicalSlot Slot;

    public PhysicalSlot slot => Slot;

    private ParentConstraint constraint;

    private void Awake() => constraint = GetComponent<ParentConstraint>();

    public void BindTo(Transform transform)
    {
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = transform;
        source.weight = 1f;
        constraint.AddSource(source);
        constraint.constraintActive = true;
    }
}

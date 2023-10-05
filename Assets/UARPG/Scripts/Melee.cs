using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

[RequireComponent(typeof(Collider))]
public class Melee : AProjectile
{
    public override void Deactivate()
    {
        InvokeDeactivated();
        reusable.Remove();
    }

    public override void Deflect() { }
}

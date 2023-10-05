using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttacking
{
    void StartAttack(Attack attack);
    void StartAttackDirected(Attack attack, Vector3 direction);
    void StartAttackTargeted(Attack attack, Vector3 point);
    void BreakAttack();
}

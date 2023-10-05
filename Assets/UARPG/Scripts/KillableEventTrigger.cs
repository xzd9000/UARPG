using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Killable))]
public class KillableEventTrigger : EventTrigger
{
    private Killable killable;

    private void Awake() => killable = GetComponent<Killable>();       

    private void OnEnable() => killable.Died += OnDeath;
    private void OnDisable() => killable.Died -= OnDeath;

    private void OnDeath(IKillable killable) => ActivateTrigger(killable);
}

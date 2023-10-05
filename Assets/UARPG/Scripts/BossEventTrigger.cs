using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossAI))]
public class BossEventTrigger : EventTrigger
{
    [Flags] private enum Trigger
    {
        phaseEnter = 1,
        phaseExit = 1 << 1,
        combatEnter = 1 << 2,
    }

    [SerializeField] Trigger trigger;
    [SerializeField] int phase;

    private BossAI ai;

    private void Awake() => ai = GetComponent<BossAI>();       

    private void OnEnable()
    {
        ai.PhaseChange += PhaseEvent;
        ai.aiController.CombatEnter += CombatEnter;
    }
    private void OnDisable()
    {
        ai.PhaseChange -= PhaseEvent;
        ai.aiController.CombatEnter -= CombatEnter;
    }

    private void PhaseEvent(int phase)
    {
        if ((trigger.HasFlag(Trigger.phaseEnter) && phase == this.phase) || (trigger.HasFlag(Trigger.phaseExit) && phase == this.phase + 1)) ActivateTrigger(ai, phase);
    }
    private void CombatEnter(object o, EventArgs e) { if (trigger.HasFlag(Trigger.combatEnter)) ActivateTrigger(ai); }
}


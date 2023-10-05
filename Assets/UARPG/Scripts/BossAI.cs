using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public abstract class BossAI : CombatAI
{
    [SerializeField] protected int CurrentPhase;
    [SerializeField] protected CombatPhase[] phases;

    protected Action[] actions;
    protected Attack[] allowedAttacks;

    public int currentPhase => CurrentPhase;

    public override void BaseAction()
    {
        if (CurrentPhase < 0 || CurrentPhase >= phases.Length) Debug.LogError("There is no action for a phase indexed " + CurrentPhase);
        else DefaultCheckBaseAction(actions[CurrentPhase]); 
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        character.HealthChange += OnCharacterHit;
        aiController.CombatEnter += OnCombatEnter;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        character.HealthChange -= OnCharacterHit;
        aiController.CombatEnter -= OnCombatEnter;
    }

    private void OnCharacterHit(object sender, EventArgs args)
    {
        if (phases[CurrentPhase].exitHealth > 0 && aiController.character.health <= phases[CurrentPhase].exitHealth) NextPhase();
    }

    private void OnCombatEnter(object o, EventArgs a)
    {
        CurrentPhase = -1;
        NextPhase();
    }

    public void NextPhase() => SetPhase(CurrentPhase + 1);
    public void SetPhase(int phase)
    {
        CurrentPhase = phase;
        if (CurrentPhase < 0 || CurrentPhase >= phases.Length) enabled = false;
        else
        {
            FormAttacks();
            if (phases[CurrentPhase].newHealthBorder > 0) character.healthBorder = phases[CurrentPhase].newHealthBorder;
            if (phaseTimer != null) StopCoroutine(phaseTimer);
            if (phases[CurrentPhase].exitTime > 0)
            {
                phaseTimer = PhaseTimer(phases[CurrentPhase].exitTime);
                StartCoroutine(phaseTimer);
            }
            else phaseTimer = null;
        }
        PhaseChange?.Invoke(CurrentPhase);
    }

    public event Action<int> PhaseChange;

    private void FormAttacks()
    {
        List<Attack> ret = new List<Attack>();
        for (int i = 0; i < attacks.Count; i++)
        {
            if (phases[CurrentPhase].allowedAttacks.Contains(i)) ret.Add(attacks[i]);
        }
        allowedAttacks = ret.ToArray();
    }

    private IEnumerator phaseTimer = null;
    private IEnumerator PhaseTimer(float time)
    {
        yield return new WaitForSeconds(time);
        phaseTimer = null;
        NextPhase();
    }
}

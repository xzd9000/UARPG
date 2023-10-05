using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterEventTrigger : EventTrigger
{
    [Flags]
    private enum CharacterEvents
    {
        Hit = 1,
        HealthChanged = 1 << 1,
        Died = 1 << 2,
        StatsChanged = 1 << 3,
        Moved = 1 << 4,
        EquipmentChanged = 1 << 5,
        SkillsChanged = 1 << 6,
        EffectsChanged = 1 << 7,
        Attacked = 1 << 8,
        Leveled = 1 << 9,
        ResourceChanged = 1 << 10,
        Interacted = 1 << 11,
        UsedSkill = 1 << 12,
        Healed = 1 << 13,
    }

    [SerializeField] CharacterEvents eventFlags;

    private Character character;

    private void Awake() => character = GetComponent<Character>();

    private void OnEnable()
    {
        if (eventFlags.HasFlag(CharacterEvents.Hit)) character.Hit += Hit;
        if (eventFlags.HasFlag(CharacterEvents.Healed)) character.Healing += Healing;
        if (eventFlags.HasFlag(CharacterEvents.HealthChanged)) character.HealthChange += HealthChanged;
        if (eventFlags.HasFlag(CharacterEvents.Died)) character.Death += Died;
        if (eventFlags.HasFlag(CharacterEvents.StatsChanged)) character.StatChange += StatsChanged;
        if (eventFlags.HasFlag(CharacterEvents.Moved)) character.Movement += Moved;
        if (eventFlags.HasFlag(CharacterEvents.EquipmentChanged)) character.EquipmentChange += EquipmentChanged;
        if (eventFlags.HasFlag(CharacterEvents.SkillsChanged)) character.SkillsChange += SkillsChanged;
        if (eventFlags.HasFlag(CharacterEvents.EffectsChanged)) character.EffectChange += EffectsChanged;
        if (eventFlags.HasFlag(CharacterEvents.Attacked)) character.AttackStart += EventType2;
        if (eventFlags.HasFlag(CharacterEvents.Leveled)) character.LevelUp += EventType3;
        if (eventFlags.HasFlag(CharacterEvents.ResourceChanged)) character.ResourceChange += EventType4;
        if (eventFlags.HasFlag(CharacterEvents.Interacted)) character.Interaction += EventType5;
        if (eventFlags.HasFlag(CharacterEvents.UsedSkill)) character.SkillUse += EventType6;
    }
    private void OnDisable()
    {
        if (eventFlags.HasFlag(CharacterEvents.Hit)) character.Hit -= Hit;
        if (eventFlags.HasFlag(CharacterEvents.Healed)) character.Healing -= Healing;
        if (eventFlags.HasFlag(CharacterEvents.HealthChanged)) character.HealthChange -= HealthChanged;
        if (eventFlags.HasFlag(CharacterEvents.Died)) character.Death -= Died;
        if (eventFlags.HasFlag(CharacterEvents.StatsChanged)) character.StatChange -= StatsChanged;
        if (eventFlags.HasFlag(CharacterEvents.Moved)) character.Movement -= Moved;
        if (eventFlags.HasFlag(CharacterEvents.EquipmentChanged)) character.EquipmentChange -= EquipmentChanged;
        if (eventFlags.HasFlag(CharacterEvents.SkillsChanged)) character.SkillsChange -= SkillsChanged;
        if (eventFlags.HasFlag(CharacterEvents.EffectsChanged)) character.EffectChange -= EffectsChanged;
        if (eventFlags.HasFlag(CharacterEvents.Attacked)) character.AttackStart -= EventType2;
        if (eventFlags.HasFlag(CharacterEvents.Leveled)) character.LevelUp -= EventType3;
        if (eventFlags.HasFlag(CharacterEvents.ResourceChanged)) character.ResourceChange -= EventType4;
        if (eventFlags.HasFlag(CharacterEvents.Interacted)) character.Interaction -= EventType5;
        if (eventFlags.HasFlag(CharacterEvents.UsedSkill)) character.SkillUse -= EventType6;
    }

    private void Hit(object o, EventArgs a)
    {
        ObjectEventArgs<Damage, Character> args = a as ObjectEventArgs<Damage, Character>;
        ActivateTrigger(character, args.obj1, args.obj2);
    }
    private void Healing(object o, EventArgs a)
    {
        ObjectEventArgs<float, Character> args = a as ObjectEventArgs<float, Character>;
        ActivateTrigger(character, args.obj1, args.obj2);
    }
    private void HealthChanged(object o, EventArgs a)
    {
        OldNewEventArgs<float, float> args = a as OldNewEventArgs<float, float>;
        ActivateTrigger(character, args.old, args.new_);
    }
    private void Died(object o, EventArgs a) => ActivateTrigger(character);
    private void StatsChanged(object o, EventArgs a)
    {
        OldNewEventArgs<CharacterStats, CharacterStats> args = a as OldNewEventArgs<CharacterStats, CharacterStats>;
        ActivateTrigger(character, args.old, args.new_);
    }
    private void Moved(object o, EventArgs a)
    {
        ObjectEventArgs<Vector3> args = a as ObjectEventArgs<Vector3>;
        ActivateTrigger(character, args.obj);
    }
    private void EquipmentChanged(object o, EventArgs a)
    {
        AddRemoveEventArgs<EquippableItem> args = a as AddRemoveEventArgs<EquippableItem>;
        ActivateTrigger(character, args.obj, args.change);
    }
    private void SkillsChanged(object o, EventArgs a)
    {
        AddRemoveEventArgs<Skill> args = a as AddRemoveEventArgs<Skill>;
        ActivateTrigger(character, args.obj, args.change);
    }
    private void EffectsChanged(object o, EventArgs a)
    {
        AddRemoveEventArgs<Effect> args = a as AddRemoveEventArgs<Effect>;
        ActivateTrigger(character, args.obj, args.change);
    }
    private void EventType2() => ActivateTrigger(character);
    private void EventType3(Character c, int i1, int i2) => ActivateTrigger(c, i1, i2);
    private void EventType4(Character c, CharacterResource r, int i, float f1, float f2) => ActivateTrigger(c, r, i, f1, f2);
    private void EventType5(Character c, InteractiveObject o) => ActivateTrigger(c, o);
    private void EventType6(Character c, Skill s, bool b, Character c2, Vector3 v, VectorParamType p) => ActivateTrigger(c, s, b, c2, v, p);
}
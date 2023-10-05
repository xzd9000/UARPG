using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public struct EffectAction
{
    public enum Action
    {
        damage = 0,
        heal = 1,
        changeHealth = 2,
        overrideHealth = 3,
        changeStats = 4,
        move = 5,
        removeThisEffect = 6,
        applyEffectsToOwner = 7,
        applyEffectsToHitTarget = 8,
        spawnCharacters = 9,
        stun = 10,
        breakStun = 11,
        despell = 12,
        scaleStats = 13,
        scaleDamage = 14,
        scaleResource = 15,
        removeOtherEffects = 16,
    }
    public enum ScaleSource
    {
        stat = 0,
        resource = 1,
        damage = 2,
        health = 3
    }

    public Action action;
    [HideUnless("action", typeof(System.Enum), 1, 2, 3)] public float amount;
    [HideUnless("action", typeof(System.Enum), 10, 11)] public int depth;
    [HideUnless("action", typeof(System.Enum), 0)] public Damage damage;
    [HideUnless("action", typeof(System.Enum), 4)] public CharacterStats statChange;
    [HideUnless("action", typeof(System.Enum), 5)] public Vector3 movement;
    [HideUnless("action", typeof(System.Enum), 13, 14, 15)] public ScaleSource scaleSource;
    [HideUnless("action", typeof(System.Enum), 13, 14, 15)] public float scaleMultiplier;
    [HideUnless("action", typeof(System.Enum), 13, 14, 15)] public float scaleOffset;
    [HideUnless("action", typeof(System.Enum), 13, 14, 15)] public CharacterStat sourceStat;
    [HideUnless("action", typeof(System.Enum), 13, 14, 15)] public CharacterResource sourceResource;
    [HideUnless("action", typeof(System.Enum), 13, 14, 15)] public DamageType sourceDamageType;
    [HideUnless("action", typeof(System.Enum), 13, 14, 15)] public bool multiply;
    [HideUnless("action", typeof(System.Enum), 13)] public CharacterStat targetStat;
    [HideUnless("action", typeof(System.Enum), 14)] public DamageType targetType;
    [HideUnless("action", typeof(System.Enum), 14)] public bool allTypes;
    [HideUnless("action", typeof(System.Enum), 15)] public CharacterResource targetResource;
    [HideUnless("action", typeof(System.Enum), 15)] public bool allowOverflow;
    [HideUnless("action", typeof(System.Enum), 13, 14, 15)] public bool fromPercent;
    public bool periodic;
    public Effect[] effects;
    public Character[] spawns;

}

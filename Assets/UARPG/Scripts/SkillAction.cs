using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class SkillAction
{
    public bool passive;
    public bool activate;
    [HideUnless("passive", false)] public bool onActivation = true;
    [HideUnless("passive", false)] public bool toUser = true;
    public bool hasAttack;
    [HideUnless("hasAttack", true)] public Attack attack;
    public Effect[] effects;
    public HittingObject[] hitObjects;
    public Damage damage;
    public float healing;
}
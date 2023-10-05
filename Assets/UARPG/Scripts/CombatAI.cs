using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public abstract class CombatAI : AI
{
    [SerializeField] bool getAttacksFromWeapons;
    [SerializeField] protected List<Attack> attacks = new List<Attack>();
    [SerializeField] protected float combatTimeOut = 5f;
    [SerializeField] protected float maxPathLength = 100f;

    protected float targetLossTime = -1;
    protected RaycastHit raycastHitBuffer;

    protected override void Awake()
    {
        base.Awake();
        if (getAttacksFromWeapons) UpdateAttacks();
    }

    public override void BaseAction() { }

    /// <summary>
    /// Base action with a prewritten check for character being stunned and target existence
    /// </summary>
    /// <param name="action">Action to execute after check is passed</param>
    protected void DefaultCheckBaseAction(Action action)
    {
        if (!character.currentState.HasFlag(StateFlags.stunned))
        {
            if (aiController.targetExists)
            {
                if (targetLossTime >= 0) targetLossTime = -1;
                action?.Invoke();
            }
            else TargetLossTimer();
        }
    }

    protected void TargetLossTimer()
    {
        if (targetLossTime < 0) targetLossTime = Time.time;
        else if (Time.time - targetLossTime > combatTimeOut)
        {
            aiController.combat = false;
            targetLossTime = -1;
        }
    }

    protected void AttackMovementAndTargeting() 
    {
        AttackTargeting();
        AttackMovement();
    }
    protected void AttackMovement() { if (character.activePhase.Value.movement.sqrMagnitude > 0) character.Move(transform.TransformDirection(character.activePhase.Value.movement * (character.activePhase.Value.scaleByMoveSpeed ? character.stats[character.moveSpeedStatIndex] : 1) * Time.deltaTime * Global.instance.moveSpeedMultiplier)); }
    protected void AttackTargeting() { if (character.activePhase.Value.targeting) { if (transform.AngleToTarget(aiController.target.transform.position, false) > aiController.targetingAngleLimit) transform.RotateToTarget(aiController.target.transform.position, character.stats.values[character.rotationSpeedStatIndex].value * Time.deltaTime * Global.instance.rotationSpeedMultiplier); } }

    protected Vector3 RandomPoint(                float xSpread,          float ySpread,          float zSpread,          float xOffset = 0, float yOffset = 0, float zOffset = 0) => RandomPoint(transform.position, xSpread, ySpread, zSpread, xOffset, yOffset, zOffset);
    protected Vector3 RandomPoint(Vector3 center, float xSpread,          float ySpread,          float zSpread,          float xOffset = 0, float yOffset = 0, float zOffset = 0) => RandomPoint(center, -xSpread, xSpread, -ySpread, ySpread, -zSpread, zSpread, xOffset, yOffset, zOffset);
    protected Vector3 RandomPoint(                float xMin, float xMax, float yMin, float yMax, float zMin, float zMax, float xOffset = 0, float yOffset = 0, float zOffset = 0) => RandomPoint(transform.position, xMin, xMax, yMin, yMax, zMin, zMax, xOffset, yOffset, zOffset);
    protected Vector3 RandomPoint(Vector3 center, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax, float xOffset = 0, float yOffset = 0, float zOffset = 0) => new Vector3
    (
        UnityEngine.Random.Range(center.x + xOffset + xMin, center.x + xOffset + xMax),
        UnityEngine.Random.Range(center.y + yOffset + yMin, center.y + yOffset + yMax),
        UnityEngine.Random.Range(center.z + zOffset + zMin, center.z + zOffset + zMax)
    );
    protected Vector3 FindValidRandomPoint(                float xSpread,          float ySpread,          float zSpread,          float xOffset = 0, float yOffset = 0, float zOffset = 0) => FindValidPont(RandomPoint(xSpread, ySpread, zSpread, xOffset, yOffset, zOffset));
    protected Vector3 FindValidRandomPoint(Vector3 center, float xSpread,          float ySpread,          float zSpread,          float xOffset = 0, float yOffset = 0, float zOffset = 0) => FindValidPont(RandomPoint(center, xSpread, ySpread, zSpread, xOffset, yOffset, zOffset));
    protected Vector3 FindValidRandomPoint(                float xMin, float xMax, float yMin, float yMax, float zMin, float zMax, float xOffset = 0, float yOffset = 0, float zOffset = 0) => FindValidPont(RandomPoint(xMin, xMax, yMin, yMax, zMin, zMax, xOffset, yOffset, zOffset));
    protected Vector3 FindValidRandomPoint(Vector3 center, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax, float xOffset = 0, float yOffset = 0, float zOffset = 0) => FindValidPont(RandomPoint(center, xMin, xMax, yMin, yMax, zMin, zMax, xOffset, yOffset, zOffset));
    protected Vector3 FindValidPont(Vector3 rawPoint)
    {
        Vector3 ret = Vector3.negativeInfinity;
        for (int i = -1; i <= 1; i += 2)
        {
            if (Physics.Raycast(rawPoint, transform.up * i, out raycastHitBuffer))
            {
                if (character.CanReach(raycastHitBuffer.point) && character.PathLengthTo(raycastHitBuffer.point) <= maxPathLength)
                {
                    ret = raycastHitBuffer.point;
                    break;
                }
            }
        }
        return ret;
    }  

    protected virtual void OnEnable() { if (getAttacksFromWeapons) character.EquipmentChange += UpdateAttacks; }
    protected virtual void OnDisable() => character.EquipmentChange -= UpdateAttacks;

    protected void UpdateAttacks(object sender, EventArgs args)
    {
        if (args is AddRemoveEventArgs<EquippableItem> itemChange)
        {
            if (itemChange.obj is Weapon weapon)
            {
                if (itemChange.change == AddRemove.added) attacks.AddRange(weapon.attackInfos);
                else if (itemChange.change == AddRemove.removed) foreach (var attack in weapon.attackInfos) attacks.Remove(attack);
            }
        }
        else UpdateAttacks();
    }
    protected void UpdateAttacks()
    {
        attacks = new List<Attack>();
        for (int i = 0; i < character.equipmentLength; i++) if (character.GetEquippedItem(i) is Weapon weapon) attacks.AddRange(weapon.attackInfos);
    }

    /// <summary>
    /// Selects a random attack and rolls its chance, returns null if failed
    /// </summary>
    protected Attack RandomAttack() => RandomAttack(attacks);
    /// <summary>
    /// Selects a random attack from attacks and rolls its chance, returns null if failed
    /// </summary>
    protected Attack RandomAttack(IList<Attack> attacks)
    {
        if (attacks != null && attacks.Count > 0)
        {
            Attack attack = attacks[UnityEngine.Random.Range(0, attacks.Count)];
            return UnityEngine.Random.Range(0, 100) <= attack.chance ? attack : null;
        }
        else return null;
    }
    /// <summary>
    /// Selects a random attack, checks it and attacks or moves, based on the result
    /// </summary>
    protected void RandomAttackOrMove(Vector3 position)
    {
        Attack attack = RandomAttack();
        AttackOrMove(position, attack);
    }
    /// <summary>
    /// Checks an attack and executes it if succesful, moves to point otherwise
    /// </summary>
    protected void AttackOrMove(Vector3 position, Attack attack)
    {
        if (attack != null)
        {
            if (aiController.targetExists && attack.Check(character, aiController.target))
            {
                character.StartAttack(attack);
            }
            else character.MoveTo(position);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public enum AttackType
{
    singleShot,
    autoShot,
    togglable,
    held
}

[System.Serializable] public class Attack
{
    [SerializeField] string name;
    [SerializeField] int Chance = 100;
    [SerializeField] float Cooldown;
    [SerializeField] bool CooldownOnStart;
    [SerializeField] AttackVariant[] attackChain = new AttackVariant[1];
    [SerializeField] AttackType AttackType;
    [SerializeField] bool AllowInAir = true;
    [SerializeField] bool Animated;
    [SerializeField] bool useInfoBoxes;
    [SerializeField] AttackArea[] areas;
    [SerializeField] InfoBox[] infos;

    [SerializeField] int attackChainIndex = 0;

    public int chance => Chance;
    public AttackVariant activeAttack => attackChain[attackChainIndex];
    public float cooldown => Cooldown;
    public bool allowInAir => AllowInAir;
    public bool animated => Animated;
    public AttackType attackType => AttackType;
    public bool cooldownOnStart => CooldownOnStart;

    public bool enabled { get; private set; } = true;

    public void RaiseAttackChainIndex()
    {
        attackChainIndex++;
        if (attackChainIndex >= attackChain.Length) attackChainIndex = 0;
    }
    public void ResetAttackChainIndex() => attackChainIndex = 0;

    public void Enable()
    {
        enabled = true;
        if (cooldownRoutine != null)
        {
            routineHolder.StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
            routineHolder = null;
        }
    }
    public void Disable(MonoBehaviour routineHolder = null)
    {
        if (cooldownRoutine == null)
        {
            enabled = false;
            if (routineHolder is MonoBehaviour) StartCooldown(routineHolder);
        }
    }
    public void StartCooldown(MonoBehaviour routineHolder)
    {
        if (Cooldown != 0)
        {
            cooldownRoutine = IECooldown();
            this.routineHolder = routineHolder;
            routineHolder.StartCoroutine(cooldownRoutine);
        }
    }

    private IEnumerator cooldownRoutine = null;
    private MonoBehaviour routineHolder = null;
    private IEnumerator IECooldown()
    {
        yield return new WaitForSeconds(Cooldown);
        Enable();
    }

    public bool Check(Character attacker, Character target)
    {
        if (enabled)
        {
            int length1;
            if (useInfoBoxes) length1 = infos.Length;
            else length1 = areas.Length;
            bool ret = false;
            if (length1 == 0) Debug.LogWarning("Attack does not have any areas/info boxes and check will always return false");
            for (int i = 0; i < length1; i++)
            {
                if (useInfoBoxes)
                {
                    int length2 = infos[i].ObjectsLength;
                    for (int ii = 0; ii < length2; ii++) if (infos[i][ii] == target.gameObject) return true;
                }
                else
                {
                    AttackArea area = areas[i];
                    switch (area.type)
                    {
                        case AreaType.circle:
                        case AreaType.cylinder:
                            Vector2 attacker2 = new Vector2(attacker.transform.position.x + area.offset.x, attacker.transform.position.z + area.offset.z);
                            Vector2 target2 = new Vector2(target.transform.position.x, target.transform.position.z);
                            ret = Vector2.Distance(attacker2, target2) <= area.radius;
                            if (area.type == AreaType.cylinder && ret == true)
                            {
                                float h = target.transform.position.y - (attacker.transform.position.y + area.offset.y);
                                ret = h <= area.height && h >= 0;
                            }
                            break;
                        case AreaType.sphere: ret = Vector3.Distance(attacker.transform.position + area.offset, target.transform.position) <= area.radius; break;
                        case AreaType.box:
                            Vector3 position = attacker.transform.InverseTransformPoint(target.transform.position);
                            ret =
                                (
                                    position.x <=  area.halfDimensions.x + area.offset.x &&
                                    position.x >= -area.halfDimensions.x + area.offset.x &&

                                    position.y <=  area.halfDimensions.y + area.offset.y &&
                                    position.y >= -area.halfDimensions.y + area.offset.y &&

                                    position.z <=  area.halfDimensions.z + area.offset.z &&
                                    position.z >= -area.halfDimensions.z + area.offset.z
                                );
                            break;
                        default: break;
                    }
                    if (area.angle > 0) ret &= attacker.transform.AngleToTarget(target.transform.position, true) <= area.angle;
                    if (ret == true) return ret;
                }
            }
        }
        return false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class Weapon : EquippableItem
{
    [SerializeField] Attack[] AttackInfos;

    public Attack[] attackInfos => AttackInfos;

    private Character bearer;
    private bool equipped;

    protected override void EquipCustom(Character character)
    {
        bearer = character;
        equipped = true;
        if (animated) bearer.anim.AddAnimator(anim);     
    }

    public override void Destroy() => DestroyGameObject();

    private void OnDestroy() { if (equipped && animated) bearer.anim.RemoveAnimator(anim); }
}

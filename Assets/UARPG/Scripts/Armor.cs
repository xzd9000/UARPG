using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class Armor : EquippableItem
{
    [SerializeField] ArmorPiece[] Pieces;

    public ArmorPiece[] pieces
    {
        get
        {
            ArmorPiece[] ret = new ArmorPiece[Pieces.Length];
            Pieces.CopyTo(ret, 0);
            return ret;
        }
    }

    protected override void EquipCustom(Character character)
    {
        if (animated && character.animated && physicalSlot != PhysicalSlot.none) anim.runtimeAnimatorController = character.anim.runtimeAnimator;        
        for (int i = 0; i < Pieces.Length; i++)
        {
            if (Pieces[i] != null)
            {
                if (Pieces[i].slot != PhysicalSlot.none) Pieces[i].BindTo(character.slots[Pieces[i].slot].transform);
                else throw new UnassignedSlotException("Cant equip item because slot is none");                
            }
        }        
    }
    public override void Destroy()
    {
        for (int i = 0; i < Pieces.Length; i++) if (Pieces[i] != null) Destroy(Pieces[i].gameObject);
        DestroyGameObject();
    }
}

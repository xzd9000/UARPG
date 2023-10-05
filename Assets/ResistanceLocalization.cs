using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu] public class ResistanceLocalization : CustomTypeLocalization<DamageType>
{
    [Serializable] private class ResistanceBlock : TypeAndBlock<DamageType> { }

    [SerializeField] ResistanceBlock[] text;

    public override void Initialize() => array = text;
}

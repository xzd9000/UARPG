using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu] public class DamageTypeLocalization : CustomTypeLocalization<DamageType>
{
    [Serializable] private class DamageBlock : TypeAndBlock<DamageType> { }

    [SerializeField] DamageBlock[] text;

    public override void Initialize() => array = text;
}

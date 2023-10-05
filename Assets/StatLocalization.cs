using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu] public class StatLocalization : CustomTypeLocalization<CharacterStat>
{
    [Serializable] private class StatBlock : TypeAndBlock<CharacterStat> { }

    [SerializeField] StatBlock[] text;

    public override void Initialize() => array = text;
}

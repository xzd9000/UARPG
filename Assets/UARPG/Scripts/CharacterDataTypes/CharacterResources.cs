using UnityEngine;
using System.Collections.Generic;
using static System.Convert;

[System.Serializable] public struct ResourceValuePair : IEnumValuePair<CharacterResource>
{
    [SerializeField] CharacterResource Type;
    [SerializeField] float Value;

    public CharacterResource type { get => Type; set => Type = value; }
    public float value { get => Value; set => Value = value; }

    public ResourceValuePair(CharacterResource type, float value)
    {
        Type = type;
        Value = value;
    }
}

[System.Serializable] public class CharacterResources : CharacterData<CharacterResources, CharacterResource, ResourceValuePair>
{
    [SerializeField] bool Percentage;
    [SerializeField] ResourceValuePair[] floats = new ResourceValuePair[0];

    public override bool percentage { get => Percentage; set => Percentage = value; }

    public override ResourceValuePair[] values { get => floats; protected set => floats = value; }

    public CharacterResources(bool percentage, params CharacterResource[] resources)
    {
        Percentage = percentage;
        floats = new ResourceValuePair[resources.Length];
        for (int i = 0; i < resources.Length; i++)
        {
            floats[i].type = resources[i];
            floats[i].value = 0;
        }
    }
    public CharacterResources() { }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Convert;

[System.Serializable] public struct DamageWithType : IEnumValuePair<DamageType>
{
    [SerializeField] DamageType Type;
    [SerializeField] float Value;

    public DamageType type { get => Type; set => Type = value; }
    public float value { get => Value; set => Value = value; }

    public DamageWithType(DamageType type, float value)
    {
        Type = type;
        Value = value;
    }
}

public abstract class DamageBase : CharacterData<DamageBase, DamageType, DamageWithType>
{
    [SerializeField] bool Percentage;
    [SerializeField] DamageWithType[] floats = new DamageWithType[0];

    public override bool percentage { get => Percentage; set => Percentage = value; }

    public override DamageWithType[] values { get => floats; protected set => floats = value; }
}

[System.Serializable] public class DamageResistance : DamageBase 
{
    public DamageResistance() { }
    public DamageResistance(bool percentage, params DamageType[] types)
    {
        this.percentage = percentage;
        values = new DamageWithType[types.Length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i].type = types[i];
            values[i].value = 0;
        }
    }
}

[System.Serializable] public class Damage : DamageBase
{
    [SerializeField] float Threat;

    public float threat => Threat;

    public void Resist(DamageResistance resistance)
    {
        int index;
        for (int i = 0; i < resistance.values.Length; i++)
        {
            index = FindIndex(resistance.values[i].type);
            if (index != -1)
            {
                values[index].value = 
                    Mathf.Clamp(ResistanceCalculation.resistanceFormulas[values[index].type].Invoke(
                                values[index].value, resistance.values[i].value), 
                                Global.instance.minDamageValue, float.PositiveInfinity);
            }
        }
    }

    public Damage() { }
    public Damage(bool percentage, params DamageType[] types)
    {
        this.percentage = percentage;
        values = new DamageWithType[types.Length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i].type = types[i];
            values[i].value = 0;
        }
        Threat = Sum();
    }
    public Damage(bool percentage, float threat, params DamageType[] types) : this(percentage, types) { Threat = threat; }

    private const char valuesEnd = '&';

    public override string MakeSaveData()
    {
        string ret = base.MakeSaveData() + "\n" + valuesEnd + "\n" + Threat.ToString();
        return ret;
    }
    public override void ReadSaveData(string saveData)
    {
        string values = "";
        string threat = "";
        int i;
        for (i = 0; saveData[i] != valuesEnd && i < saveData.Length; i++) values += saveData[i];
        base.ReadSaveData(values);
        while (i < saveData.Length) { threat += saveData[i]; i++; }
        Threat = ToSingle(threat);
    }

    public float Sum() 
    {
        float ret = 0;
        for (int i = 0; i < values.Length; i++) ret += values[i].value;
        return ret;
    }
}

using System;
using UnityEngine;
using static System.Convert;

[Serializable] public struct FloatStatPair : IEnumValuePair<CharacterStat>
{
    [SerializeField] CharacterStat Type;
    [SerializeField] float Value;

    public CharacterStat type { get => Type; set => Type = value; }
    public float value { get => Value; set => Value = value; }

    public FloatStatPair(CharacterStat type, float value) : this()
    {
        this.type = type;
        this.value = value;
    }
}

[Serializable] public class CharacterStats : CharacterData<CharacterStats, CharacterStat, FloatStatPair>
{

    [SerializeField] bool Percentage;
    [SerializeField] FloatStatPair[] floats = new FloatStatPair[0];
    [SerializeField] Damage Damage = new Damage();
    [SerializeField] DamageResistance Resistance = new DamageResistance();
    [SerializeField] CharacterResources MaxResources = new CharacterResources();
    [SerializeField] HitObjectContactFlags HitObjectFlags;

    public Damage damage => Damage;
    public DamageResistance resistance => Resistance;
    public CharacterResources maxResources => MaxResources;
    public HitObjectContactFlags hitObjectFlags => HitObjectFlags;

    public override bool percentage
    {
        get => Percentage;
        set
        {
            Percentage = value;
            Damage.percentage = value;
            Resistance.percentage = value;
            MaxResources.percentage = value;
        }
    }

    public override FloatStatPair[] values { get => floats; protected set => floats = value; }

    public override void Add(CharacterStats second)
    {
        if (second != null)
        {
            base.Add(second);
            Damage.Add(second.Damage);
            Resistance.Add(second.Resistance);
            HitObjectFlags |= second.HitObjectFlags;
        }
    }
    public override void Subtract(CharacterStats second)
    {
        if (second != null)
        {
            base.Subtract(second);
            Damage.Subtract(second.Damage);
            Resistance.Subtract(second.Resistance);
        }
    }
    public override void Override(CharacterStats override_)
    {
        base.Override(override_);
        Damage.Override(override_.Damage);
        Resistance.Override(override_.Resistance);
        MaxResources.Override(override_.MaxResources);
        if (!HitObjectFlags.HasFlag(HitObjectContactFlags.disallowOverride)) HitObjectFlags = override_.HitObjectFlags;
    }
    public override void Override(float override_)
    {
        base.Override(override_);
        Damage.Override(override_);
        Resistance.Override(override_);
        MaxResources.Override(override_);
    }

    public override void CreateEmptyAllTypes()
    {
        base.CreateEmptyAllTypes();
        Damage.CreateEmptyAllTypes();
        Resistance.CreateEmptyAllTypes();
        MaxResources.CreateEmptyAllTypes();
        HitObjectFlags = HitObjectContactFlags.nothing;
    }

    public CharacterStats() { }
    public CharacterStats(bool percentage, Damage damage, DamageResistance resistance, CharacterResources resources, params CharacterStat[] stats) : this(percentage, stats)
    {
        Damage = damage == null ? new Damage() : damage;
        Resistance = resistance == null ? new DamageResistance() : resistance;
        MaxResources = resources == null ? new CharacterResources() : resources;       
    }
    public CharacterStats(bool percentage, params CharacterStat[] stats)
    {
        Percentage = percentage;
        floats = new FloatStatPair[stats.Length];
        for (int i = 0; i < stats.Length; i++)
        {
            floats[i].type = stats[i];
            floats[i].value = 0;
        }
    }

    private const string end = "end";

    public override string MakeSaveData()
    {
        string ret = Damage.MakeSaveData() + "\n" + 
                     end + "\n" + 
                     Resistance.MakeSaveData() + "\n"
                     + end + "\n" +
                     MaxResources.MakeSaveData() + "\n" 
                     + end + "\n";
        ret += base.MakeSaveData();
        return ret;
    }

    public override void ReadSaveData(string saveData)
    {
        string damage = "";
        string resistance = "";
        string resources = "";
        string floats = "";
        string[] lines = saveData.Split('\n');
        int i;
        for (i = 0; lines[i] != end; i++) damage += lines[i];
        for (i++; lines[i] != end; i++) resistance += lines[i];
        for (i++; lines[i] != end; i++) resources += lines[i];
        for (i++; i < lines.Length; i++) floats += lines[i];
        Damage.ReadSaveData(damage);
        Resistance.ReadSaveData(resistance);
        base.ReadSaveData(floats);
    }

    public override void CopyFrom(CharacterStats data)
    {
        base.CopyFrom(data);
        Damage.CopyFrom(data.Damage);
        Resistance.CopyFrom(data.resistance);
        HitObjectFlags = data.HitObjectFlags;
        MaxResources.CopyFrom(data.MaxResources);
    }

    public override string ToString() => (percentage ? "percentages" : "values") + "\n" + base.ToString() + "\n" + Damage.ToString() + "\n" + Resistance.ToString() + "\n" + MaxResources.ToString() + "\n" + HitObjectFlags.ToString();
    
}
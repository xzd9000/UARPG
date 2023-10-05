using System;
using System.Collections.Generic;
using static System.Convert;

public interface IEnumValuePair<T>
{
    T type { get; set; }
    float value { get; set; }
}

public abstract class CharacterData<TDataClass, TDataTypesEnum, TEnumValue> : ISaveDataProvider where TDataClass : CharacterData<TDataClass, TDataTypesEnum, TEnumValue> where TEnumValue : IEnumValuePair<TDataTypesEnum>, new() where TDataTypesEnum : Enum
{
    public abstract TEnumValue[] values { get; protected set; }
    public abstract bool percentage { get; set; }

    public virtual float FindValue(TDataTypesEnum type)
    {
        for (int i = 0; i < values.Length; i++) if (values[i].type.Equals(type)) return values[i].value;
        return float.NegativeInfinity;
    }
    public virtual int FindIndex(TDataTypesEnum type)
    {
        for (int i = 0; i < values.Length; i++) if (values[i].type.Equals(type)) return i;
        return -1;
    }

    public virtual void Add(TDataClass second) => AddOrSubtract(second, (first, second_) => first + second_, (first, second_) => first * second_);
    public virtual void Subtract(TDataClass second) => AddOrSubtract(second, (first, second_) => first - second_, (first, second_) => first / second_);
    private void AddOrSubtract(TDataClass second, Func<float, float ,float> op1, Func<float, float, float> op2)
    {
        if (second != null)
        {
            if (percentage == second.percentage) CommitOperation(second, op1);
            else if (!percentage && second.percentage) CommitOperation(second, op2);
        }
    }

    public virtual void Override(TDataClass override_) => CommitOperation(override_, (value, override__) => override__);
    public virtual void Override(float override_) => CommitOperation(override_, (value, override__) => override__);

    public void Clamp(float value, bool max) => Clamp<float>(value, max);
    public void Clamp(TDataClass value, bool max) => Clamp<TDataClass>(value, max);
    public void Clamp(TDataClass min, TDataClass max, bool clampMin = true, bool clampMax = true) => Clamp<TDataClass>(min, max, clampMin, clampMax);
    public void Clamp(float min, float max, bool clampMin = true, bool clampMax = true) => Clamp<float>(min, max, clampMin, clampMax);
    private void Clamp<T>(T value, bool max)
    {
        if (max) Clamp(default, value, false, true);
        else Clamp(value, default, true, false);
    }
    protected virtual void Clamp<T>(T min, T max, bool clampMin, bool clampMax)
    {
        object min_;
        object max_;
        Func<float, float, float>[] ops =
        {
            (value, min___) =>
            {
                if (value < min___) return min___;
                else return value;
            },
            (value, max___) =>
            {
                if (value > max___) return max___;
                else return value;
            }
        };
        if (min is float min__ && max is float max__)
        {
            min_ = min__;
            max_ = max__;
            if (clampMin) CommitOperation(min__, ops[0]);
            if (clampMax) CommitOperation(max__, ops[1]);
        }
        else if (min is TDataClass min___ && max is TDataClass max___)
        {
            min_ = min___;
            max_ = max___;
            if (clampMin) CommitOperation(min___, ops[0]);
            if (clampMax) CommitOperation(max___, ops[1]);
        }
        else return;        
    }

    /// <param name="operation">first param is this value, second is operands value</param>
    protected void CommitOperation(float operand, Func<float, float, float> operation) { for (int i = 0; i < values.Length; i++) values[i].value = operation(values[i].value, operand); }

    /// <param name="operation">first param is this value, second is operands value</param>
    protected void CommitOperation(TDataClass operandValues, Func<float, float, float> operation)
    {
        if (operandValues != null)
        {
            if (operandValues.values != null)
            {
                for (int i = 0; i < operandValues.values.Length; i++)
                {
                    for (int ii = 0; ii < values.Length; ii++)
                    {
                        if (values[ii].type.Equals(operandValues.values[i].type))
                        {
                            values[ii].value = operation(values[ii].value, operandValues.values[i].value);
                            break;
                        }
                    }
                }
            }
        }
    }

    public virtual void CopyFrom(TDataClass data)
    {
        if (data != null)
        {
            values = new TEnumValue[data.values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = new TEnumValue();
                values[i].type = data.values[i].type;
                values[i].value = data.values[i].value;
            }
            percentage = data.percentage;
        }
    }

    public float this[int i]
    {
        get
        {
            if (values != null) if (i >= 0 && i < values.Length) return values[i].value;
            return float.NegativeInfinity;
        }
        set  { if (values != null) if (i >= 0 && i < values.Length) values[i].value = value; }
    }

    public virtual void CreateEmptyAllTypes()
    {
        Array values = Enum.GetValues(typeof(TDataTypesEnum));
        this.values = new TEnumValue[values.Length];
        TEnumValue value;
        for (int i = 0; i < values.Length; i++)
        {
            value = new TEnumValue();
            value.type = (TDataTypesEnum)values.GetValue(i);
            value.value = 0f;
            this.values.SetValue(value, i);
        }
    }

    public virtual string MakeSaveData()
    {
        string ret = "";
        ret += ToInt32(percentage).ToString() + "\n";
        for (int i = 0; i < values.Length; i++)
        {           
            ret += ChangeType(values[i].type, typeof(int)) + " " + values[i].value;
            if (i != values.Length - 1) ret += "\n";
        }
        return ret;
    }

    public virtual void ReadSaveData(string saveData)
    {
        string[] lines = saveData.Split('\n');
        values = new TEnumValue[lines.Length];
        string[] words;
        percentage = ToBoolean(ToInt32(lines[0]));
        for (int i = 1; i < lines.Length; i++)
        {
            words = lines[i].Split(' ');
            values[i] = new TEnumValue();
            values[i].type = (TDataTypesEnum)ChangeType(ToInt32(words[0]), typeof(TDataTypesEnum));
            values[i].value = ToSingle(words[1]);
        }
    }

    public override string ToString()
    {
        string ret = "";
        for (int i = 0; i < values.Length; i++)
        {
            ret += values[i].value + " " + values[i].type;
            if (i < values.Length - 1) ret += "\n";
        }
        return ret;
    }
}
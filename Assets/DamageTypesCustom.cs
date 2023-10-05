using System;
using System.Collections.Generic;

public enum DamageType
{
    physical
}

public static class ResistanceCalculation
{
    public static readonly Dictionary<DamageType, Func<float, float, float>> resistanceFormulas = new Dictionary<DamageType, Func<float, float, float>>
    {
        [DamageType.physical] = (inc, res) => inc - (0.00065f * res * inc + (3.5f * res / inc))
    };
}
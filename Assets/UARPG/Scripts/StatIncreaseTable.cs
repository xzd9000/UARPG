using UnityEngine;

public class StatIncreaseTable : ScriptableObject
{
    [System.Serializable] private struct LevelStats
    {
        public int level;
        public CharacterStats statsIncrease;
    }

    [SerializeField] LevelStats[] statIncrease;

    public CharacterStats GetStatIncrease(int level)
    {
        for (int i = 0; i < statIncrease.Length; i++)
        {
            if (statIncrease[i].level == level) return statIncrease[i].statsIncrease;
        }
        return new CharacterStats();
    }
}
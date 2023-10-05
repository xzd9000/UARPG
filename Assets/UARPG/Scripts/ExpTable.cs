using System;
using UnityEngine;
using System.Collections.Generic;
using static System.Convert;

[CreateAssetMenu()]
public class ExpTable : ScriptableObject
{
    [Serializable] private struct ExpLevel
    {
        public int level;
        public float expRequired;

        public ExpLevel(int level, float expRequired)
        {
            this.level = level;
            this.expRequired = expRequired;
        }
    }

    [SerializeField] ExpLevel[] levelRequirements;
    [SerializeField] TextAsset textAsset;

    private void Awake() => Sort();

    private void Sort() => Array.Sort(levelRequirements, (first, second) => first.level.CompareTo(second.level));

    [ContextMenu("Read from text file")]
    private void ReadTextAsset()
    {
        if (textAsset != null)
        {
            string[] lines = textAsset.text.Split('\n');
            if (lines.Length > 0)
            {
                levelRequirements = new ExpLevel[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] rows = lines[i].Split(' ');
                    levelRequirements[i] = new ExpLevel(ToInt32(rows[0]), Mathf.Round(ToSingle(rows[1])));
                }
                Sort();
            }
        }
    }

    public int GetLevel(float exp)
    {
        for (int i = levelRequirements.Length - 1; i >= 0; i--)
        {
            if (exp >= levelRequirements[i].expRequired) return levelRequirements[i].level;
        }
        return 1;
    }
    public float GetRequiredExp(int level)
    {
        for (int i = 0; i < levelRequirements.Length; i++)
        {
            if (levelRequirements[i].level == level) return levelRequirements[i].expRequired;
        }
        return 0f;
    }
}


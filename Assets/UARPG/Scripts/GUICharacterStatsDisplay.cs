using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GUICharacterStatsDisplay : GUIObject
{
    private interface IDataToDispay<T> where T : System.Enum 
    { 
        T type { get; } 
        int index { get; set; }
        string text { get; }
    }

    [System.Serializable] private class StatToDisplay : IDataToDispay<CharacterStat>
    {
        [EditorReadOnly] public int Index = -1;
        public CharacterStat stat;
        public LocalizedTextBlock localizedText;

        public CharacterStat type => stat;
        public int index { get => Index; set => Index = value; }
        public string text => localizedText.text;
    }
    [System.Serializable] private class DamageTypeToDisplay : IDataToDispay<DamageType>
    {
        [EditorReadOnly] public int Index = -1;
        public DamageType damageType;
        public LocalizedTextBlock localizedText;

        public DamageType type => damageType;
        public int index { get => Index; set => Index = value; }
        public string text => localizedText.text;
    }
    [System.Serializable] private class ResourceTypeToDisplay : IDataToDispay<CharacterResource>
    {
        [EditorReadOnly] public int Index = -1;
        public CharacterResource resource;
        public LocalizedTextBlock localizedText;

        public CharacterResource type => resource;
        public int index { get => Index; set => Index = value; }
        public string text => localizedText.text;
    }

    [SerializeField] Character character;
    [SerializeField] StatToDisplay[] statsToDisplay;
    [SerializeField] DamageTypeToDisplay[] damageTypesToDisplay;
    [SerializeField] DamageTypeToDisplay[] resistancesToDisplay;
    [SerializeField] ResourceTypeToDisplay[] resourcesToDisplay;
    [SerializeField] LocalizedTextBlock max;
    [SerializeField] LocalizedTextBlock damage;
    [SerializeField] LocalizedTextBlock resistance;

    private CharacterStats stats;

    protected override void AwakeCustom()
    {
        stats = character.stats;
        AssignIndexes(statsToDisplay, stats);
        AssignIndexes(damageTypesToDisplay, stats.damage);
        AssignIndexes(resistancesToDisplay, stats.resistance);
        AssignIndexes(resourcesToDisplay, stats.maxResources);
    }

    private void Update()
    {
        texts[0].text = "";
        texts[1].text = "";
        AddTexts(texts[0], texts[1], statsToDisplay, stats);
        AddTexts(texts[0], texts[1], resourcesToDisplay, stats.maxResources, max.text);
        if (damageTypesToDisplay.Length > 0)
        {
            texts[0].text += "\n" + damage.text + ":\n";
            AddTexts(texts[0], texts[1], damageTypesToDisplay, stats.damage);
        }
        if (resourcesToDisplay.Length > 0)
        {
            texts[0].text += "\n" + resistance.text + ":\n";
            AddTexts(texts[0], texts[1], resistancesToDisplay, stats.resistance);
        }
    }

    private void AssignIndexes<T1, T2, T3>(IDataToDispay<T2>[] toDispay,CharacterData<T1, T2, T3> data) where T1 : CharacterData<T1, T2, T3> where T2 : System.Enum where T3 : IEnumValuePair<T2>, new()
    {
        for (int i = 0; i < toDispay.Length; i++) toDispay[i].index = System.Array.FindIndex(data.values, (e) => e.type.Equals(toDispay[i].type));
    }
    private void AddTexts<T1, T2, T3>(Text text1, Text text2, IDataToDispay<T2>[] toDispay, CharacterData<T1, T2, T3> data, string lineIntro = "") where T1 : CharacterData<T1, T2, T3> where T2 : System.Enum where T3 : IEnumValuePair<T2>, new()
    {
        for (int i = 0; i < toDispay.Length; i++)
        {
            text1.text += lineIntro + toDispay[i].text + '\n';
            text2.text += data[toDispay[i].index].ToString() + '\n';
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

[RequireComponent(typeof(RectTransform))]
public class GUIBars : GUIObject {

    private enum Bar
    {
        HP,
        resource,
        maxHP,
        maxResource
    }

    [ContextMenu("Update")]
    void UpdateUI() { if (character != null) OnStatChange(null, null); }

    [SerializeField] Bar bar;
    [SerializeField] bool percentage = false;
    [SerializeField] [HideUnless("percentage", true)] float maxWidth = 100f;
    [SerializeField][HideUnless("percentage", false)] float pointsPerUnit = 5f;
    [SerializeField][HideUnless("bar", typeof(System.Enum), 1, 3)] CharacterResource resource;
    [SerializeField] Character character;

    private int resourceIndex;
    private int maxResourceIndex;

    protected sealed override void AwakeCustom()
    {
        if (character == null) character = GetComponentInParent<Character>();
        if (bar == Bar.resource || bar == Bar.maxResource)
        {
            resourceIndex = character.GetResourceIndex(resource);
            maxResourceIndex = character.stats.maxResources.FindIndex(resource);
        }
        AwakeCustom2();
    }
    protected virtual void AwakeCustom2() { }

    private void Start() => UpdateBars(); 

    private void OnEnable()
    {
        if (character != null)
        {
            character.StatChange += OnStatChange;
            character.HealthChange += OnHealthChange;
            character.ResourceChange += OnResourceChange;
        }
    }
    private void OnDisable()
    {
        if (character != null)
        {
            character.StatChange -= OnStatChange;
            character.HealthChange -= OnHealthChange;
            character.ResourceChange -= OnResourceChange;
        }
    }

    private void OnStatChange(object sender, System.EventArgs args) => UpdateBars();
    private void OnHealthChange(object sender, System.EventArgs args) => UpdateBars();
    private void OnResourceChange(Character c, CharacterResource resource, int i, float old, float new_) => UpdateBars();
    private void UpdateBars()
    {
        float value = 1;
        float maxValue = 1;

        switch (bar)
        {
            case Bar.HP:
                value = character.health;
                maxValue = character.stats.values[character.maxHealthStatIndex].value;
                break;
            case Bar.resource:
                value = character.GetResource(resourceIndex);
                maxValue = character.stats.maxResources.values[maxResourceIndex].value;
                break;
            case Bar.maxHP:
                value = character.stats.values[character.maxHealthStatIndex].value;
                maxValue = value;
                break;
            case Bar.maxResource:
                value = character.stats.maxResources.values[maxResourceIndex].value;
                maxValue = value;
                break;
            default:
                break;
        }

        if (maxValue != 0) SetSizes(value, maxValue);
                  
    }

    protected virtual void SetSizes(float value, float maxValue)
    {
        float num = rect.sizeDelta.x;
        if (!percentage) num = value / pointsPerUnit;
        else num = maxWidth * (value / maxValue);
        rect.sizeDelta = new Vector2(num, rect.sizeDelta.y);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EffectTrigger
{
    none,
    attackHit,
    weaponHit,
    skillHit,
    damageTaken,
    characterMove,
}

#pragma warning disable 0649

[CreateAssetMenu] public class Effect : ScriptableObject, IGUIDataProvider
{
    public CharacterStats statChange { get; private set; } = new CharacterStats();
    public CharacterStats statChangePercent { get; private set; } = new CharacterStats();
    public float timeLeft { get; private set; }
    public bool stun { get; private set; }
    public int stunDepth { get; private set; }

    [SerializeField][EditorReadOnly] int ID;
    [SerializeField] LocalizedTextBlock Name;
    [SerializeField] Sprite Icon;
    [SerializeField] EffectAction[] actions = new EffectAction[0];
    [SerializeField] EffectTrigger Trigger;
    [SerializeField] EffectTrigger EndTrigger;
    [SerializeField] float Duration;
    [SerializeField] float period = 0.01f;
    [SerializeField] bool limited;
    [SerializeField] bool SingleInstance = true;
    [SerializeField][HideUnless("SingleInstance", true)] bool ResetDuration_;
    [SerializeField][HideUnless("limited", true)] CharacterStats maxStatChange = new CharacterStats();
    [SerializeField][HideUnless("limited", true)] CharacterStats minStatChange = new CharacterStats();
    [SerializeField][HideUnless("limited", true)] CharacterStats maxStatChangePercent = new CharacterStats();
    [SerializeField][HideUnless("limited", true)] CharacterStats minStatChangePercent = new CharacterStats();
    [SerializeField][HideUnless("limited", true)] float minHealthChange = -1;
    [SerializeField][HideUnless("limited", true)] float maxHealthChange = float.PositiveInfinity;

    public int id => ID;
    public string UIName => Name.text;
    public Sprite icon => Icon;
    public EffectTrigger trigger => Trigger;
    public EffectTrigger endTrigger => EndTrigger;
    public float duration => Duration;
    public bool singleInstance => SingleInstance;
    public bool resetDuration => ResetDuration_;

    private Effect[] attackEffects;
    private EffectAction[] periodic;
    private IEnumerator timer;

    public GUIData guiData { get; private set; }

    [HideInInspector] public bool wasApplied = false;

    private void Awake()
    {
        var per = new List<EffectAction>();
        var effects = new List<Effect>();
        statChange.CreateEmptyAllTypes();
        statChangePercent.CreateEmptyAllTypes();
        statChangePercent.percentage = true;
        foreach (EffectAction action in actions)
        {
            if (action.periodic) per.Add(action);
            if (action.action == EffectAction.Action.applyEffectsToHitTarget) effects.AddRange(action.effects);
        }
        periodic = per.ToArray();
        attackEffects = effects.ToArray();
    }

    private void OnEnable()
    {
        if (Name != null) guiData = new GUIData(icon, UIName);
        else guiData = new GUIData(icon, "");
    }

    [ContextMenu("SetID")]
    private void SetID() => ID = Global.GenerateID();

    public void Apply(object sender, System.EventArgs args)
    {
        Character character = sender as Character;
        if (character != null)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                EffectAction action = actions[i];
                if (!action.periodic) ApplyAction(character, action);
            }
            timer = Timer(character);
            character.StartCoroutine(timer);
        }
    }
    private void ApplyAction(Character character, EffectAction action)
    {        
        bool changeHealth;
        if (limited) changeHealth = (character.health >= minHealthChange && character.health <= maxHealthChange);       
        else changeHealth = true;
        switch (action.action)
        {
            case EffectAction.Action.changeHealth: if (changeHealth) character.ChangeHealthDirectly(action.amount); break;
            case EffectAction.Action.overrideHealth: if (changeHealth) character.OverrideHealthDirectly(action.amount); break;
            case EffectAction.Action.changeStats: ChangeStats(action.statChange, character); break;
            case EffectAction.Action.damage: if (changeHealth) character.Damage(action.damage); break;
            case EffectAction.Action.heal: if (changeHealth) character.Heal(action.amount); break;
            case EffectAction.Action.move: character.Move(action.movement * Time.deltaTime * Global.instance.moveSpeedMultiplier); break;
            case EffectAction.Action.removeThisEffect: Remove(character, System.EventArgs.Empty); break;
            case EffectAction.Action.applyEffectsToOwner: for (int i = 0; i < action.effects.Length; i++) character.AddEffect(action.effects[i]); break;
            case EffectAction.Action.applyEffectsToHitTarget: character.AddAttackEffects(this, attackEffects); break;
            case EffectAction.Action.spawnCharacters: for (int i = 0; i < action.spawns.Length; i++) action.spawns[i].Spawn(); break;
            case EffectAction.Action.stun: stun = true; stunDepth = action.depth; break;
            case EffectAction.Action.breakStun: character.BreakStun(action.depth); break;
            case EffectAction.Action.removeOtherEffects: for (int i = 0; i < action.effects.Length; i++) character.RemoveEffect(action.effects[i]); break;
            case EffectAction.Action.scaleStats: 
            case EffectAction.Action.scaleDamage: 
            case EffectAction.Action.scaleResource: {

                    int index;
                    float scaleAmount;

                    if (action.scaleSource == EffectAction.ScaleSource.stat)
                    {
                        index = character.stats.FindIndex(action.sourceStat);
                        if (index >= 0) scaleAmount = character.stats[index];
                        else break;
                    }
                    else if (action.scaleSource == EffectAction.ScaleSource.health)
                    {
                        scaleAmount = character.health;
                        if (action.fromPercent) scaleAmount /= character.stats[character.maxHealthStatIndex];
                    }
                    else if (action.scaleSource == EffectAction.ScaleSource.resource)
                    {
                        index = character.GetResourceIndex(action.sourceResource);
                        if (index >= 0) scaleAmount = character.GetResource(index);
                        else break;
                        if (action.fromPercent)
                        {
                            index = character.stats.maxResources.FindIndex(action.sourceResource);
                            if (index >= 0) scaleAmount /= character.stats.maxResources[index];
                            else break;
                        }
                    }
                    else if (action.scaleSource == EffectAction.ScaleSource.damage)
                    {
                        index = character.stats.damage.FindIndex(action.sourceDamageType);
                        if (index >= 0) scaleAmount = character.stats.damage[index];
                        else break;
                    }
                    else break;

                    scaleAmount *= action.scaleMultiplier;
                    scaleAmount += action.scaleOffset;

                    if (action.action == EffectAction.Action.scaleStats || action.action == EffectAction.Action.scaleDamage)
                    {
                        CharacterStats stats;
                        bool percentage = action.multiply;
                        if (action.action == EffectAction.Action.scaleStats)
                        {
                            stats = new CharacterStats(percentage, action.targetStat);
                            stats[0] = scaleAmount;
                        }
                        else
                        {
                            if (character.stats.damage.FindIndex(action.targetType) >= 0)
                            {
                                stats = new CharacterStats(percentage, new Damage(percentage, action.targetType), null, null);
                                stats.damage[0] = scaleAmount;
                            }
                            else break;
                        }
                        ChangeStats(stats, character, true);
                    }
                    else if (action.action == EffectAction.Action.scaleResource)
                    {
                        index = character.GetResourceIndex(action.targetResource);
                        if (index >= 0) 
                        {
                            int maxIndex;
                            if (!action.allowOverflow)
                            {
                                maxIndex = character.stats.maxResources.FindIndex(action.targetResource);
                                if (maxIndex < 0) break;
                            }
                            character.SetResource(action.targetResource, action.multiply ? character.GetResource(index) * scaleAmount : scaleAmount);
                        }
                    }
                    break;
            }
            default: throw new System.NotImplementedException();
        }
    }
    public void Remove(object sender, System.EventArgs args)
    {
        Character character = sender as Character;
        if (timer != null)
        {
            character.StopCoroutine(timer);
            timer = null;
            timeLeft = -1;
        }
        character.RemoveEffect(this);
    }

    private IEnumerator Timer(Character character)
    {
        float periodLeft = float.PositiveInfinity;
        if (period > 0) periodLeft = period;
        timeLeft = float.PositiveInfinity;
        if (duration > 0) timeLeft = duration;
        while (timeLeft > 0)
        {
            if (periodLeft <= 0)
            {
                for (int i = 0; i < periodic.Length; i++) ApplyAction(character, periodic[i]);
                periodLeft = period;
            }
            yield return new WaitForEndOfFrame();
            timeLeft -= Time.deltaTime;
            periodLeft -= Time.deltaTime;
        }
        Remove(character, System.EventArgs.Empty);
    }

    private void ChangeStats(CharacterStats stats, Character character, bool reset = false)
    {     
        CharacterStats stats_;
        if (stats.percentage) stats_ = statChangePercent;
        else stats_ = statChange;
        CharacterStats oldStats;
        oldStats = new CharacterStats(); 
        oldStats.CopyFrom(stats_);
        if (reset) stats_.Override(0);
        stats_.Add(stats);
        if (limited)
        {
            if (stats.percentage) stats_.Clamp(minStatChangePercent, maxStatChangePercent);
            else stats_.Clamp(minStatChange, maxStatChange);
        }
        character.RecalculateEffectStats(this, oldStats);
    }

    public void ResetDuration()
    {
        if (duration != 0) timeLeft = duration;
    }

    public static bool operator ==(Effect one, Effect two)
    {
        if ((one as object) != null && (two as object) != null) return one.ID == two.ID;       
        else return false;
    }
    public static bool operator !=(Effect one, Effect two) => !(one == two);

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;        
        return ((obj as Effect).ID == ID);
    }
    public override int GetHashCode() => ID;
}

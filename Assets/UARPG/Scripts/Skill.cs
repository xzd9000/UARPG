using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SkillActivationType
{
    none,
    simple,
    directed,
    targeted,
    toFriendlyChar,
    toEnemyChar, 
    destinated,
    togglable,
    automatic, //togglable skill with automatic periodic activation
    fullAuto  //not manually activatable by character
}

#pragma warning disable 0649

[CreateAssetMenu] public class Skill : ScriptableObject, IGUIDataProvider
{
    [SerializeField] bool Enabled = true;
    [SerializeField] bool Active;
    [SerializeField] SkillActivationType type;
    [SerializeField] float CooldownTimeLeft;
    [SerializeField][EditorReadOnly] int ID;
    [SerializeField] LocalizedTextBlock Name;
    [SerializeField] Sprite Icon;
    [SerializeField] SkillAction[] actions = new SkillAction[0];
    [SerializeField] CharacterResources resourceRequirements;
    [SerializeField] bool applyResourceCostsOnEveryActivation = false;
    [SerializeField] float MaxUseRange;
    [SerializeField] float MaxUseAngle = -1;
    [SerializeField] float CooldownTime;
    [SerializeField] bool ActivatableDuringAttack;
    [SerializeField] float UsageThreat = 0;
    [SerializeField][HideUnless("ActivatableDuringAttack", true)] bool BreakAttackOnActivation;
    [SerializeField] bool cooldownOnActivation = true;
    [SerializeField][HideUnless("type", typeof(System.Enum), 6, 7)] float autoActivationPeriod = 5;

    public int id => ID;
    public string UIName => Name.text;
    public Sprite icon => Icon;
    public float cooldownTimeLeft => CooldownTimeLeft;
    public SkillActivationType activationType => type;
    public float cooldown => CooldownTime;
    public float maxUseRange => MaxUseRange;
    public bool enabled => Enabled;
    public bool active => Active;
    public bool breakAttackOnActivation => BreakAttackOnActivation;
    public bool activatableDuringAttack => ActivatableDuringAttack;
    public float maxUseAngle => MaxUseAngle;
    public float usageThreat => UsageThreat;

    public GUIData guiData { get; private set; }

    [ContextMenu("SetID")]
    private void SetID() => ID = Global.GenerateID();

    private void Awake() => SetID();

    private void OnEnable()
    {
        if (Name != null) guiData = new GUIData(Icon, UIName);
        else guiData = new GUIData(Icon, "");
        Enabled = true;
        ResetCooldown();
    }

    private IEnumerator IECooldown;
    private IEnumerator autoActivation;

    public void Activate(Character character) => Activate(character, null, true, Vector3.negativeInfinity, VectorParamType.none);
    public void ActivateDirected(Character character, Vector3 direction) => Activate(character, null, true, direction, VectorParamType.direction);
    public void ActivateTargeted(Character character, Vector3 target) => Activate(character, null, true, target, VectorParamType.target);
    public void ActivateTargeted(Character user, Character target) => Activate(user, target, true, Vector3.negativeInfinity, VectorParamType.none);
    public void ActivateDestinated(Character character, Vector3 destination) => Activate(character, null, true, destination, VectorParamType.destination);
    public void Deactivate(Character character) => Activate(character, null, false, Vector3.negativeInfinity, VectorParamType.none);
    public void DeactivateDirected(Character character, Vector3 direction) => Activate(character, null, false, direction, VectorParamType.direction);
    public void DeactivateTargeted(Character character, Vector3 target) => Activate(character, null, false, target, VectorParamType.target);
    public void DeactivateDestinated(Character character, Vector3 destination) => Activate(character, null, false, destination, VectorParamType.destination);
    public void Activate(Character user, Character target, bool activation, Vector3 vector, VectorParamType paramType)
    {
        if (type != SkillActivationType.none)
        {
            if (Active != activation)
            {
                if (activation && !Enabled) return;
                bool cooldown = false;
                if (type == SkillActivationType.automatic)
                {
                    if (CheckResources(user) || applyResourceCostsOnEveryActivation || !activation)
                    {
                        if (!applyResourceCostsOnEveryActivation && activation) ApplyResourceCosts(user);
                        autoActivation = IEActivate(user, target, activation, vector, paramType, autoActivationPeriod);
                        user.StartCoroutine(autoActivation);
                        cooldown = true;
                    }
                }
                else
                {
                    autoActivation = null;
                    if (CheckResources(user) || !activation)
                    {
                        if (activation) ApplyResourceCosts(user);
                        user.StartCoroutine(IEActivate(user, target, activation, vector, paramType));
                        cooldown = true;
                    }
                }
                if (cooldownOnActivation == activation && cooldown) StartCooldown(user);
            }
        }
    }

    private IEnumerator IEActivate(Character user, Character target, bool activation, Vector3 vector, VectorParamType paramType, float period = 0)
    {
        while (true)
        {
            if (applyResourceCostsOnEveryActivation && activation && autoActivation != null)
            {
                if (!CheckResources(user)) yield break;
                ApplyResourceCosts(user);
            }
            Character target_;
            for (int i = 0; i < actions.Length; i++)
            {
                if (!actions[i].passive)
                {
                    if (actions[i].toUser) target_ = user;
                    else target_ = target;

                    bool targetExists = target_ != null;

                    if (activation == actions[i].onActivation)
                    {
                        if (actions[i].hasAttack)
                        {
                            if (actions[i].activate) user.StartAttack(actions[i].attack);
                            else user.BreakAttack();
                        }

                        if (actions[i].activate)
                        {
                            if (targetExists)
                            {
                                target_.Damage(actions[i].damage);
                                target_.Heal(actions[i].healing, user);
                            }
                        }

                        Vector3 startingPoint = Vector3.negativeInfinity;
                        Vector3 targetPoint = Vector3.negativeInfinity;
                        if (paramType == VectorParamType.target) targetPoint = vector;
                        else if (paramType == VectorParamType.destination) startingPoint = vector;

                        for (int ii = 0; ii < actions[i].hitObjects.Length; ii++)
                        {
                            if (actions[i].activate) actions[i].hitObjects[ii].Activate(user, startingPoint, targetPoint);
                            else actions[i].hitObjects[ii].Deactivate();
                        }

                        for (int ii = 0; ii < actions[i].effects.Length; ii++)
                        {
                            if (targetExists)
                            {
                                if (actions[i].activate) target_.AddEffect(actions[i].effects[ii]);
                                else target_.RemoveEffect(actions[i].effects[ii]);
                            }
                        }
                    }
                }
            }
            if (activation) user.OnSkillUse(this, activation, target, vector, paramType);
            if (period <= 0 || !activation) break;
            yield return new WaitForSeconds(period);
        }
        
        
    }

    public bool CheckResources(Character user)
    {
        if (resourceRequirements.values.Length > 0)
        {
            for (int i = 0; i < resourceRequirements.values.Length; i++)
            {
                if (resourceRequirements.values[i].value > 0) if (user.GetResource(resourceRequirements.values[i].type) < resourceRequirements.values[i].value) return false;
            }
        }
        return true;
    }
    public void ApplyResourceCosts(Character user) => user.SubtractResources(resourceRequirements);
   
    public void ApplyPassiveEffects(Character owner, bool add)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i].passive)
            {
                for (int ii = 0; ii < actions[i].effects.Length; ii++)
                {
                    if (add) owner.AddEffect(actions[i].effects[ii]);
                    else owner.RemoveEffect(actions[i].effects[ii]);
                }
            }
        }
    }

    private void StartCooldown(Character character)
    {
        if (cooldown > 0)
        {
            ResetCooldown();
            IECooldown = Cooldown();
            character.StartCoroutine(IECooldown);
        }
    }
    private void ResetCooldown()
    {
        if (cooldown != 0) CooldownTimeLeft = cooldown;
    }
    private void BreakCooldown() => CooldownTimeLeft = 0;
    private IEnumerator Cooldown()
    {
        Enabled = false;
        float tick = Global.instance.defaultTimeTick;
        CooldownTimeLeft = CooldownTime;
        while (CooldownTimeLeft > 0)
        {
            yield return new WaitForEndOfFrame();
            CooldownTimeLeft -= Time.deltaTime;
        }
        Enabled = true;

    }
}

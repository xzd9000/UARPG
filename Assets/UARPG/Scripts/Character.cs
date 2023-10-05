#define showEffects

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static System.Convert;

[Flags] public enum StateFlags
{
    [InspectorName("default")] default_ = 0,
    inAir = 1,
    stunned = 1 << 1,
    onSlope = 1 << 2,
    dead = 1 << 31
}

public enum Faction
{
    none = -1,
    friendly = 0,
    enemy = 1,
}

public enum DeathColliderAction
{
    nothing,
    disable,
    changeLayer,
    destroy
}

#pragma warning disable 0649

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Inventory))]
public abstract class Character : MonoBehaviour, IKillable, IMoveable, IAttacking, ISaveable
{
    [Serializable] public class Slot
    {
        [SerializeField] LogicalSlot slot_;
        public EquippableItem item;

        public LogicalSlot slot => slot_;
    }
    public class AnimationControl
    { 
        private List<Animator> animators;
        private RuntimeAnimatorController defaultController;

        public Animator coreAnimator => animators[0];
        public RuntimeAnimatorController runtimeAnimator => animators[0].runtimeAnimatorController;

        public void AddAnimator(Animator animator) => animators.Add(animator);
        public void RemoveAnimator(Animator animator) => animators.Remove(animator);

        public AnimatorStateInfo GetStateInfo(int layer) => animators[0].GetCurrentAnimatorStateInfo(layer);
        public bool IsInTransition(int layer) => animators[0].IsInTransition(layer);

        public void SetParameter(string name, object value)
        {
            for (int i = 0; i < animators.Count; i++)
            {
                if (value.GetType() == typeof(float)) animators[i].SetFloat(name, (float)value);
                else if (value.GetType() == typeof(int)) animators[i].SetInteger(name, (int)value);
                else if (value.GetType() == typeof(bool)) animators[i].SetBool(name, (bool)value);
            }
        }
        public void SetTrigger(string name, bool set = true)
        {
            for (int i = 0; i < animators.Count; i++)
            {
                if (set) animators[i].SetTrigger(name);
                else animators[i].ResetTrigger(name);
            }
        }

        public void OverrideController(AnimatorOverrideController controller) => animators[0].runtimeAnimatorController = controller;
        public void ResetController() => animators[0].runtimeAnimatorController = defaultController;

        public AnimationControl(Animator anim)
        {
            animators = new List<Animator>();
            animators.Add(anim);
            if (anim != null) defaultController = anim.runtimeAnimatorController;
        }
    }

    [SerializeField] int ID;
    [SerializeField] LocalizedTextBlock Name;
    [SerializeField] int Level = 1;
    [SerializeField] float exp;
    [SerializeField] ExpTable expTable;
    [SerializeField] StatIncreaseTable StatTable;
    [SerializeField][Delayed] float Health = 100f;
    [SerializeField] float invincibilityOnHitTime;
    [SerializeField] float HealthBorder; //level of health lower than which cant be reached within one damage hit
    [SerializeField] float maxDamage;
    [SerializeField] CharacterResources resources;
    [SerializeField] CharacterStats baseStats = new CharacterStats
    (
            false, new Damage(false), new DamageResistance(false), new CharacterResources(false), 
            CharacterStat.maxHealth,
            CharacterStat.healing,
            CharacterStat.moveSpeed,
            CharacterStat.runSpeed,
            CharacterStat.slowSpeed,
            CharacterStat.rotationSpeed,
            CharacterStat.lifestealAmount,
            CharacterStat.lifestealPercent,
            CharacterStat.jumpHeight,
            CharacterStat.jumpsAmount
    );
    [SerializeField][EditorReadOnly] CharacterStats Stats;
    [SerializeField][EditorReadOnly] protected float CurrentMovement;
    [SerializeField] float TargetingAngle = 1f;
    [SerializeField] protected bool scaleMovementAnimationSpeed;
    [SerializeField][HideUnless("scaleMovementAnimationSpeed", true)] protected float movementAnimationScale;
    [SerializeField] List<Effect> customEffects;
    [SerializeField] List<Skill> customSkills;
    [SerializeField] bool Immortal;
    [SerializeField] bool Invincible;
    [SerializeField] bool startDead = false;
    [SerializeField] Slots Slots = new Slots();
    [SerializeField] Slot[] itemSlots = new Slot[0];
    [SerializeField] int AttackAnimLayer;
    [SerializeField] bool dropInventory;
    [SerializeField][HideUnless("dropInventory", true)] ItemContainer lootContainer;
    [SerializeField] bool questNotificationOnDeath = true;
    [SerializeField] bool questInventoryNotifications;
    [SerializeField] bool Save = false;
    [SerializeField] DeathColliderAction onDeath = DeathColliderAction.changeLayer;
    [SerializeField][HideUnless("onDeath", typeof(Enum), 2)] int deadColliderLayer;
    [SerializeField][HideUnless("onDeath", typeof(Enum), 1, 2, 3)] float deathHitboxDeactivationDelay = 2f;
    [SerializeField] float groundContactMinDistance = 0.1f;
    [SerializeField] Color color = Color.white;
    [SerializeField] AudioSource AttackSource;
    [SerializeField] AudioSource HitSource;
    [SerializeField] AudioSource FootstepSource;
    [SerializeField] AudioClip defaultStepSound;

    #if showEffects
    [SerializeField][EditorReadOnly] Effect[] visibleEffects;
    #endif

    public float xp => exp;
    public ExpTable xpTable => expTable;
    public StatIncreaseTable statTable => StatTable;
    public int level => Level;
    public int id => ID;
    public int attackAnimLayer => AttackAnimLayer;
    public virtual float health
    {
        get => Health;             
        protected set
        {
            float maxHP = Stats.values[maxHealthStatIndex].value;
            if (value > maxHP) Health = maxHP;
            else if (value <= 0)
            {
                if (Immortal) Health = 1;
                else
                {
                    Health = 0;
                    Kill();
                }
            }
            else Health = value;          
        }
    }

    public float GetResource(int index) => resources.values[index].value;
    public float GetResource(CharacterResource resource) => resources.FindValue(resource);
    public int GetResourceIndex(CharacterResource resource) => resources.FindIndex(resource);
    public bool SetResource(CharacterResource resource, float value)
    {
        int maxIndex = stats.maxResources.FindIndex(resource);
        if (maxIndex != -1) return SetResource(resources.FindIndex(resource), maxIndex, value);
        else return false;
    }
    public bool SetResource(int index, float value)
    {
        int maxIndex = stats.maxResources.FindIndex(resources.values[index].type);
        return SetResource(index, maxIndex, value);
    }
    public void AddResources(CharacterResources resources)
    {
        this.resources.Add(resources);
        ClampResources();
    }
    public void SubtractResources(CharacterResources resources)
    {
        this.resources.Subtract(resources);
        ClampResources();
    }
    private void ClampResources()
    {
        resources.Clamp(0f, false);
        resources.Clamp(stats.maxResources, true);
    }
    private bool SetResource(int index, int maxIndex, float value)
    {
        if (maxIndex != -1)
        {
            if (index != -1)
            {
                float oldValue = stats.values[index].value;
                if (value > stats.maxResources.values[maxIndex].value) value = stats.maxResources.values[maxIndex].value;
                else if (value < 0) value = 0;

                if (value != oldValue)
                {
                    resources.values[index].value = value;
                    ResourceChange?.Invoke(this, resources.values[index].type, index, oldValue, value);
                    return true;
                }
            }
        }
        return false;
    }

    public virtual CharacterStats stats
    {
        get => Stats;
        set
        {
            var oldStats = Stats;
            var newStats = value;
            if (newStats[maxHealthStatIndex] <= 0f) newStats[maxHealthStatIndex] = 1;
            newStats.damage.Clamp(0, false);
            newStats.resistance.Clamp(Global.instance.minCharacterResistance, Global.instance.maxCharacterResistance);
            Stats = newStats;
            StatChange?.Invoke(this, new OldNewEventArgs<CharacterStats, CharacterStats>(oldStats, newStats));
        }
    }
    public float healthBorder
    {
        get => HealthBorder;
        set
        {
            if (value > 0 && value <= stats[maxHealthStatIndex]) HealthBorder = value;
        }
    }
    public bool immortal { get => Immortal; set => Immortal = value; }
    public bool invincible { get => Invincible; set => Invincible = value; }
    public Slots slots => Slots;
    public string UIName => Name.text;
    public bool save => Save;
    public float movement => CurrentMovement;
    public float targetingAngle => TargetingAngle;
    public int equipmentLength => itemSlots.Length;
    public AudioSource attackSource => AttackSource;
    public AudioSource hitSource => HitSource;
    public AudioSource footstepSource => FootstepSource;

    public bool attackTargeting { get; protected set; }
    public bool attackMovement { get; protected set; }
    public CharacterController controller { get; private set; }
    public AnimationControl anim { get; protected set; }
    public Inventory inventory { get; protected set; }
    public Reusable reusable { get; protected set; }
    public Attack activeAttack { get; protected set; }
    public AttackPhase? activePhase { get; protected set; }
    public bool animated { get; private set; }
    public bool expTableExists { get; private set; }
    public bool statTableExists { get; private set; }
    public bool reusableExists { get; private set; }
    public bool attackSourceExists { get; private set; }
    public bool hitSourceExists { get; private set; }
    public bool footstepSourceExists { get; private set; }
    public GUIDataList<Skill> skills { get; private set; } = new GUIDataList<Skill>();
    public GUIDataList<Effect> effects { get; private set; } = new GUIDataList<Effect>();

    protected int physicsLayer { get; private set; }

    public bool lockMovement;
    public Faction faction = Faction.enemy;
    public StateFlags currentState;
    public bool affectedByGravity = true;

    protected HittingObject[] activeHitObjects;
    protected IEnumerator attackCoroutine;
    protected AttackVariant? ActiveAttack;

    protected GameObjectContinuousArray interactiveObjects;

    [SerializeField][EditorReadOnly] CharacterStats effectsStatChange = new CharacterStats();
    [SerializeField][EditorReadOnly] CharacterStats effectsStatChangePercent = new CharacterStats();
    [SerializeField][EditorReadOnly] CharacterStats equipmentStatChange = new CharacterStats();
    [SerializeField][EditorReadOnly] CharacterStats equipmentStatChangePercent = new CharacterStats();

    [ContextMenu("Set ID")]
    private void SetID() => ID = Global.GenerateID();

    public int maxHealthStatIndex { get; private set; }
    public int healingStatIndex { get; private set; }
    public int moveSpeedStatIndex { get; private set; }
    public int runSpeedStatIndex { get; private set; }
    public int slowSpeedStatIndex { get; private set; }
    public int rotationSpeedStatIndex { get; private set; }
    public int lifestealAmountStatIndex { get; private set; }
    public int lifestealPercentStatIndex { get; private set; }
    public int jumpHeightStatIndex { get; private set; }
    public int jumpsAmountStatIndex { get; private set; }

    private GameObject contactedObject;

    private void Awake()
    {
        maxHealthStatIndex = baseStats.FindIndex(CharacterStat.maxHealth);
        healingStatIndex = baseStats.FindIndex(CharacterStat.healing);
        moveSpeedStatIndex = baseStats.FindIndex(CharacterStat.moveSpeed);
        runSpeedStatIndex = baseStats.FindIndex(CharacterStat.runSpeed);
        slowSpeedStatIndex = baseStats.FindIndex(CharacterStat.slowSpeed);
        rotationSpeedStatIndex = baseStats.FindIndex(CharacterStat.rotationSpeed);
        lifestealAmountStatIndex = baseStats.FindIndex(CharacterStat.lifestealAmount);
        lifestealPercentStatIndex = baseStats.FindIndex(CharacterStat.lifestealPercent);
        jumpHeightStatIndex = baseStats.FindIndex(CharacterStat.jumpHeight);
        jumpsAmountStatIndex = baseStats.FindIndex(CharacterStat.jumpsAmount);
        physicsLayer = gameObject.layer;
        foreach (Skill skill in customSkills) if (skill != null) AddSkill(skill);
        foreach (Effect effect in customEffects) AddEffect(effect);
        RecalculateEquipmentStats();
        RecalculateEffectStats();
        if (lootContainer == null) dropInventory = false;
        anim = new AnimationControl(GetComponentInChildren<Animator>());
        controller = GetComponent<CharacterController>();
        reusable = GetComponent<Reusable>();
        inventory = GetComponent<Inventory>();
        inventory.InventoryChange += OnInventoryChange;
        animated = anim.coreAnimator != null;
        attackSourceExists = AttackSource != null;
        hitSourceExists = HitSource != null;
        footstepSourceExists = FootstepSource != null;
        reusableExists = reusable != null;
        statTableExists = StatTable != null;
        expTableExists = expTable != null;

        ApplyColor();

        AwakeCustom();
    }
    protected virtual void AwakeCustom() { }

    private void ApplyColor() { foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) renderer.material.SetColor("_Color", color); }

    private void OnEnable() { if (!startDead) currentState &= ~StateFlags.dead; }

    private bool stepsStoppedLastFrame;

    private void Update()
    {
        //Debug.DrawRay(new Vector3(transform.position.x + controller.center.x * transform.localScale.x,
        //                          transform.position.y + controller.center.y * transform.localScale.y - controller.height * 0.5f * transform.localScale.y + 0.3f * transform.localScale.y,
        //                          transform.position.z + controller.center.z * transform.localScale.z), -transform.up);
        //Debug.DrawRay(new Vector3(transform.position.x + controller.center.x * transform.localScale.x,
        //                          transform.position.y + controller.center.y * transform.localScale.y - controller.height * 0.5f * transform.localScale.y + 0.3f * transform.localScale.y,
        //                          transform.position.z + controller.center.z * transform.localScale.z), transform.right);
        currentState &= ~(StateFlags.inAir | StateFlags.onSlope);
        if (!Physics.Raycast(new Vector3(transform.position.x + controller.center.x * transform.localScale.x,
                                         transform.position.y + controller.center.y * transform.localScale.y - controller.height * 0.5f * transform.localScale.y + 0.3f * transform.localScale.y,
                                         transform.position.z + controller.center.z * transform.localScale.z),
                                        -transform.up, groundContactMinDistance + 0.3f, Physics.DefaultRaycastLayers & ~(1 << gameObject.layer), QueryTriggerInteraction.Ignore)) currentState |= controller.isGrounded ? StateFlags.onSlope : StateFlags.inAir;       

        if (animated)
        {   
            anim.SetParameter(Constants.Animator.boolInAir, currentState.HasFlag(StateFlags.inAir));
            SetMoveAnimationSpeed();
        }

        if (CurrentMovement > 0f && affectedByGravity && !currentState.HasFlag(StateFlags.inAir) && !currentState.HasFlag(StateFlags.dead) && footstepSourceExists && !footstepSource.isPlaying)
        {
            footstepSource.Play();
        }

        UpdateCustom();

        CurrentMovement = 0f;
        #if showEffects
        visibleEffects = new Effect[effects.objects.Count];
        for (int i = 0; i < effects.objects.Count; i++) visibleEffects[i] = effects.objects[i];
        #endif
    }   
    protected abstract void SetMoveAnimationSpeed();
    protected virtual void UpdateCustom() { }

    public event EventHandler Hit;
    public event EventHandler Healing;
    public event EventHandler HealthChange;
    public event EventHandler Death;
    public event EventHandler StatChange;
    public event EventHandler Movement;
    public event EventHandler EquipmentChange;
    public event EventHandler SkillsChange;
    public event EventHandler EffectChange;
    public event Action AttackStart;
    public event Action<Character, int, int> LevelUp;
    public event Action<Character, CharacterResource, int, float, float> ResourceChange;
    public event Action<Character, InteractiveObject> Interaction;
    public event Action<Character, Skill, bool, Character, Vector3, VectorParamType> SkillUse;

    public void OnSkillUse(Skill skill, bool activation, Character target, Vector3 vector, VectorParamType paramType) => SkillUse?.Invoke(this, skill, activation, target, vector, paramType);

    public void AddXP(float xp)
    {
        exp += xp;
        if (expTableExists) RaiseLevel(expTable.GetLevel(exp));       
    }
    
    public void RaiseLevel(int level)
    {
        if (level > Level)
        {
            int oldLevel = Level;
            Level = level;
            if (statTableExists) baseStats.Add(StatTable.GetStatIncrease(Level));
            LevelUp?.Invoke(this, oldLevel, Level);
        }
    }

    public void AddSkill(Skill skill)
    {
        Skill skill_ = Instantiate(skill);
        skills.AddObject(skill_);
        skill.ApplyPassiveEffects(this, true);
        if (skill_.activationType == SkillActivationType.fullAuto) skill_.Activate(this);
        SkillsChange?.Invoke(this, new AddRemoveEventArgs<Skill>(skill_, AddRemove.added));
    }
    public void RemoveSkill(Skill skill)
    {
        int index = skills.objects.FindIndex((target) => target.id == skill.id);
        Skill skill_ = skills.objects[index];
        if (skill_ != null)
        {
            skill.ApplyPassiveEffects(this, false);
            if (skill_.activationType == SkillActivationType.fullAuto) skill_.Deactivate(this);
            skills.RemoveObject(index);
            SkillsChange?.Invoke(this, new AddRemoveEventArgs<Skill>(skill_, AddRemove.removed));
            Destroy(skill_);
        }
    }
    public Skill[] GetSkills() => skills.objects.ToArray();

    public void AddEffect(Effect effect)
    {
        Effect effect_;
        if (effect.singleInstance)
        {
            int index = effects.objects.FindIndex((target) => target.id == effect.id);
            if (index != -1)
            {
                if (effect.resetDuration) effects.objects[index].ResetDuration();                
                return;
            }
        }
        effect_ = Instantiate(effect);
        effects.AddObject(effect_);
        if (effect_.trigger == EffectTrigger.none) effect_.Apply(this, EventArgs.Empty);
        else ApplyEffectDelegate(effect_.trigger, effect_.Apply);
        ApplyEffectDelegate(effect_.endTrigger, effect_.Remove);
        CheckStuns();
        EffectChange?.Invoke(this, new AddRemoveEventArgs<Effect>(effect_, AddRemove.added));
    }
    public void RemoveEffect(Effect effect)
    {
        int index = effects.objects.FindIndex((target) => target.id == effect.id);
        if (index >= 0)
        {
            Effect effect_ = effects.objects[index];
            if (attackEffects.ContainsKey(effect_)) attackEffects.Remove(effect_);
            effectsStatChange.Subtract(effect_.statChange);
            effects.RemoveObject(index);
            RemoveEffectDelegate(effect_, effect_.Apply);
            RecalculateEffectStats();
            if (effect_.stun) CheckStuns();
            EffectChange?.Invoke(this, new AddRemoveEventArgs<Effect>(effect_, AddRemove.removed));
            Destroy(effect_);
        }
    }
    private void ApplyEffectDelegate(EffectTrigger trigger, EventHandler delegate_)
    {
        switch (trigger)
        {
            case EffectTrigger.none: break;
            case EffectTrigger.attackHit: attackEffect += delegate_; break;
            case EffectTrigger.weaponHit: attackEffect += delegate_; break;
            case EffectTrigger.skillHit: throw new NotImplementedException();
            case EffectTrigger.damageTaken: Hit += delegate_; break;
            case EffectTrigger.characterMove: Movement += delegate_; break;
            default: throw new NotImplementedException();
        }
    }
    private void RemoveEffectDelegate(Effect effect, EventHandler delegate_)
    {
        removeDelegate(effect.trigger);
        removeDelegate(effect.endTrigger);
        void removeDelegate(EffectTrigger trigger)
        {
            switch (effect.trigger)
            {
                case EffectTrigger.none: break;
                case EffectTrigger.attackHit: attackEffect -= delegate_; break;
                case EffectTrigger.weaponHit: attackEffect -= delegate_; break;
                case EffectTrigger.skillHit: throw new NotImplementedException();
                case EffectTrigger.damageTaken: Hit -= delegate_; break;
                case EffectTrigger.characterMove: Movement -= delegate_; break;
                default: throw new NotImplementedException();
            }
        }
    }
    public Effect[] GetEffects() => effects.objects.ToArray();

    private void RecalculateStats()
    {
        CharacterStats newStats = new CharacterStats();
        newStats.CopyFrom(baseStats);
        newStats.Add(equipmentStatChange);
        newStats.Add(effectsStatChange);
        newStats.Add(equipmentStatChangePercent);
        newStats.Add(effectsStatChangePercent);
        stats = newStats;
    }

    public void RecalculateEffectStats()
    {
        effectsStatChange.CopyFrom(baseStats);
        effectsStatChange.Override(0f);
        effectsStatChangePercent.CopyFrom(baseStats);
        effectsStatChangePercent.Override(1);
        effectsStatChangePercent.percentage = true;
        CharacterStats statChange;
        for (int i = 0; i < effects.objects.Count; i++)
        {
            statChange = effects.objects[i].statChange;
            effectsStatChange.Add(statChange);
            statChange = effects.objects[i].statChangePercent;
            effectsStatChangePercent.Add(statChange);
        }
        RecalculateStats();
    }

    public void RecalculateEffectStats(Effect effect, CharacterStats oldStats)
    {
        Action<CharacterStats, Effect, CharacterStats, CharacterStats, bool> action = (stats, effect_, oldStats_, statChange, wasApplied) =>
        {
            if (wasApplied) stats.Subtract(oldStats_);
            stats.Add(statChange);
            RecalculateStats();
        };
        if (effects.objects.Contains(effect))
        {
            if (oldStats.percentage) action(effectsStatChangePercent, effect, oldStats, effect.statChangePercent, effect.wasApplied);
            else action(effectsStatChange, effect, oldStats, effect.statChange, effect.wasApplied);
            if (!effect.wasApplied) effect.wasApplied = true;
        }       
    }

    private void RecalculateEquipmentStats()
    {
        equipmentStatChange.CopyFrom(baseStats);
        equipmentStatChange.Override(0);
        equipmentStatChangePercent.CopyFrom(baseStats);
        equipmentStatChangePercent.Override(1);
        equipmentStatChangePercent.percentage = true;
        CharacterStats statChange;
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].item != null)
            {
                statChange = itemSlots[i].item.stats;
                if (statChange.percentage) equipmentStatChangePercent.Add(statChange);
                else equipmentStatChange.Add(statChange);
            }
        }
        RecalculateStats();
    }

    private void CheckStuns()
    {
        for (int i = 0; i < effects.objects.Count; i++)
        {
            if (effects.objects[i].stun == true)
            {
                BreakAttack();
                currentState |= StateFlags.stunned;
                return;
            }
        }
        currentState &= ~StateFlags.stunned;
    }

    public LogicalSlot GetEquipmentSlot(int index)
    {
        if (index >= 0 && index < itemSlots.Length) return itemSlots[index].slot;
        else return (LogicalSlot)(-1);
    }
    public EquippableItem GetEquippedItem(int index)
    {
        if (index >= 0 && index < itemSlots.Length) return itemSlots[index].item;
        else return null;
    }

    public bool EquipItem(EquippableItem item, LogicalSlot slot)
    {
        if (item.logicalSlot == slot)
        {
            int index;
            if ((index = itemSlots.FindIndex((s) => s.slot == slot && s.item == null)) >= 0) return EquipItem(item, index);
            else if ((index = itemSlots.FindIndex((s) => s.slot == slot)) >= 0) return EquipItem(item, index);
        }
        return false;
    }
    public bool EquipItem(EquippableItem item, int index)
    {
        if (index >= 0 && index < itemSlots.Length)
        {
            if (item.logicalSlot == itemSlots[index].slot)
            {
                EquippableItem eq = (EquippableItem)item.CreateInstance();
                eq.gameObject.SetActive(true);
                if (itemSlots[index].item != null) UnequipItem(index);
                itemSlots[index].item = eq;
                eq.Equip(this);
                for (int i = 0; i < eq.effects.Length; i++) AddEffect(eq.effects[i]);
                RecalculateEquipmentStats();
                EquipmentChange?.Invoke(this, new AddRemoveEventArgs<EquippableItem>(eq, AddRemove.added));
                return true;
            }
        }
        return false;
    }
    public void UnequipItem(LogicalSlot slot)
    {
        int index;
        if ((index = itemSlots.FindIndex((s) => s.slot == slot)) >= 0) UnequipItem(index);
    }
    public void UnequipItem(int index)
    {
        if (index >= 0 && index < itemSlots.Length)
        {
            EquippableItem eq = itemSlots[index].item;
            if (eq != null)
            {
                eq.gameObject.SetActive(false);
                inventory.AddInventoryItems(eq.CreateInstance() as UnityEngine.Object);
                itemSlots[index].item = null;
                for (int i = 0; i < eq.effects.Length; i++) RemoveEffect(eq.effects[i]);
                RecalculateEquipmentStats();
                EquipmentChange?.Invoke(this, new AddRemoveEventArgs<EquippableItem>(eq, AddRemove.removed));
                eq.Destroy();
            }
        }
    }

    public Damage Damage(Damage damage, Character dealer = null)
    {
        Damage hit = new Damage();
        if (!invincible)
        {
            hit = damage;
            hit.Resist(stats.resistance);
            hit.Clamp(0, false);
        }
        Hit?.Invoke(this, new ObjectEventArgs<Damage, Character>(hit, dealer));
        float newHealth = health - hit.Sum();
        if (newHealth != health)
        {
            if (maxDamage > 0 && health - newHealth > maxDamage) newHealth = health - maxDamage;
            if (health > HealthBorder && newHealth < HealthBorder) newHealth = HealthBorder;
            float oldHealth = health;
            health = newHealth;
            HealthChange?.Invoke(this, new OldNewEventArgs<float, float>(oldHealth, newHealth));
            BreakStun(1);
            if (invincibilityOnHitTime > 0) StartCoroutine(InvincibilityTimer());
        }
        return hit;
    }
    public void Heal(float amount, Character healer = null)
    {
        if (amount > 0)
        {
            float oldHealth = health;
            health += amount;
            if (health != oldHealth) 
            {               
                Healing?.Invoke(this, new ObjectEventArgs<float, Character>(health - oldHealth, healer));
                HealthChange?.Invoke(this, new OldNewEventArgs<float, float>(oldHealth, health));
            }
        }
    }
    public void ChangeHealthDirectly(float amount)
    {
        float oldHealth = health;
        health += amount;
        if (oldHealth != health) HealthChange?.Invoke(this, new OldNewEventArgs<float, float>(oldHealth, health));        
    }
    public void OverrideHealthDirectly(float newHealth)
    {
        if (newHealth != health)
        {
            float oldHealth = health;
            health = newHealth;
            HealthChange?.Invoke(this, new OldNewEventArgs<float, float>(oldHealth, health));
        }
    }

    private IEnumerator InvincibilityTimer()
    {
        invincible = true;
        yield return new WaitForSeconds(invincibilityOnHitTime);
        invincible = false;
    }

    public void AddInteractiveObject(InteractiveObject obj) => interactiveObjects.Add(obj.gameObject);
    public void RemoveInteractiveObject(InteractiveObject obj) => interactiveObjects.Remove(obj.gameObject);
    public virtual void Interact()
    {
        if (interactiveObjects.last > -1) Interact(interactiveObjects.GetLast().GetComponent<InteractiveObject>());                   
    }
    public void Interact(InteractiveObject obj)
    {
        obj.Interact(this, EventArgs.Empty);
        Interaction?.Invoke(this, obj);
    }

    private IEnumerator hitboxDeactivation;
    private IEnumerator DelayedHitboxDeactivation()
    {
        if (onDeath != DeathColliderAction.nothing)
        {
            if (deathHitboxDeactivationDelay > 0) yield return new WaitForSeconds(deathHitboxDeactivationDelay);
            if (onDeath == DeathColliderAction.disable) controller.enabled = false;
            else if (onDeath == DeathColliderAction.changeLayer) gameObject.layer = deadColliderLayer;
            else if (onDeath == DeathColliderAction.destroy)
            {
                if (reusableExists) reusable.Remove();
                else Destroy(gameObject);
            }
        }
        hitboxDeactivation = null;
    }

    public void Kill()
    {
        if (!currentState.HasFlag(StateFlags.dead))
        {
            currentState |= StateFlags.dead;
            if (dropInventory) inventory.Drop(lootContainer);
            if (questNotificationOnDeath) QuestManager.instance.NotifyDeath(this);
            if (animated) anim.SetTrigger(Constants.Animator.triggerDeath);
            DeathCustom();
            hitboxDeactivation = DelayedHitboxDeactivation();
            StartCoroutine(hitboxDeactivation);
            Death?.Invoke(this, EventArgs.Empty);
        }
    }
    protected virtual void DeathCustom() { }
    public void Resurrect(float amount = 0f, bool percentage = true)
    {
        if (currentState.HasFlag(StateFlags.dead))
        {
            if (hitboxDeactivation != null) StopCoroutine(hitboxDeactivation);
            if (animated) anim.SetTrigger(Constants.Animator.triggerResurrect);

            if (percentage)
            {
                if (amount > 0f) health = stats.values[maxHealthStatIndex].value * (amount / 100f);
                else if (amount >= 100f) health = stats.values[maxHealthStatIndex].value;
                else health = Global.instance.defaultResHPPercent;
            }
            else
            {
                if (amount > 0f) health = amount;
                else health = 1f;
            }

            ResurrectionCustom();

            gameObject.layer = physicsLayer;
            controller.enabled = true;
            currentState &= ~StateFlags.dead;
        }
    }
    protected virtual void ResurrectionCustom() { }

    /// <param name="breakingDepth">
    /// stun depths:
    /// 0 - broken by movement
    /// 1 - broken by damage
    /// 2+ - only despell
    /// </param>
    public virtual void BreakStun(int breakingDepth)
    {
        for (int i = 0; i < effects.objects.Count; i++)
        {
            if (effects.objects[i].stun) if (effects.objects[i].stunDepth <= breakingDepth) effects.objects[i].Remove(this, EventArgs.Empty);
        }
    }

    private void BaseMove(Vector3 movement, bool breakStun, bool callEvent)
    {        
        if (callEvent) Movement?.Invoke(this, new ObjectEventArgs<Vector3>(movement));
        if (breakStun) BreakStun(0);        
    }
    public void Move(Vector3 movement, bool breakStun = true, bool callEvent = true)
    {
        if (!lockMovement)
        {
            BaseMove(movement, breakStun, callEvent);
            MoveCustom(movement);
        }
    }
    public void MoveTo(Vector3 position, Vector3 movement, bool breakStun = true, bool callEvent = true)
    {
        if (!lockMovement)
        {
            BaseMove(movement, breakStun, callEvent);
            MoveToCustom(position, movement);
        }
    }
    public void Move(Vector3 movement) => Move(movement, true, true);
    public void MoveTo(Vector3 position, Vector3 movement) => MoveTo(position, movement, true, true);
    public void MoveTo(Vector3 position) => MoveTo(position, new Vector3(0f, Physics.gravity.y * Global.instance.gravityMultiplier * (affectedByGravity ? 1f : 0f), stats.values[moveSpeedStatIndex].value * Global.instance.moveSpeedMultiplier) * Time.deltaTime);
    protected abstract void MoveToCustom(Vector3 position, Vector3 movement);
    protected abstract void MoveCustom(Vector3 movement);
    public void TeleportTo(Vector3 position) => StartCoroutine(IETeleport(position));
    private IEnumerator IETeleport(Vector3 position)
    {
        lockMovement = true;
        yield return new WaitForEndOfFrame();
        transform.position = position;
        yield return new WaitForEndOfFrame();
        lockMovement = false;
    }

    public abstract bool CanReach(Vector3 position);
    public abstract float PathLengthTo(Vector3 position);

    public void Knockback(Vector3 movement, float duration)
    {
        if (knockback != null) StopCoroutine(knockback);
        knockback = IEKnockback(movement, duration);
        StartCoroutine(knockback);
    }
    private IEnumerator knockback = null;
    private IEnumerator IEKnockback(Vector3 movement, float duration)
    {
        float timeLeft = duration;
        BreakAttack();
        currentState |= StateFlags.stunned;
        if (animated) anim.SetParameter(Constants.Animator.boolKnockback, true);
        while (timeLeft > 0)
        {
            Move(movement * Time.deltaTime * Global.instance.moveSpeedMultiplier);
            yield return new WaitForEndOfFrame();
            timeLeft -= Time.deltaTime;
        }
        currentState &= ~StateFlags.stunned;
        knockback = null;
    }

    public GameObject Spawn() => Spawn(Vector3.negativeInfinity);
    public GameObject Spawn(Vector3 position)
    {
        if (reusableExists) return reusable.Spawn(null, position);
        else return Instantiate(gameObject, position, Quaternion.identity);
    }

    protected EventHandler attackEffect;
    protected Dictionary<Effect, Effect[]> attackEffects = new Dictionary<Effect, Effect[]>();
    public void AddAttackEffects(Effect source, Effect[] effects) => attackEffects.Add(source, effects);

    private int currentAnimState;

    public void StartAttack(Attack attack) => StartAttack(attack, Vector3.negativeInfinity, VectorParamType.none);
    public void StartAttackTargeted(Attack attack, Vector3 target) => StartAttack(attack, target, VectorParamType.target);
    public void StartAttackDirected(Attack attack, Vector3 direction) => StartAttack(attack, direction, VectorParamType.direction);
    public void StartAttackDestinated(Attack attack, Vector3 destination) => StartAttack(attack, destination, VectorParamType.destination);
    public void StartAttack(Attack attack, Vector3 vector, VectorParamType paramType)
    {
        if (attack.enabled)
        {
            if (!currentState.HasFlag(StateFlags.stunned) && !currentState.HasFlag(StateFlags.dead))
            {
                if (currentState.HasFlag(StateFlags.inAir) && !attack.allowInAir) return;
                if (ActiveAttack != null)
                {
                    if (activePhase != null)
                    {
                        if (!activePhase.Value.allowNextAttack) return;
                    }
                    else return;
                }
                BreakAttack();
                Attack();
            }
        }

        void Attack() { attackCoroutine = IEAttack(attack, vector, paramType); StartCoroutine(attackCoroutine); }
    }
    public void BreakAttack()
    {
        activePhase = null;
        if (anim != null)
        {
            anim.SetTrigger(Constants.Animator.triggerBreakAttack);
            anim.SetTrigger(Constants.Animator.triggerAttack, false);
        }
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        if (activeHitObjects != null) DeactivateAttack();
        if (activeAttack != null)
        {
            if (activeAttack.cooldown > 0) activeAttack.Disable(this);
            activeAttack = null;
        }
    }
    private IEnumerator IEAttack(Attack attack, Vector3 vector, VectorParamType paramType)
    {
        if (!float.IsNegativeInfinity(vector.x))
        {
            while (transform.AngleToTarget(vector, false, paramType == VectorParamType.direction) > targetingAngle)
            {
                transform.RotateToTarget(vector, stats.values[rotationSpeedStatIndex].value * Time.deltaTime, paramType == VectorParamType.direction);
                yield return new WaitForEndOfFrame();
            }
        }

        this.activeAttack = attack;
        bool isAnim = animated && attack.animated;
        bool idle = false;
        ActiveAttack = attack.activeAttack;
        AttackVariant activeAttack = ActiveAttack.Value;
        AttackStart?.Invoke();

        if (isAnim)
        {
            if (activeAttack.overrideController != null) anim.OverrideController(activeAttack.overrideController);
            anim.SetTrigger(Constants.Animator.triggerAttack);
            yield return new WaitUntil(() => anim.GetStateInfo(AttackAnimLayer).IsTag(Constants.Animator.tagPhase0));
            currentAnimState = anim.GetStateInfo(attackAnimLayer).shortNameHash;
        }

        attack.RaiseAttackChainIndex();

        if (attack.cooldownOnStart && attack.cooldown > 0) attack.Disable(this);

        for (int i = 0; i < activeAttack.phases.Length; i++)
        {
            activePhase = activeAttack.phases[i];
            AttackPhase phase = activePhase.Value;
            float holdTime = phase.holdDuration;
            if (holdTime == 0) holdTime = Global.instance.maxAttackHoldTime;

            if (phase.targeting) attackTargeting = true;
            if (phase.movement != Vector3.zero) attackMovement = true;

            CheckedAttackActivaton(phase, AttackActivation.onStart);

            if (isAnim)
            {
                if (anim.GetStateInfo(attackAnimLayer).IsTag(Constants.Animator.tagIdle))
                {
                    anim.SetTrigger(Constants.Animator.triggerAttack, false);
                    anim.SetTrigger(Constants.Animator.triggerBreakAttack, false);
                    break;
                }

                if (phase.hold) anim.SetParameter(Constants.Animator.boolHold, true);
                yield return new WaitForSeconds(anim.GetStateInfo(AttackAnimLayer).length);
            }

            CheckedAttackActivaton(phase, AttackActivation.onEndBeforeHold);
            if (phase.hold) yield return new WaitForSeconds(holdTime);
            CheckedAttackActivaton(phase, AttackActivation.onEndAfterHold);

            if (isAnim) anim.SetParameter(Constants.Animator.boolHold, false);
            if (i < activeAttack.phases.Length - 1)
            {
                yield return new WaitUntil(() => (currentAnimState != anim.GetStateInfo(attackAnimLayer).shortNameHash));
                currentAnimState = anim.GetStateInfo(attackAnimLayer).shortNameHash;
            }
        }

        if (!attack.cooldownOnStart && attack.cooldown > 0) attack.Disable(this);

        activePhase = null;
        ActiveAttack = null;
        this.activeAttack = null;

        if (isAnim && !idle) yield return new WaitUntil(() => anim.GetStateInfo(AttackAnimLayer).IsTag(Constants.Animator.tagIdle));

        attack.ResetAttackChainIndex();

        void CheckedAttackActivaton(AttackPhase phase, AttackActivation activation)
        {
            if (phase.activation == activation)
            {
                Vector3 target = Vector3.negativeInfinity;
                Vector3 startingPoint = Vector3.negativeInfinity;
                if (!float.IsNegativeInfinity(vector.x))
                {
                    if (paramType == VectorParamType.target) target = vector;
                    else if (paramType == VectorParamType.destination) startingPoint = vector;
                }
                ActivateAttack(activeAttack, target, startingPoint);
            }
            if (phase.deactivation == activation) DeactivateAttack();
        }
    }

    private void ActivateAttack(AttackVariant attack, Vector3 target, Vector3 startingPoint)
    {
        activeHitObjects = new HittingObject[attack.hitObjects.Length];
        for (int i = 0; i < activeHitObjects.Length; i++)
        {
            activeHitObjects[i] = attack.hitObjects[i].Activate(this, startingPoint, target);
            if (activeHitObjects[i] is HittingObject)
            { 
                activeHitObjects[i].CharacterHit += attackEffect;
                foreach (var KV in attackEffects) activeHitObjects[i].AddEffects(KV.Value);
            }
        }
    }
    private void DeactivateAttack()
    {
        for (int i = 0; i < activeHitObjects.Length; i++) if (activeHitObjects[i] != null) activeHitObjects[i].Deactivate();
        activeHitObjects = null;
    }

    private const string baseStatsEnd = "baseStatsEnd";
    private const string statsEnd = "statsEnd";
    private const string skillsEnd = "skillsEnd";    

    private void OnInventoryChange(Inventory inventory, IInventoryItem item, int index, bool added, bool newItem, int amountChange, int totalAmount, float moneyChange, float totalMoney)
    {
        if (questInventoryNotifications)
        {
            if (item != null) QuestManager.instance.NotifyInventory(item);         
        }
    }

    public string MakeSaveData()
    {
        //string ret =
        //Level.ToString() + "\n" +
        //Health.ToString() + "\n" +
        //baseStats.MakeSaveData() + "\n" +
        //baseStatsEnd + "\n" +
        //stats.MakeSaveData() + "\n" +
        //statsEnd + "\n";
        //for (int i = 0; i < skills.objects.Count; i++) ret += skills.objects[i].id.ToString() + "\n";
        //ret += skillsEnd +
        //ToInt32(Invincible).ToString() + ToInt32(Immortal).ToString();
        //return ret;
        throw new NotImplementedException();
    }
    public void ReadSaveData(string saveData)
    {
        //string[] lines = saveData.Split('\n');
        //Level = ToInt32(lines[0]);
        //Health = ToSingle(lines[1]);
        //string baseStats = "";
        //int i; for (i = 2; lines[i] != baseStatsEnd; i++) baseStats += lines[i];
        //this.baseStats.ReadSaveData(baseStats);
        //string stats = "";
        //for (i++; lines[i] != statsEnd; i++) stats += lines[i];
        //Stats.ReadSaveData(stats);
        //var skills = new List<Skill>();
        //foreach (string guid in FindAssets("t:Skill")) skills.Add(LoadAssetAtPath<Skill>(GUIDToAssetPath(guid)));
        //for (i++; lines[i] != skillsEnd; i++) skills.Add(skills.Find((skill) => skill.id == ToInt32(lines[i])));
        //i++;
        //Invincible = ToBoolean(ToInt32(lines[i][0]));
        //Immortal = ToBoolean(ToInt32(lines[i][1]));
        throw new NotImplementedException();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject != contactedObject)
        {
            contactedObject = hit.gameObject;
            if (contactedObject.TryGetComponent(out FootstepSource stepSource) && footstepSourceExists) footstepSource.clip = stepSource.stepSound;
            else if (footstepSourceExists) footstepSource.clip = defaultStepSound;
        }
    }

    [ContextMenu("Locate slots by name")]
    private void LocateSlotsByName()
    {
        string[] names = typeof(PhysicalSlot).GetEnumNames();
        for (int i = 1; i < name.Length; i++) Slots[i] = gameObject.FindInChildren(names[i]);       
    }

}
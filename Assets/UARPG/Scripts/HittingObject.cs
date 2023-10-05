    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

#pragma warning disable 0649

[System.Flags]
public enum HitObjectContactFlags
{
    nothing = 0,
    disableThis = 0b_00_00_01,
    disableContacted = 0b_00_00_10,
    destroyThis = 0b_00_01_00,
    destroyContacted = 0b_00_10_00,
    deflectThis = 0b_01_00_00,
    deflectContacted = 0b_10_00_00,
    disallowOverride = 0b_1_00_00_00
}

[RequireComponent(typeof(Reusable))]
public abstract class HittingObject : MonoBehaviour, IReusable, IActivatable {

    protected enum RendererAction
    {
        nothing = 0,
        enable = 1,
        disable = 2,
    }
    protected enum SoundSource
    {
        none,
        this_,
        target,
        owner,
        other
    }
    protected enum CharactersSource
    {
        attack = 0,
        hit,
        step
    }
    [System.Serializable] protected class Sound
    {
        public SoundSource source;
        public AudioClip sound;

        [HideUnless("source", typeof(System.Enum), (int)SoundSource.owner, (int)SoundSource.target)] public CharactersSource charactersSource;
        [HideUnless("source", typeof(System.Enum), (int)SoundSource.other)] public AudioSource extSource;
        [HideUnless("source", typeof(System.Enum), (int)SoundSource.target)] public bool checkGameObjectSources;
    }

    [SerializeField][EditorReadOnly] protected Character owner = null;
    [SerializeField] protected Vector3 destination = Vector3.negativeInfinity;
    [SerializeField] protected Damage baseDamage;
    [SerializeField] protected bool useStatusDamage = true;
    [SerializeField] protected float knockbackPower = 0f;
    [SerializeField] protected float knockbackDuration = 0.5f;
    [SerializeField] protected float LifestealAmount = 0;
    [SerializeField] protected float LifestealPercent = 0;
    [SerializeField][HideUnless("useStatusDamage", true)] protected Damage statusDamage;
    [SerializeField][EditorReadOnly] protected Damage damage;
    [SerializeField] protected float baseHealing;
    [SerializeField] protected bool useStatusHealing = false;
    [SerializeField][HideUnless("useStatusHealing", true)] protected float statusHealing;
    [SerializeField][EditorReadOnly] protected float healing;
    [SerializeField] protected float AttackDelay = 0.25f;
    [SerializeField] protected HitObjectContactFlags characterContact = HitObjectContactFlags.destroyThis;
    [SerializeField] protected HitObjectContactFlags hitObjectContact = HitObjectContactFlags.nothing;
    [SerializeField] protected HitObjectContactFlags obstacleContact = HitObjectContactFlags.destroyThis;
    [SerializeField] protected PhysicalSlot Slot = PhysicalSlot.none;
    [SerializeField] protected bool bindToParent;
    [SerializeField] protected bool allowDestroingObjects;
    [SerializeField] protected bool allowDisablingObjects;
    [SerializeField] protected bool allowDeflectingObjects;
    [SerializeField][HideUnless("allowDestroingObjects",true)] protected bool absorbHitObjects = false;
    [SerializeField][HideUnless("absorbHitObjects",true)] protected Damage absorbDamageIncrease;
    [SerializeField] protected Color[] colors = new Color[0];
    [SerializeField] protected Sound activationSound;
    [SerializeField] protected Sound characterHitSound;
    [SerializeField] protected Sound projectileHitSound;
    [SerializeField] protected Sound obstacleHitSound;
    [SerializeField] protected RendererAction onActivation = 0;
    [SerializeField] protected bool includeChildrenOnActivation;
    [SerializeField] protected RendererAction onDeactivation = 0;
    [SerializeField] protected List<Effect> effects = new List<Effect>();
    [SerializeField] protected bool indestructibleByHitObjects = false;
  
    public void AddStatusDamage(Damage add)
    {
        if (useStatusDamage)
        {
            statusDamage.Add(add);
            RecalculateDamage();
        }
    }
    public void SetStatusDamage(Damage dmg)
    {
        if (useStatusDamage)
        {
            statusDamage = dmg;
            RecalculateDamage();
        }
    }
    private void RecalculateDamage()
    {
        damage = baseDamage;
        if (useStatusDamage) damage.Add(statusDamage);
    }
    public Damage GetTotalDamage() => damage;
    public Damage GetBaseDamageCopy()
    {
        Damage ret = new Damage();
        ret.CopyFrom(baseDamage);
        return ret;
    }
    public float GetStatusHealing() => statusHealing;
    public Damage GetStatusDamage() => statusDamage;
    public void AddStatusHealing(float healing)
    {
        if (useStatusHealing)
        {
            statusHealing += healing;
            RecalculateHealing();
        }
    }
    public void SetStatusHealing(float healing)
    {
        if (useStatusHealing)
        {
            statusHealing = healing;
            RecalculateHealing();
        }
    }
    private void RecalculateHealing()
    {
        healing = baseHealing;
        if (useStatusHealing) healing += statusHealing;
        if (healing < 0) healing = 0;
    }
    public bool attacking { get; protected set; }
    public float attackDelay => AttackDelay; 
    public PhysicalSlot slot => Slot;
    public float lifestealAmount
    {
        get => LifestealAmount;
        set { if (value >= 0) LifestealAmount = value; else LifestealAmount = 0; }
    }
    public float lifestealPercent
    {
        get => LifestealPercent;
        set { if (value >= 0) LifestealPercent = value; else LifestealPercent = 0; }
    }

    private Reusable Reusable;
    private bool reusableExists;

    public Reusable reusable
    {
        get
        {
            if (!reusableExists)
            {
                Reusable = GetComponent<Reusable>();
                reusableExists = true;
            }
            return Reusable;
        }
    }
    protected Renderer[] renderers = new Renderer[0];
    protected Light[] lights = new Light[0];

    protected int colorPropertyID;
    protected int emissionColorPropertyID;

    protected new Collider collider;
    protected bool colliderExists;

    private AudioSource audioSource;
    private bool audioSourceExists = false;

    protected bool ownerExists;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        lights = GetComponentsInChildren<Light>();
        Reusable = GetComponent<Reusable>();
        RecalculateDamage();
        RecalculateHealing();
        if (healing < 0) healing = 0;
        colorPropertyID = Shader.PropertyToID("_Color");
        emissionColorPropertyID = Shader.PropertyToID("_EmissionColor");
        collider = GetComponent<Collider>();
        colliderExists = collider != null;
        audioSource = GetComponent<AudioSource>();
        audioSourceExists = audioSource != null;
        AwakeCustom();
    }
    protected virtual void AwakeCustom() { }

    public void AddEffects(Effect effect) => effects.Add(effect);
    public void AddEffects(IEnumerable<Effect> effects) => this.effects.AddRange(effects);

    [ContextMenu("Activate")]
    public HittingObject Activate() => Activate(null, Vector3.negativeInfinity, Vector3.negativeInfinity);
    public HittingObject Activate(Character owner) => Activate(owner, Vector3.negativeInfinity, Vector3.negativeInfinity);
    public HittingObject ActivateTargeted(Vector3 target) => Activate(null, Vector3.negativeInfinity, target);
    public HittingObject ActivateAt(Vector3 startingPoint) => Activate(null, startingPoint, Vector3.negativeInfinity);
    public abstract HittingObject Activate(Character owner, Vector3 startingPoint, Vector3 targetPoint);
    public abstract void Deactivate();

    void IActivatable.Activate() => Activate();
    void IActivatable.Activate(object obj) => Activate(obj as Character);
    void IActivatable.Activate(object obj1, object obj2) => Activate(obj1 as Character);
    void IActivatable.Activate(object obj1, object obj2, object obj3) => Activate(obj1 as Character);
    void IActivatable.Activate(object obj1, object obj2, object obj3, object obj4) => Activate(obj1 as Character);
    void IActivatable.Activate(params object[] args) => Activate(args[0] as Character);
    void IActivatable.Activate(object sender, System.EventArgs args) => Activate();

    protected void Hit(GameObject target, Vector3 point)
    {
        if (target.TryGetComponent(out IKillable killable))
        {
            Damage resisted = killable.Damage(damage, owner);
            if (LifestealAmount > 0) owner.Heal(LifestealAmount);
            if (LifestealPercent > 0) owner.Heal(resisted.Sum() * lifestealPercent);

            if (killable is Character character)
            {
                CharacterHit?.Invoke(this, new SourceRecieverEventArgs(owner, character));
                for (int i = 0; i < effects.Count; i++) character.AddEffect(effects[i]);
                if (knockbackPower != 0f) character.Knockback(transform.DirectionToTarget(target.transform.position, true) * knockbackPower, knockbackDuration);
            }

            TargetHit?.Invoke(killable, point);
            Contact(killable, characterContact);
            PlaySound(characterHitSound, killable);
        }
        else if (target.TryGetComponent(out HittingObject hitObj))
        {
            HitObjectHit?.Invoke(hitObj, point);
            Contact(hitObj, hitObjectContact);
            PlaySound(projectileHitSound, hitObj);
        }
        else
        {
            ObstacleHit?.Invoke(target, point);
            Contact(target, obstacleContact);
            PlaySound(obstacleHitSound, target);
        }       
    }

    public event System.EventHandler CharacterHit;
    public event System.Action<IKillable, Vector3> TargetHit;
    public event System.Action<HittingObject, Vector3> HitObjectHit;
    public event System.Action<GameObject, Vector3> ObstacleHit;
    public event System.Action<HittingObject, Character, Vector3, Vector3> Activated;
    public event System.Action Deactivated;

    protected void InvokeActivated(HittingObject hitObj, Character owner, Vector3 startingPoint, Vector3 targetPoint) => Activated?.Invoke(hitObj, owner, startingPoint, targetPoint);
    protected void InvokeDeactivated() => Deactivated?.Invoke();

    protected IEnumerator Delay()
    {
        attacking = true;
        yield return new WaitForSeconds(attackDelay);
        attacking = false;
    }
    
    protected void Contact(object contact, HitObjectContactFlags action)
    {
        if (action == HitObjectContactFlags.nothing || action == HitObjectContactFlags.disallowOverride) return;
        if (action.HasFlag(HitObjectContactFlags.disableContacted))
        {
            if (allowDisablingObjects) if (contact is HittingObject hitObj) hitObj.Disable();
            
        }
        if (action.HasFlag(HitObjectContactFlags.destroyContacted))
        {
            if (allowDestroingObjects)
            {
                if (contact is HittingObject hitObj)
                {
                    if (!hitObj.indestructibleByHitObjects)
                    {
                        hitObj.reusable.Remove();
                        AddStatusDamage(absorbDamageIncrease);
                    }
                }
                else if (contact is IReusable reusable) reusable.reusable.Remove();
                else if (contact is Character character) character.Kill();
                else if (contact is GameObject obj) { if (obj.CompareTag(Constants.Tags.destroyable)) Destroy(obj); }
            }
        }
        if (action.HasFlag(HitObjectContactFlags.deflectContacted))
        {
            if (allowDeflectingObjects) if (contact is AProjectile proj) proj.Deflect();           
        }
        if (action.HasFlag(HitObjectContactFlags.disableThis)) Disable();
        if (action.HasFlag(HitObjectContactFlags.deflectThis)) { if (this is AProjectile proj) proj.Deflect(); }
        if (action.HasFlag(HitObjectContactFlags.destroyThis)) reusable.Remove();
    }
    protected void PlaySound(Sound sound, object target = null)
    {
        switch (sound.source)
        {
            case SoundSource.this_: if (audioSourceExists) play(audioSource); break;
            case SoundSource.target: {

                    if (target is Character character) { if (character.hitSourceExists) playForCharacter(character); }
                    else if (target is HittingObject hitObject) { if (hitObject.audioSourceExists) play(hitObject.audioSource); }
                    else if (target is GameObject obj && sound.checkGameObjectSources) { if (obj.TryGetComponent(out AudioSource source)) play(source); }
                    break;
            }
            case SoundSource.owner:  if (ownerExists && owner.attackSourceExists) playForCharacter(owner); break;           
            case SoundSource.other: play(sound.extSource); break;                
            default:
                break;
        }

        void playForCharacter(Character character)
        {
            switch (sound.charactersSource)
            {
                case CharactersSource.attack: play(character.attackSource); break;
                case CharactersSource.hit: play(character.hitSource); break;
                case CharactersSource.step: play(character.footstepSource); break;
            }
        }
        void play(AudioSource source)
        {
            if (source.isPlaying) source.Stop();
            source.clip = sound.sound;
            source.Play();
        }

    }

    public virtual void Disable() { if (colliderExists) collider.enabled = false; }

    protected void SetVisualsEnabled(bool enabled, bool includeChildren)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (i > 0 && !includeChildren) break;
            renderers[i].enabled = enabled;
        }
        for (int i = 0; i < lights.Length; i++)
        {
            if (i > 0 && !includeChildren) break;
            lights[i].enabled = enabled;
        }
    }

    protected void SetEnabled(RendererAction action, bool includeChildren)
    {
        switch (action)
        {
            case RendererAction.enable:
                gameObject.SetActive(true);
                SetVisualsEnabled(true, includeChildren);                
                break;
            case RendererAction.disable:
                gameObject.SetActive(false);
                break;
        }
    }

    protected void OnDestroy() => CharacterHit = null;
    
}

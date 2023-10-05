using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Animations;

#pragma warning disable 0649

public abstract class EquippableItem : MonoBehaviour, IInventoryItem
{
    [SerializeField][EditorReadOnly] int ID;
    [SerializeField] LocalizedTextBlock Name;
    [SerializeField] string ResourcePath;
    [SerializeField] Sprite Icon;
    [SerializeField] Color RarityColor = Color.white;
    [SerializeField] LocalizedTextBlock Description;
    [SerializeField][HideUnless("UseExtraText", true)] LocalizedTextBlock ExtraText;
    [SerializeField] CharacterStats StatMod;
    [SerializeField] Effect[] Effects;
    [SerializeField] PhysicalSlot PhysicalSlot;
    [SerializeField] LogicalSlot LogicalSlot;
    [SerializeField] bool bindToSlotObject;
    [SerializeField] bool visualsDisabled;
    [SerializeField] ParticleSystem[] particles;
    [SerializeField] AnimatorOverrideController OverrideController;

    public int id => ID;
    public string UIName => Name.text;
    public Sprite icon => Icon;
    public string resourcePath => ResourcePath;
    public CharacterStats stats => StatMod;
    public PhysicalSlot physicalSlot => PhysicalSlot;
    public LogicalSlot logicalSlot => LogicalSlot;
    public Color rarityColor => RarityColor;
    public Effect[] effects => Effects;
    public string description => Description;
    public string extraText => ExtraText;

    public bool animated { get; private set; }
    public bool controllerAvailable { get; private set; }
    public Animator anim { get; private set; }
    public AnimatorOverrideController overrideController => OverrideController;

    private ParentConstraint constraint;
    private Renderer[] renderers;
    private Light[] lights;

    [ContextMenu("SetID")]
    private void SetID() => ID = Global.GenerateID();

    private void Awake()
    {
        if (bindToSlotObject)
        {
            constraint = GetComponent<ParentConstraint>();
            if (constraint == null) constraint = gameObject.AddComponent<ParentConstraint>();
        }
        anim = GetComponent<Animator>();
        animated = anim != null;
        controllerAvailable = OverrideController != null;

        renderers = GetComponentsInChildren<Renderer>();
        lights = GetComponentsInChildren<Light>();

        if (visualsDisabled) SetVisualsEnabled(false);
    }

    public void SetVisualsEnabled(bool enabled)
    {
        for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = enabled;
        for (int i = 0; i < lights.Length; i++) lights[i].enabled = enabled;
        for (int i = 0; i < particles.Length; i++) { if (enabled) particles[i].Play(); else particles[i].Stop(); }
    }

    public void Equip(Character character)
    {
        if (physicalSlot != PhysicalSlot.none)
        {
            if (bindToSlotObject)
            {
                ConstraintSource source = new ConstraintSource();
                source.sourceTransform = character.slots[physicalSlot].transform;
                source.weight = 1f;
                constraint.AddSource(source);
                constraint.constraintActive = true;
            }
            EquipCustom(character);
        }
    }
    protected virtual void EquipCustom(Character character) { }

    public bool Equals(IInventoryItem item) => item.id == ID;

    public IInventoryItem CreateInstance()
    {
        IInventoryItem item = Instantiate(this);
        ((EquippableItem)item).gameObject.name = gameObject.name;
        return item;
    }

    public IInventoryItem Load() => Resources.Load<EquippableItem>(resourcePath);
    public abstract void Destroy();

    protected void DestroyGameObject() { if (gameObject.scene.IsValid()) Destroy(gameObject); }
}

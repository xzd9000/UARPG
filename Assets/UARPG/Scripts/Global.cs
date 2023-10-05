using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0649

public class Global : Singleton<Global>
{  
    [Header("Reusables")]
    [SerializeField] Vector3 InactiveObjectPosition = Vector3.zero;
    [SerializeField] Vector3 InactiveObjectAngles = Vector3.zero;
    [Header("Delta time compensation multipliers")]
    [SerializeField][Min(float.Epsilon)] float RotationSpeedMultiplier = 6f;
    [SerializeField][Min(float.Epsilon)] float MoveSpeedMultiplier = 10f;
    [SerializeField][Min(float.Epsilon)] float ProjectileMoveSpeedMultiplier = 1f;
    [SerializeField][Min(float.Epsilon)] float GravityMultiplier = 10f;
    [SerializeField][Min(float.Epsilon)] float AccelerationMultiplier = 1f;
    [SerializeField][Min(float.Epsilon)] float Acceleration2Multiplier = 1f;
    [Header("Fixed time multipliers")]
    [SerializeField][Min(float.Epsilon)] float RotationSpeedFixedMultiplier = 1f / 6f;
    [SerializeField][Min(float.Epsilon)] float MoveSpeedFixedMultiplier = 1f / 10f;
    [SerializeField][Min(float.Epsilon)] float GravityFixedMultiplier = 1f / 10f;
    [Header("Localization")]
    [SerializeField] StatLocalization StatLocalization;
    [SerializeField] ResistanceLocalization ResistanceLocalization;
    [SerializeField] DamageTypeLocalization DamageTypeLocalization;
    [Header("Other")]
    [SerializeField] string savesFolderLocalPath = "UARPG";
    [SerializeField][Min(0)] float MaxAttackHoldTime = 60f;
    [SerializeField][Min(float.Epsilon)] float LaserUpdateRate = 0.001f;
    [SerializeField] DamageResistance MinCharactersResistance;
    [SerializeField] DamageResistance MaxCharactersResistance;
    [SerializeField][Min(1)] int MaxInteractiveObjectsForPlayer = 100;
    [SerializeField][Min(1)] int MaxInteractiveObjectsForNPC = 10;
    [SerializeField][Min(1f)] float DefaultResHPPercent = 10f;
    [SerializeField][Min(float.Epsilon)] float MinDamageValue = 1f;
    [SerializeField][Min(float.Epsilon)] float DefaultTimeTick = 0.001f;
    [SerializeField][Min(float.Epsilon)] float DefaultAggroRange = 10f;
    [SerializeField][Min(float.Epsilon)] float MinSpawnDelay = 0.0001f;
    [SerializeField][Min(1f)] float LaterJumpsMultiplier = 1.09f;
    [SerializeField] Color DefaultUIColor = new Color(147f, 147f, 147f);
    [SerializeField] Color DefaultUITransparentColor = new Color(147f, 147f, 147f, 152f);

    public float projectileMoveSpeedMultiplier => ProjectileMoveSpeedMultiplier;
    public float accelerationMultiplier => AccelerationMultiplier;
    public float acceleration2Multiplier => Acceleration2Multiplier;
    public string savesPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\" + instance.savesFolderLocalPath;
    public float maxAttackHoldTime => MaxAttackHoldTime;
    public DamageResistance minCharacterResistance => MinCharactersResistance;
    public DamageResistance maxCharacterResistance => MaxCharactersResistance;
    public Vector3 inactiveObjectPosition => InactiveObjectPosition;
    public Vector3 inactiveObjectAngles => InactiveObjectAngles;
    public float laserUpdateRate => LaserUpdateRate;
    public int maxInteractiveObjectsForPlayer => MaxInteractiveObjectsForPlayer;
    public int maxInteractiveObjectsForNPC => MaxInteractiveObjectsForNPC;
    public float defaultTimeTick => DefaultTimeTick;
    public float rotationSpeedMultiplier => RotationSpeedMultiplier;
    public float moveSpeedMultiplier => MoveSpeedMultiplier;
    public float defaultAggroRange => DefaultAggroRange;
    public Color defaultUIColor => DefaultUIColor;
    public Color defultUITransparentColor => DefaultUITransparentColor;
    public float minDamageValue => MinDamageValue;
    public float defaultResHPPercent => DefaultResHPPercent;
    public float gravityMultiplier => GravityMultiplier;
    public float rotationSpeedFixedMultiplier => RotationSpeedFixedMultiplier;
    public float moveSpeedFixedMultiplier => MoveSpeedFixedMultiplier;
    public float gravityFixedMultiplier => GravityFixedMultiplier;
    public float minSpawnDelay => MinSpawnDelay;
    public float laterJumpsMultiplier => LaterJumpsMultiplier;
    public StatLocalization statLocalization => StatLocalization;
    public DamageTypeLocalization damageLocalization => DamageTypeLocalization;
    public ResistanceLocalization resistanceLocalization => ResistanceLocalization;

    public static Player player { get; private set; }
    public static Control control { get; private set; }

    protected override void AwakeCustom()
    {
        initLocalization<StatLocalization, CharacterStat>(ref StatLocalization);
        initLocalization<DamageTypeLocalization, DamageType>(ref DamageTypeLocalization);
        initLocalization<ResistanceLocalization, DamageType>(ref ResistanceLocalization);

        player = FindObjectOfType<Player>();
        control = FindObjectOfType<Control>();
        Debug.Log(player);
        Debug.Log(control);

        void initLocalization<T1, T2>(ref T1 localization) where T2 : Enum where T1 : CustomTypeLocalization<T2>
        {
            T1[] found;
            if (localization != null) localization.Initialize();
            else if ((found = Resources.FindObjectsOfTypeAll<T1>()).Length > 0) localization = found[0];
            else localization = ScriptableObject.CreateInstance<T1>();
        }
    }

    public static int GenerateID()
    {
        DateTime now = DateTime.Now;
        return
            now.Second + 
            now.Minute * 60 + 
            now.Hour * 3600 + 
            now.DayOfYear * 24 * 3600 + 
            (now.Year % 10) * 365 * 24 * 3600;
    }

    public static int FlagValue(int flags, int position) => (flags >> (position - 1)) % 2;
    public static bool FlagValueBool(int flags, int position) => FlagValue(flags, position) == 1;

}

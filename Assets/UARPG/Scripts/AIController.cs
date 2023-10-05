using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Character))]
[RequireComponent(typeof(CombatAI))]
[RequireComponent(typeof(IdleAI))]
public class AIController : MonoBehaviour
{

    [SerializeField] Character Target;
    [SerializeField] protected bool Combat;
    [SerializeField] bool useAggroSphere = true;
    [SerializeField][HideUnless("useAggroSphere", true)] float autoAggroSphereRadius;
    [SerializeField][HideUnless("useAggroSphere", true)] int autoAggroSphereLayer;
    [SerializeField][HideUnless("useAggroSphere", true)] protected InfoBox aggroSphere;
    [SerializeField] bool chainAggro = true;
    [SerializeField] List<AITarget> targets;
    [SerializeField] protected float DestinationReachDistance = 0.1f;
    [SerializeField] protected float TargetingAngleLimit = 0.1f;

    public bool targetExists { get; private set; }

    public IdleAI idleAI { get; protected set; }
    public CombatAI combatAI { get; protected set; }
    public Character character { get; protected set; }
    public CharacterController controller { get; protected set; }

    public float destinationReachDistance => DestinationReachDistance;
    public float targetingAngleLimit => TargetingAngleLimit;

    public bool combat
    {
        get => Combat;
        set
        {
            if (value == false && Combat == true)
            {
                CombatExit?.Invoke(this, System.EventArgs.Empty);
                combatAI.enabled = false;
                idleAI.enabled = true;
                targets = null;
            }
            else if (value == true && Combat == false)
            {
                CombatEnter?.Invoke(this, System.EventArgs.Empty);
                combatAI.enabled = true;
                idleAI.enabled = false;
            }

            Combat = value;
        }
    }
    public Character target => Target;

    protected virtual void Awake()
    {
        idleAI = GetComponent<IdleAI>();
        combatAI = GetComponent<CombatAI>();
        character = GetComponent<Character>();
        controller = GetComponent<CharacterController>();
        character.Death += OnDeath;
        if (useAggroSphere)
        {
            if (aggroSphere == null)
            {
                InfoBox[] infos = GetComponentsInChildren<InfoBox>();
                for (int i = 0; i < infos.Length; i++)
                {
                    if (infos[i].tag == "Aggro")
                    {
                        aggroSphere = infos[i];
                        break;
                    }
                }
                if (aggroSphere == null) aggroSphere = CreateAggroSphere().GetComponent<InfoBox>();
            }
        }
    }

    private void Update()
    {
        if (combat) combatAI.BaseAction();
        else idleAI.BaseAction();
    }

    protected void OnEnable()
    {
        if (useAggroSphere) aggroSphere.InfoEnter += InfoEnter;
        character.Hit += OnDamaged;
        idleAI.enabled = true;
        combatAI.enabled = true;
    }
    protected void OnDisable()
    {
        if (useAggroSphere) aggroSphere.InfoEnter -= InfoEnter;
        character.Hit -= OnDamaged;
        idleAI.enabled = false;
        combatAI.enabled = false;
    }

    protected void InfoEnter(object sender, System.EventArgs args)
    {                 
        if (args is ObjectEventArgs<GameObject, int> oArgs)
        {
            Character entered = oArgs.obj1.GetComponent<Character>();
            if (entered != this)
            {
                if ((int)entered.faction != (int)character.faction)
                {
                    if (chainAggro) CallAllies(entered);
                    AddTarget(entered);
                }
                else if (chainAggro) if (oArgs.obj1.TryGetComponent(out AIController controller)) if (controller.targetExists) AddTarget(controller.target);
            }
        }
    }

    public void CallAllies(Character target)
    {
        for (int i = 0; i < aggroSphere.ObjectsLength; i++)
        {
            if (aggroSphere[i] != null)
            {
                if ((int)aggroSphere[i].GetComponent<Character>().faction == (int)character.faction)
                {
                    if (aggroSphere[i].TryGetComponent(out AIController controller)) if (controller != this) controller.AddTarget(target);
                }
            }
        }
    }

    protected void OnDeath(object sender, System.EventArgs args) => SetTarget(null);

    protected void OnDamaged(object sender, System.EventArgs args)
    {
        if (args is ObjectEventArgs<Damage, Character> args_ && args_.obj2 != null && targets != null)
        {
            int index = -1;
            if ((index = targets.FindIndex((target) => target.target == args_.obj2)) != -1)
            {
                targets[index].AddThreat(args_.obj1.threat);
                CheckTargetAt(index, true);
            }
            else
            {
                AddTarget(args_.obj2, args_.obj1.threat);
                if (chainAggro) CallAllies(args_.obj2);
            }
        }
    }
    protected void OnTargetDead(object sender, System.EventArgs args)
    {
        if (sender is Character character)
        {
            int index;
            if ((index = targets.FindIndex((target) => target.target == character)) != -1)
            {
                targets.RemoveAt(index);
                CheckTargets();
                character.Death -= OnTargetDead;
            }
        }
    }
    protected void OnTargetSkillUse(Character user, Skill skill, bool activation, Character target, Vector3 vector, VectorParamType paramType)
    {
        int index;
        if ((index = targets.FindIndex((target_) => target_.target == user)) != -1)
        {
            targets[index].AddThreat(skill.usageThreat);
            CheckTargetAt(index, true);
        }
    }

    protected GameObject CreateAggroSphere()
    {
        GameObject obj = new GameObject("AggroSphere");
        obj.transform.SetParent(transform);
        obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
        SphereCollider collider = obj.AddComponent<SphereCollider>();
        Rigidbody body = obj.AddComponent<Rigidbody>();
        collider.radius = autoAggroSphereRadius;
        collider.isTrigger = true;
        body.useGravity = false;
        body.isKinematic = true; 
        obj.AddComponent<InfoBox>();
        obj.layer = autoAggroSphereLayer;
        return obj;
    }

    public event System.Action<AIController, Character, bool> TargetChange;
    public event System.EventHandler CombatEnter;
    public event System.EventHandler CombatExit;

    public void SetTarget(Character target, bool startCombat = true)
    {
        Target = target;
        targetExists = (target != null);
        if (startCombat && targetExists) combat = true;
        TargetChange?.Invoke(this, target, targetExists);
    }

    public void AddTarget(Character target, float threat = 0)
    {
        if (targets == null) targets = new List<AITarget>();
        if (targets.FindIndex((target_) => target_.target == target) == -1)
        {
            targets.Add(new AITarget(target, threat));
            if (targetExists) CheckTargetAt(targets.Count - 1, true);
            else
            {
                if (targets.Count > 1) CheckTargets();
                else SetTarget(target);
            }
            target.Death += OnTargetDead;
            target.SkillUse += OnTargetSkillUse;
        }
    }
    public void CheckTargetAt(int i, bool setTarget = false)
    {
        if (targets != null)
        {
            if (targets.Count > 0)
            {
                if (i < targets.Count)
                {
                    if (targets[i].threat > targets[0].threat)
                    {
                        AITarget target = targets[i];
                        targets.RemoveAt(i);
                        targets.Insert(0, target);
                        if (setTarget) if (this.target != targets[0].target) SetTarget(targets[0].target);
                    }
                }
            }
        }
    }
    public void CheckTargets()
    {
        if (targets == null) targets = new List<AITarget>();
        if (targets.Count > 0)
        {
            for (int i = 1; i < targets.Count; i++) CheckTargetAt(i);
            if (target != targets[0].target) SetTarget(targets[0].target);
        }
        else if (combat == true)
        {
            SetTarget(null, false);
            combat = false;
        }
    }

}   
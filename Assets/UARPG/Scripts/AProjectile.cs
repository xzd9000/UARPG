using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public abstract class AProjectile : HittingObject {

    [Header("Projectile Properties")]
    [SerializeField] protected List<ProjectileTransformation> transformations = new List<ProjectileTransformation>();
    [SerializeField] protected bool cycleTransformations;
    [SerializeField] protected float targetingAngle = 1f;
    [SerializeField] protected float targetingStopDistance = 3f;
    [SerializeField] protected float targetingRotationSpeed = 10f;
    [SerializeField] protected bool XAxisTargeting;
    [SerializeField] protected bool ZAxisTargeting;
    [Header("")]
    [SerializeField] protected bool autoDestroy = true;
    [SerializeField][HideUnless("autoDestroy", true)] protected float autoDestroyTime = 10;

    protected bool active;
    private ProjectileTransformation transformation;
    
    public override HittingObject Activate(Character owner, Vector3 startingPoint, Vector3 targetPoint)
    {
        this.owner = owner;
        ownerExists = owner is Character;
        GameObject obj;
        AProjectile proj;
        if (attacking == false)
        {
            if (attackDelay != 0f && gameObject.activeInHierarchy) StartCoroutine(Delay());
            Transform parent = null;
            if (slot != PhysicalSlot.none && ownerExists && bindToParent) parent = owner.slots[slot].transform;
            obj = reusable.Spawn(parent, startingPoint);
            proj = obj.GetComponent<AProjectile>();
            proj.owner = owner;
            proj.ownerExists = ownerExists;
            if (autoDestroy == true) proj.StartCoroutine(proj.AutoDestroy());        
            obj.transform.SetPositionAndRotation(float.IsNegativeInfinity(startingPoint.x) ? GetStartingTransform(owner).position : startingPoint, GetStartingTransform(owner).rotation);
            obj.transform.localScale = transform.lossyScale;           
            if (proj.colliderExists) proj.collider.enabled = true;
            proj.StartCoroutine(proj.Moving());
            proj.SetEnabled(proj.onActivation, proj.includeChildrenOnActivation);
            if (!float.IsNegativeInfinity(destination.x)) proj.destination = destination;
            else destination = targetPoint;
            if (colors.Length > 0)
            {
                Color color = colors[Random.Range(0, colors.Length)];
                for (int i = 0; i < proj.renderers.Length; i++)
                {
                    proj.renderers[i].material.SetColor(proj.colorPropertyID, color);
                    proj.renderers[i].material.SetColor(proj.emissionColorPropertyID, color);
                }
                for (int i = 0; i < lights.Length; i++) proj.lights[i].color = color;
            }
            proj.PlaySound(activationSound);
            proj.SetStatusHealing(statusHealing);
            proj.SetStatusDamage(statusDamage);
            if (ownerExists)
            {
                proj.AddStatusDamage(owner.stats.damage);
                proj.statusHealing += owner.stats.FindValue(CharacterStat.healing);
                proj.hitObjectContact |= owner.stats.hitObjectFlags;
                proj.lifestealPercent = owner.stats[owner.lifestealPercentStatIndex];
                proj.lifestealAmount = owner.stats[owner.lifestealAmountStatIndex];
            }
            proj.InvokeActivated(proj, owner, startingPoint, targetPoint);
        }
        else proj = null;
        return proj;
    }
    public override void Deactivate() { }

    public virtual void Deflect() => transform.rotation = Quaternion.LookRotation(-transform.forward, transform.up);

    protected IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(autoDestroyTime);
        reusable.Remove();
    }

    protected IEnumerator Moving()
    {
        if (transformations.Count > 0)
        {
            for (int i = 0; i < transformations.Count; i++)
            {
                transformation = transformations[i];
                if (transformation.duration != 0)
                    StartCoroutine(MoveTimer(transformation.duration));
                else active = true;

                ApplyOffset(transformation);
                Vector3 movement = transformation.movement;
                Vector3 scale = transformation.scale;
                Vector3 rotation = transformation.rotation;
                Vector3 movementAcceleration = transformation.movementAcceleration;
                Vector3 scaleAcceleration = transformation.scaleAcceleration;
                Vector3 rotationAcceleration = transformation.rotationAcceleration;
                while (active == true)
                {
                    if (!float.IsNegativeInfinity(destination.x))
                    {
                        if (transform.AngleToTarget(destination, XAxisTargeting) > targetingAngle) transform.RotateToTarget(destination, targetingRotationSpeed * Time.deltaTime, false, !XAxisTargeting, !ZAxisTargeting);
                    }

                    movement += movementAcceleration * Time.deltaTime * Global.instance.accelerationMultiplier;
                    scale += scaleAcceleration * Time.deltaTime * Global.instance.accelerationMultiplier;
                    rotation += rotationAcceleration * Time.deltaTime * Global.instance.accelerationMultiplier;
                    
                    if (!transformation.allowNegativeMovement) movement = movement.CutLow(0f);
                    if (!transformation.allowNegativeScale) scale = scale.CutLow(0f);
                    if (!transformation.allowNegativeRotation) rotation = rotation.CutLow(0f);
                    
                    transform.Rotate((rotation + RandomVector(transformation.rotationRandomRange)) * Time.deltaTime * Global.instance.projectileMoveSpeedMultiplier, transformation.space);
                    transform.Translate((movement + RandomVector(transformation.movementRandomRange)) * Time.deltaTime * Global.instance.projectileMoveSpeedMultiplier, transformation.space);
                    transform.localScale += (scale + RandomVector(transformation.scaleRandomRange)) * Time.deltaTime * Global.instance.projectileMoveSpeedMultiplier;

                    movementAcceleration += transformation.movementAcc2 * Time.deltaTime * Global.instance.acceleration2Multiplier;
                    rotationAcceleration += transformation.rotationAcc2 * Time.deltaTime * Global.instance.acceleration2Multiplier;
                    scaleAcceleration += transformation.scaleAcc2 * Time.deltaTime * Global.instance.acceleration2Multiplier;

                    yield return new WaitForEndOfFrame();
                }
            }
            if (cycleTransformations == true) StartCoroutine(Moving());
        }
        else yield return null;
    }
    protected IEnumerator MoveTimer(float time)
    {
        active = true;
        yield return new WaitForSeconds(time);
        active = false;
    }

    public void AddTransformation(ProjectileTransformation transformation)
    {
        ApplyOffset(transformation);
        this.transformation = this.transformation.AddTransformAndAcceleration(transformation);
    }

    public void ApplyOffset(ProjectileTransformation transformation)
    {
        transform.Translate(transformation.positionOffset + RandomVector(transformation.positionOffsetRandomRange), transformation.space);
        transform.Rotate(transformation.rotationOffset + RandomVector(transformation.rotationOffsetRandomRange), transformation.space);
        transform.localScale += transformation.scaleOffset + RandomVector(transformation.scaleOffsetRandomRange);
    }

    protected Vector3 RandomVector(Vector3 absRange)
    {
        return new Vector3(Random.Range(-absRange.x, absRange.x),
                           Random.Range(-absRange.y, absRange.y),
                           Random.Range(-absRange.z, absRange.z));
    }

    protected Transform GetStartingTransform(Character owner)
    {
        if (owner == null || slot == PhysicalSlot.none) return transform;
        else return owner.slots[slot].transform;
    }

    protected void OnCollisionEnter(Collision collision) => Hit(collision.gameObject, collision.GetContact(0).point);
    protected void OnTriggerEnter(Collider other) => Hit(other.gameObject, other.ClosestPoint(transform.position));
}

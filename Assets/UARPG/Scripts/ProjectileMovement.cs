using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ProjectileTransformation 
{
    [Header("Transformation")]
    public Vector3 movement; 
    public Vector3 movementRandomRange; 
    public Vector3 rotation; 
    public Vector3 rotationRandomRange; 
    public Vector3 scale; 
    public Vector3 scaleRandomRange;
    public bool allowNegativeMovement;
    public bool allowNegativeScale;
    public bool allowNegativeRotation;
    [Header("Offset")]
    public Vector3 positionOffset;
    public Vector3 positionOffsetRandomRange;
    public Vector3 rotationOffset;
    public Vector3 rotationOffsetRandomRange;
    public Vector3 scaleOffset;
    public Vector3 scaleOffsetRandomRange;
    [Header("Acceleration")]
    public Vector3 movementAcceleration;
    public Vector3 rotationAcceleration;
    public Vector3 scaleAcceleration;
    public bool allowNegativeMovementAcceleration;
    public bool allowNegativeScaleAcceleration;
    public bool allowNegativeRotationAcceleration;
    [Header("Acceleration acceleration")]
    public Vector3 movementAcc2;
    public Vector3 rotationAcc2;
    public Vector3 scaleAcc2;
    public Space space;
    public float duration;

    public static ProjectileTransformation operator +(ProjectileTransformation first, ProjectileTransformation second)
    {
        ProjectileTransformation ret = first;

        ret.movement = first.movement + second.movement;
        ret.rotation = first.rotation + second.rotation;
        ret.scale = first.scale + second.scale;
        ret.positionOffset = first.positionOffset + second.positionOffset;
        ret.rotationOffset = first.rotationOffset + second.rotationOffset;
        ret.scaleOffset = first.scaleOffset + second.scaleOffset;
        ret.movementAcceleration = first.movementAcceleration + second.movementAcceleration;
        ret.rotationAcceleration = first.rotationAcceleration + second.rotationAcceleration;
        ret.scaleAcceleration = first.scaleAcceleration + second.scaleAcceleration;
        ret.movementAcc2 = first.movementAcc2 = second.movementAcc2;
        ret.scaleAcc2 = first.scaleAcc2 = second.scaleAcc2;
        ret.rotationAcc2 = first.rotationAcc2 = second.rotationAcc2;
        return ret;
    }
    public static ProjectileTransformation operator *(ProjectileTransformation first, int second)
    {
        ProjectileTransformation ret = first;

        ret.movement = first.movement * second;
        ret.rotation = first.rotation * second;
        ret.scale = first.scale * second;
        ret.positionOffset = first.positionOffset * second;
        ret.rotationOffset = first.rotationOffset * second;
        ret.scaleOffset = first.scaleOffset * second;
        ret.movementAcceleration = first.movementAcceleration * second;
        ret.rotationAcceleration = first.rotationAcceleration * second;
        ret.scaleAcceleration = first.scaleAcceleration * second;
        return ret;
    }

    public ProjectileTransformation AddTransformAndAcceleration(ProjectileTransformation transformation)
    {
        ProjectileTransformation ret = this;
        AddTransformRef(ref ret, ref transformation);
        AddAccelerationRef(ref ret, ref transformation);
        return ret;
    }
    public ProjectileTransformation AddTransform(ProjectileTransformation transformation)
    {
        ProjectileTransformation ret = this;
        AddTransformRef(ref ret, ref transformation);
        return ret;
    }
    public ProjectileTransformation AddAcceleration(ProjectileTransformation transformation)
    {
        ProjectileTransformation ret = this;
        AddAccelerationRef(ref ret, ref transformation);
        return ret;
    }

    private void AddTransformRef(ref ProjectileTransformation ret, ref ProjectileTransformation transformation)
    {
        ret.movement += transformation.movement;
        ret.movementRandomRange += transformation.movementRandomRange;
        ret.rotation += transformation.rotation;
        ret.rotationRandomRange += transformation.rotationRandomRange;
        ret.scale += transformation.scale;
        ret.scaleRandomRange += transformation.scaleRandomRange;
    }
    private void AddAccelerationRef(ref ProjectileTransformation ret, ref ProjectileTransformation transformation)
    {
        ret.movementAcceleration += transformation.movementAcceleration;
        ret.rotationAcceleration += transformation.rotationAcceleration;
        ret.scaleAcceleration += transformation.scaleAcceleration;
    }

    public override string ToString()
    {
        return
           "m:" + movement.ToString() + " +- " + movementRandomRange + "\n" +
           "r:" + rotation.ToString() + " +- " + rotationRandomRange + "\n" +
           "s:" + scale.ToString() + " +- " + scaleRandomRange + "\n" +

           "mo:" + positionOffset.ToString() + " +- " + positionOffsetRandomRange + "\n" +
           "ro:" + rotationOffset.ToString() + " +- " + rotationOffsetRandomRange + "\n" +
           "so:" + scaleOffset.ToString() + " +- " + scaleOffsetRandomRange + "\n" +

           "ma:" + movementAcceleration.ToString() + "\n" +
           "ra:" + rotationAcceleration.ToString() + "\n" +
           "sa:" + scaleAcceleration.ToString();
    }
}

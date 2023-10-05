using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HittingObject))]
public class HittingObjectEventTrigger : EventTrigger
{
    [System.Flags]
    private enum Trigger
    {
        activation = 1,
        deactivation = 1 << 1,
        targetHit = 1 << 2,
        hitObjectHit = 1 << 3,
        obstacleHit = 1 << 4,
        anyHit = targetHit | hitObjectHit | obstacleHit,
        destroyed = 1 << 5
    }

    [SerializeField] Trigger trigger;

    private HittingObject hitObject;

    private void Awake() => hitObject = GetComponent<HittingObject>();

    private void OnEnable()
    {
        if (trigger.HasFlag(Trigger.activation)) hitObject.Activated += EventType1;
        if (trigger.HasFlag(Trigger.deactivation)) hitObject.Deactivated += EventType0;
        if (trigger.HasFlag(Trigger.targetHit)) hitObject.TargetHit += EventType2;
        if (trigger.HasFlag(Trigger.hitObjectHit)) hitObject.HitObjectHit += EventType3;
        if (trigger.HasFlag(Trigger.obstacleHit)) hitObject.ObstacleHit += EventType4;
        if (trigger.HasFlag(Trigger.destroyed)) hitObject.reusable.Removed += EventType5;
    }
    private void OnDisable()
    {
        if (trigger.HasFlag(Trigger.activation)) hitObject.Activated -= EventType1;
        if (trigger.HasFlag(Trigger.deactivation)) hitObject.Deactivated -= EventType0;
        if (trigger.HasFlag(Trigger.targetHit)) hitObject.TargetHit -= EventType2;
        if (trigger.HasFlag(Trigger.hitObjectHit)) hitObject.HitObjectHit -= EventType3;
        if (trigger.HasFlag(Trigger.obstacleHit)) hitObject.ObstacleHit -= EventType4;
        if (trigger.HasFlag(Trigger.destroyed)) hitObject.reusable.Removed -= EventType5;
    }

    private void EventType0() => ActivateTrigger(hitObject);
    private void EventType1(HittingObject h, Character c, Vector3 v1, Vector3 v2) => ActivateTrigger(h, c, v1, v2);
    private void EventType2(IKillable k, Vector3 v) => ActivateTrigger(hitObject, k, v);
    private void EventType3(HittingObject k, Vector3 v) => ActivateTrigger(hitObject, k, v);
    private void EventType4(GameObject k, Vector3 v) => ActivateTrigger(hitObject, k, v);
    private void EventType5(GameObject o) => ActivateTrigger(hitObject);
}

using System;
using UnityEngine;

public class EventEventTrigger : EventTrigger
{
    [Flags]
    private enum Triggers
    {
        eventActivated = 1,
        triggerActivated = 1 << 1
    }

    [SerializeField] Triggers trigger;
    [SerializeField] Event watchedEvent;

    private void OnEnable()
    {
        if (trigger.HasFlag(Triggers.eventActivated)) watchedEvent.Activated += Activate;
        if (trigger.HasFlag(Triggers.triggerActivated)) watchedEvent.TriggerActivated += Activate;
    }
    private void OnDisable()
    {
        if (trigger.HasFlag(Triggers.eventActivated)) watchedEvent.Activated -= Activate;
        if (trigger.HasFlag(Triggers.triggerActivated)) watchedEvent.TriggerActivated -= Activate;
    }

    private void Activate() => ActivateTrigger(); 
    private void Activate(EventTrigger t, object[] d) => ActivateTrigger(t, d);
}
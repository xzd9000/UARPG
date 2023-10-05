using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class Event : MonoBehaviour , IActivatable
{
    private object[][] data;

    [Serializable] private class TriggerActivationData
    {
        public EventTrigger trigger;
        public bool activated;
    }

    [SerializeField] TriggerActivationData[] triggers;
    [SerializeField] bool anyTrigger;
    [SerializeField] bool resetTriggers = true;
    [SerializeField] List<EventAction> actions;
    
    private void Awake() => data = new object [triggers.Length > 0 ? triggers.Length : 1][];    

    public void ActivateTrigger(EventTrigger trigger, params object[] data)
    {
        if (triggers.Length > 0)
        {
            int index = triggers.FindIndex((t) => trigger == t.trigger);
            if (index >= 0)
            {
                triggers[index].activated = true;
                this.data[index] = data;
                CheckTriggers();
                TriggerActivated?.Invoke(trigger, data);
            }
        }
        else if (anyTrigger)
        {
            this.data[0] = data;
            TriggerActivated?.Invoke(trigger, data);
            Activate();
        }
    }
    
    private void CheckTriggers()
    {
        for (int i = 0; i < triggers.Length; i++)
        {
            if (triggers[i].activated == false) { if (!anyTrigger) return; }
            else if (anyTrigger) break;
        }
        Activate();
    }

    public void Activate(params object[] args) => Activate();
    public void Activate(object obj1, object obj2) => Activate(obj1, obj2);
    public void Activate(object obj1, object obj2, object obj3) => Activate(obj1, obj2, obj3);
    public void Activate(object obj1, object obj2, object obj3, object obj4) => Activate(obj1, obj2, obj3, obj4);
    public void Activate(object obj) => Activate(obj);
    public void Activate(object source, EventArgs args) => Activate(source);
    public void Activate()
    {
        if (resetTriggers) for (int i = 0; i < triggers.Length; i++) triggers[i].activated = false;
        foreach (EventAction action in actions)
        {
            StartCoroutine(action.Activate(data));
        }
        Activated?.Invoke();
    }

    public event Action Activated;
    public event Action<EventTrigger, object[]> TriggerActivated;
}

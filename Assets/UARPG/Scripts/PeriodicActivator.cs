using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeriodicActivator : MonoBehaviour
{
    [SerializeField] MonoBehaviour[] activatables;
    [SerializeField] bool randomPeriod;
    [SerializeField][HideUnless("randomPeriod", false)] float period;
    [SerializeField][HideUnless("randomPeriod", true)] Vector2 periodSpread;

    private float lastActivationTime;

    private void Awake() => lastActivationTime = Time.time;

    private void Update()
    {
        if (Time.time - lastActivationTime >= period)
        {
            for (int i = 0; i < activatables.Length; i++) if (activatables[i] is IActivatable iactivatable) iactivatable.Activate();
            lastActivationTime = Time.time;

            if (randomPeriod) period = Random.Range(periodSpread.x, periodSpread.y);
        }        
    }
}

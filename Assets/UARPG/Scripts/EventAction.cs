using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

#pragma warning disable 0649

[System.Serializable] public class EventAction
{

    enum Action
    {
        activateObject = 0,
        [InspectorName("[D(0, 1)]Increase Character Parameters")] increaseCharacterParameters = 1,
        overrideCharacterParameters = 2,
        changeAIEnabledStatus = 3,
        [InspectorName("[D(0)] Set AI Target")] setAITarget = 4,
        showGUI = 5,
        overrideColliderState = 6,
        overrideGameobjectState = 7,
        [InspectorName("[D(0, 1, 2)] Change Animator Parameters")] changeAnimatorParameters = 8,
        [InspectorName("[D(0)] Move Characters")] moveCharacters = 9,
        setBehaviorEnabled = 10,
        resetTrailRenderer = 11,
        [InspectorName("[D(0)] Spawn Object")] spawnObject = 12,
        [InspectorName("[D(0, 1, 2) Activate hitObject")] activateHitobject = 13,
        logString = 14,
        [InspectorName("[D(0, 1)] Damage")] damage = 15,
        destroy = 16,
        loadScene = 17,
        changeUIFocus = 18,
        quit = 99
    }

    [SerializeField] Action action;
    [SerializeField] float activationDelay;
    [SerializeField] bool randomizedDelay;
    [SerializeField] bool delayForEachObject;

    [SerializeField] Object[] objects;
    [SerializeField] bool useEventData;
    [SerializeField] Vector2Int[] eventDataUsage;

    // if override value is negative no override will be done
    [SerializeField][HideUnless("action", typeof(System.Enum), 1, 2)] float healthChange;
    [SerializeField][HideUnless("action", typeof(System.Enum), 1, 2)] CharacterStats statsChange;
    [SerializeField][HideUnless("action", typeof(System.Enum), 4)] AITarget target;
    [SerializeField][HideUnless("action", typeof(System.Enum), 2)] Faction factionOverride;
    [SerializeField][HideUnless("action", typeof(System.Enum), 2)] BoolOverride immortalityOverride = BoolOverride.noChange;
    [SerializeField][HideUnless("action", typeof(System.Enum), 2)] BoolOverride invincibilityOverride = BoolOverride.noChange;
    [SerializeField][HideUnless("action", typeof(System.Enum), 3, 5, 6, 7, 10)] BoolOverride boolOverride = BoolOverride.noChange;
    [SerializeField][HideUnless("action", typeof(System.Enum), 8)] string boolParam;
    [SerializeField][HideUnless("action", typeof(System.Enum), 8)] bool boolValue;
    [SerializeField][HideUnless("action", typeof(System.Enum), 8)] string intParam;
    [SerializeField][HideUnless("action", typeof(System.Enum), 8)] int intValue;
    [SerializeField][HideUnless("action", typeof(System.Enum), 8)] string floatParam;
    [SerializeField][HideUnless("action", typeof(System.Enum), 8)] float floatValue;
    [SerializeField][HideUnless("action", typeof(System.Enum), 8)] string triggerParam;
    [SerializeField][HideUnless("action", typeof(System.Enum), 9, 12, 13)] Vector3 position;
    [SerializeField][HideUnless("action", typeof(System.Enum), 13)] Character projOwner;
    [SerializeField][HideUnless("action", typeof(System.Enum), 13)] Vector3 projTargetPoint;
    [SerializeField][HideUnless("action", typeof(System.Enum), 14, 17)] string string_;
    [SerializeField][HideUnless("action", typeof(System.Enum), 15)] Damage damage;
    [SerializeField][HideUnless("action", typeof(System.Enum), 18)] UIFocus focus;
    

    private Vector2Int[] e => eventDataUsage;

    public IEnumerator Activate(object[][] eventData)
    {
        if (activationDelay > 0 && !delayForEachObject) yield return new WaitForSeconds(randomizedDelay ? Random.Range(0f, activationDelay) : activationDelay);
        for (int i = 0; i < objects.Length; i++)
        {
            if (activationDelay > 0 && delayForEachObject) yield return new WaitForSeconds(randomizedDelay ? Random.Range(0f, activationDelay) : activationDelay);
            switch (action)
            {
                case Action.activateObject: {

                        IActivatable act = objects[i] as IActivatable;
                        if (act != null) act.Activate();
                        else Debug.LogError("Referenced object is not IActivatable");
                        break;
                }
                case Action.increaseCharacterParameters: {        
                        
                        if (objects[i] is Character character)
                        {
                            character.ChangeHealthDirectly(useEventData ? (float)eventData[e[0].x][e[0].y] : healthChange);
                            character.stats.Add(useEventData ? (CharacterStats)eventData[e[1].x][e[1].y] : statsChange);
                        }                        
                        break;
                }
                case Action.overrideCharacterParameters: {

                        if (objects[i] is Character character)
                        {
                            if (healthChange >= 0) character.OverrideHealthDirectly(healthChange);
                            character.stats.Override(statsChange);
                            character.immortal = character.immortal.Override(immortalityOverride);
                            character.invincible = character.invincible.Override(invincibilityOverride);
                            if (factionOverride != Faction.none) character.faction = factionOverride;
                        }
                        break;
                }
                case Action.changeAIEnabledStatus: {

                        if (objects[i] is AIController controller)  controller.enabled.Override(boolOverride);
                        break;
                }
                case Action.setAITarget: {

                        if (objects[i] is AIController controller) controller.AddTarget(useEventData ? (Character)eventData[e[0].x][e[0].y] : target.target, useEventData ? 0f : target.threat);
                        break;
                }
                case Action.showGUI: {

                        if (objects[i] is GUIObject gui)
                        {
                            switch (boolOverride)
                            {
                                case BoolOverride.noChange: break;
                                case BoolOverride.toTrue: gui.Show(); break;
                                case BoolOverride.toFalse: gui.Hide(); break;
                                default: break;
                            }
                        }
                        break;
                }
                case Action.overrideColliderState: {
                        if (objects[i] is Collider collider) collider.enabled = collider.enabled.Override(boolOverride);
                        break;
                }
                case Action.overrideGameobjectState: {
                        bool active;
                        if (objects[i] is GameObject gameObject)
                        {
                            active = gameObject.activeInHierarchy;
                            gameObject.SetActive(active.Override(boolOverride));
                        }

                        break;
                }
                case Action.changeAnimatorParameters: {
                        if (objects[i] is Animator anim)
                        {
                            if (floatParam.Length > 0) anim.SetFloat(floatParam, useEventData ? (float)eventData[e[0].x][e[0].y] : floatValue);
                            if (boolParam.Length > 0) anim.SetBool(boolParam, useEventData ? (bool)eventData[e[1].x][e[1].y] : boolValue);
                            if (intParam.Length > 0) anim.SetInteger(intParam, useEventData ? (int)eventData[e[2].x][e[2].y] : intValue);
                            if (triggerParam.Length > 0) anim.SetTrigger(triggerParam);
                        }
                        break;
                }
                case Action.moveCharacters: {
                        if (objects[i] is Character character) character.TeleportTo(useEventData ? (Vector3)eventData[e[0].x][e[0].y] : position);
                        break;
                }
                case Action.setBehaviorEnabled: {
                             if (objects[i] is Behaviour b) b.enabled = b.enabled.Override(boolOverride);
                        else if (objects[i] is Collider c) c.enabled = c.enabled.Override(boolOverride);
                        else if (objects[i] is Renderer r) r.enabled = r.enabled.Override(boolOverride);
                        break;
                }
                case Action.resetTrailRenderer: {

                        if (objects[i] is TrailRenderer trail) trail.Clear();
                        break;
                }
                case Action.spawnObject: {

                        if (objects[i] is Reusable reusable) reusable.Spawn(null, useEventData ? (Vector3)eventData[e[0].x][e[0].y] : position);                        
                        break;
                }
                case Action.activateHitobject: {

                        if (objects[i] is HittingObject hitObj) hitObj.Activate
                        (
                            useEventData ? (e[0].y >= 0 ? (Character)eventData[e[0].x][e[0].y] : null) : projOwner,
                            useEventData ? (e[1].y >= 0 ? (Vector3)eventData[e[1].x][e[1].y] : Vector3.negativeInfinity) : position,
                            useEventData ? (e[2].y >= 0 ? (Vector3)eventData[e[2].x][e[2].y] : Vector3.negativeInfinity) : projTargetPoint
                        );
                        break;
                }
                case Action.damage: {

                        if (objects[i] is IKillable killable)
                        {
                            killable.Damage(useEventData ? (Damage)eventData[e[0].x][e[0].y] : damage, 
                                            useEventData ? (e[1].y >= 0 ? (Character)eventData[e[1].x][e[1].y] : null) : null);
                        }
                        break;
                }
                case Action.destroy: Object.Destroy(objects[i]); break;
                case Action.changeUIFocus: {

                        if (objects[i] is Control control) control.ChangeUIFocus(focus);
                        break;
                }
            }
        }
        switch (action)
        {
            case Action.loadScene: UnityEngine.SceneManagement.SceneManager.LoadScene(string_); break;
            case Action.logString: Debug.Log(string_); break;
            case Action.quit:
                {
                    Application.Quit();
                    yield break;
                }
        }
    }

}

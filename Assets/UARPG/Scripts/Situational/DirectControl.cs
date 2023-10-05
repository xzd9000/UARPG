using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(CharacterMovement))]
public class DirectControl : Control
{
    public bool sendKeyboardInput = true;
    public bool sendMouseInput = false;

    [SerializeField] protected Weapon activeWeapon;
    [SerializeField] protected List<Weapon> weapons = new List<Weapon>();

    public bool alwaysRun;

    protected Character character;
    protected CharacterMovement charMoveComponent;
    protected int jumpIndex = 1;
    protected MouseLook[] mouse;
    protected int activeWeaponIndex { get; private set; } = -1;
    protected bool inAnimation { get; private set; }

    private void Awake()
    {
        character = GetComponent<Character>();
        charMoveComponent = GetComponent<CharacterMovement>();
        mouse = FindObjectsOfType<MouseLook>();
        MouseLook mouseX = mouse.Find((m) => m.axis == Axis.X);
        MouseLook mouseY = mouse.Find((m) => m.axis == Axis.Y);
        mouse[0] = mouseX;
        mouse[1] = mouseY;
        FocusChange += ChangeMouseState;
        AwakeCustom();
    }
    protected virtual void AwakeCustom() { }

    public void ChangeMouseState(UIFocus focus)
    {
        bool lock_ = focus == UIFocus.game ? true : false;
        for (int i = 0; i < mouse.Length; i++) mouse[i].SetCursorLock(lock_);
    }

    protected void CheckAttackInput(Attack attack, string input)
    {
        switch (attack.attackType)
        {
            case AttackType.singleShot: if (Input.GetButtonDown(input) && character.activeAttack == null) character.StartAttack(attack); break;
            case AttackType.autoShot: if (Input.GetButton(input) && character.activeAttack == null) character.StartAttack(attack); break;
            case AttackType.togglable:
                {
                    if (Input.GetButtonDown(input))
                    {
                        if (character.activeAttack == null) character.StartAttack(attack);
                        else if (character.activeAttack == attack) character.BreakAttack();
                    }
                    break;
                }
            case AttackType.held:
                {
                    if (Input.GetButtonDown(input)) { if (character.activeAttack == null) character.StartAttack(attack); }
                    else if (Input.GetButtonUp(input)) { if (character.activeAttack == attack) character.BreakAttack(); }
                    break;
                }
            default:
                break;
        }
    }

    protected void SetActiveWeapon(int i, bool changeAnimator = false) => StartCoroutine(IESetActiveWeapon(i, changeAnimator));
    protected IEnumerator IESetActiveWeapon(int i, bool changeAnimator = false)
    {
        if (i < weapons.Count)
        {
            if (activeWeaponIndex != i)
            {
                character.BreakAttack();
                if (i >= 0)
                {
                    if (weapons[i] != null)
                    {
                        if (activeWeaponIndex >= 0)
                        {
                            if (weapons[activeWeaponIndex] != null)
                            {
                                if (character.animated)
                                {
                                    inAnimation = true;
                                    character.anim.SetTrigger(Constants.Animator.triggerUnequip);
                                    yield return new WaitUntil(() => character.anim.GetStateInfo(character.attackAnimLayer).IsTag(Constants.Animator.tagEpuip));
                                    yield return new WaitForSeconds(character.anim.GetStateInfo(character.attackAnimLayer).length);
                                    inAnimation = false;
                                }
                                activeWeapon.SetVisualsEnabled(false);
                            }
                        }
                        activeWeapon = weapons[i];
                        if (character.animated)
                        {
                            inAnimation = true;
                            character.anim.SetTrigger(Constants.Animator.triggerEquip);
                            yield return new WaitUntil(() => character.anim.GetStateInfo(character.attackAnimLayer).IsTag(Constants.Animator.tagEpuip));
                            if (changeAnimator && weapons[i].controllerAvailable) character.anim.OverrideController(weapons[i].overrideController);
                        }
                        activeWeapon.SetVisualsEnabled(true);
                        if (character.animated) yield return new WaitUntil(() => character.anim.GetStateInfo(character.attackAnimLayer).IsTag(Constants.Animator.tagIdle));
                        inAnimation = false;
                        activeWeaponIndex = i;
                    }
                }
                else
                {
                    activeWeapon = null;
                    activeWeaponIndex = -1;
                    if (changeAnimator) character.anim.ResetController();
                }
                ActiveWeaponChanged?.Invoke();
            }
        }
    }

    public event System.Action ActiveWeaponChanged;

    private float moveSpeed;
    private bool run;

    protected override void UpdateCustom()
    {
        if (focus == UIFocus.game)
        {
            run = Input.GetButton(Constants.Input.runButton);
            if (alwaysRun) run = !run;

            if (run) moveSpeed = character.stats[character.runSpeedStatIndex];
            else moveSpeed = character.stats[character.moveSpeedStatIndex];

            if (sendKeyboardInput) 
            {
                charMoveComponent.movementInput.x = Input.GetAxis(Constants.Input.horizontalAxis);
                charMoveComponent.movementInput.z = Input.GetAxis(Constants.Input.verticalAxis);
                charMoveComponent.jumpInput = Input.GetButtonDown(Constants.Input.jumpButton);
                charMoveComponent.moveSpeedInput = moveSpeed;
            }

            if (sendMouseInput) 
            {
                charMoveComponent.mouseXInput = Input.GetAxis(Constants.Input.mouseXAxis);
                charMoveComponent.mouseYInput = Input.GetAxis(Constants.Input.mouseYAxis);
                charMoveComponent.mouseWheelInput = Input.GetAxis(Constants.Input.mouseWheelAxis);
            }
        }
        else
        {
            charMoveComponent.movementInput.x = 0f;
            charMoveComponent.movementInput.z = 0f;
            charMoveComponent.jumpInput = false;
            charMoveComponent.moveSpeedInput = 0f;
        }
    }
}

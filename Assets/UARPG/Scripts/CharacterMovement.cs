using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField][EditorReadOnly] protected Vector3 movement;
    [SerializeField] protected float Mass;
    [SerializeField] protected float fallSpeedLimit;
    [SerializeField] protected float groundedGravity;
    [SerializeField] protected bool infiniteJumps;
    [SerializeField] protected bool checkSlope;
    [SerializeField][HideUnless("checkSlope", true)] protected float slipping = 0.1f;

    private float tVertSpeed;
    private int jumpIndex = 1;
    private ControllerColliderHit controllerColliderHit;
    private Vector3 groundDir;
    protected Character character;
    protected bool jumpAllowed = true;
    protected bool jump;

    [EditorReadOnly] public Vector3 movementInput;
    [EditorReadOnly] public float moveSpeedInput;
    [EditorReadOnly] public bool jumpInput;
    [EditorReadOnly] public float mouseXInput;
    [EditorReadOnly] public float mouseYInput;
    [EditorReadOnly] public float mouseWheelInput;
                     public float massModifierInput = 1f;

    public float mass
    {
        get => Mass;
        set => Mass = value > 0 ? value : 0;
    }

    private void Awake()
    {
        character = GetComponent<Character>();
        AwakeCustom();
    }
    protected virtual void AwakeCustom() { }

    protected virtual void Update()
    {
        tVertSpeed = movement.y;
        movement.y = 0f;
        movement.x = 0f;
        movement.z = 0f;
        jump = false;

        if (!character.currentState.HasFlag(StateFlags.dead))
        {
            movement.x = movementInput.x;
            movement.z = movementInput.z;
            movement *= moveSpeedInput;
            movement = Vector3.ClampMagnitude(movement, moveSpeedInput);

            jump = jumpInput && jumpAllowed;
        }
        
        movement.y = tVertSpeed;

        if (character.currentState.HasFlag(StateFlags.inAir) && !character.controller.isGrounded)
        {
            if (movement.y > fallSpeedLimit && character.affectedByGravity) movement.y += Physics.gravity.y * Mass * massModifierInput * Time.deltaTime * Global.instance.gravityMultiplier;

            if (jump)
            {
                if (jumpIndex < character.stats[character.jumpsAmountStatIndex] || infiniteJumps)
                {
                    movement.y += character.stats[character.jumpHeightStatIndex] * Global.instance.laterJumpsMultiplier;
                    movement.y = Mathf.Clamp(movement.y, 0f, character.stats[character.jumpHeightStatIndex] * Global.instance.laterJumpsMultiplier);
                    jumpIndex++;
                }
            }
        }
        else
        {
            if (character.affectedByGravity && movement.y < -groundedGravity) movement.y = groundedGravity;
            jumpIndex = 1;

            if (checkSlope && character.affectedByGravity && character.currentState.HasFlag(StateFlags.onSlope))
            {                
                groundDir = transform.InverseTransformDirection(Vector3.Normalize(controllerColliderHit.point - new Vector3
                (
                    transform.position.x + character.controller.center.x * transform.localScale.x,
                    transform.position.y + character.controller.center.y * transform.localScale.y - character.controller.height * transform.localScale.y,
                    transform.position.z + character.controller.center.z * transform.localScale.z
                )));

                movement -= groundDir * mass * slipping * Mathf.Acos(Vector3.Dot(groundDir, Vector3.down)) * Mathf.Rad2Deg;
            }
            
            if (jump) movement.y = character.stats[character.jumpHeightStatIndex];
        }

        movement.y += movementInput.y * Time.deltaTime;

        character.Move(movement * Time.deltaTime * Global.instance.moveSpeedMultiplier);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) => controllerColliderHit = hit;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public abstract class ItemContainer : InteractiveObject { }

[RequireComponent(typeof(Inventory))]
public class InstantPickupItemContainer : ItemContainer
{
    [SerializeField] bool destroyOnInteraction;
    [SerializeField][HideUnless("destroyOnInteraction", true)] float destructionDelay;
    
    private Animator anim;
    private bool animExists;

    public Inventory inventory { get; private set; }

    protected override void AwakeCustom()
    {
        inventory = GetComponent<Inventory>();
        anim = GetComponent<Animator>();
        animExists = anim != null;
    }

    public override void Interact(object sender, System.EventArgs args)
    {
        if (sender is Character character)
        {
            inventory.MoveInventoryItems(character.inventory);
            base.Interact(sender, args);
            collider.enabled = false;
            if (animExists) anim.SetTrigger(Constants.Animator.triggerInteraction);
            if (destroyOnInteraction)
            {
                if (destructionDelay > 0 || animExists) StartCoroutine(Destroy());
                else Destroy(gameObject);
            }
        }
    }

    private IEnumerator Destroy()
    {
        if (destructionDelay > 0) yield return new WaitForSeconds(destructionDelay);
        else if (animExists)
        {
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsTag(Constants.Animator.tagInteraction));
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        }
        Destroy(gameObject);
    }
}

using UnityEngine;

public class KillableFixedAttacks : MonoBehaviour, IKillable
{
    [SerializeField] int HitsLeft;
    [SerializeField] int MaxHits;

    public float health
    {
        get => HitsLeft;
        set
        {
            HitsLeft = (int)value;
            if (HitsLeft > MaxHits) HitsLeft = MaxHits;
            else if (HitsLeft <= 0)
            {
                if (invincible) HitsLeft = 1;
                else Kill();
            }
        }
    }

    public bool immortal { get; set; }
    public bool invincible { get; set; }

    public Damage Damage(Damage damage, Character dealer)
    {
        health--;
        return new Damage();
    }

    public void Heal(float amount, Character healer) => health++;

    public void Kill() => Destroy(gameObject);
}

using System.Collections;
using UnityEngine;

public class Killable : MonoBehaviour, IKillable
{
    [SerializeField] float Health;
    [SerializeField] float maxHealth;

    public float health
    {
        get => Health;
        set
        {
            if (value > maxHealth) Health = maxHealth;
            else if (value <= 0) Kill();
            else Health = value;
        }
    }

    public bool immortal { get; set; }
    public bool invincible { get; set; }

    public Damage Damage(Damage damage, Character dealer)
    {
        Damage hit;
        if (!invincible)
        {
            hit = damage;
            float newHealth = health - hit.Sum();
            if (newHealth <= health)
            {
                if (newHealth < 0)
                {
                    if (immortal) health = 1;
                    else health = 0;
                }
                else health = newHealth;
            }
            else hit = new Damage();
        }
        else hit = new Damage();
        return hit;
    }

    public void Heal(float amount, Character healer) => health += amount;

    public event System.Action<IKillable> Died;

    public void Kill()
    {
        Died?.Invoke(this);
        Destroy(gameObject);
    }
}

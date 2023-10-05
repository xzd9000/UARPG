public interface IKillable
{
    float health { get; }
    bool immortal { get; set; }
    bool invincible { get; set; }

    /// <returns>returns the damage that was dealt</returns>
    Damage Damage(Damage damage, Character dealer);
    void Heal(float amount, Character healer);

    void Kill();
}

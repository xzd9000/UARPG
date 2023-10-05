[System.Serializable] public class AITarget
{
    public Character target;
    public float threat;

    public void AddThreat(float threat) => this.threat += threat;
    

    public AITarget(Character target, float threat)
    {
        this.target = target;
        this.threat = threat;
    }
}
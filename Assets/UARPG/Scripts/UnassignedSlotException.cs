[System.Serializable] public class UnassignedSlotException : System.Exception
{
    public UnassignedSlotException() { }
    public UnassignedSlotException(string message) : base(message) { }
    public UnassignedSlotException(string message, System.Exception inner) : base(message, inner) { }
    protected UnassignedSlotException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
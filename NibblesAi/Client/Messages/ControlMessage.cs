namespace Client.Messages
{
    public class ControlMessage : BaseMessage
    {
        public ControlMessage(Direction direction)
        {
            msg = "control";
            data = new { direction = (int)direction };
        }
    }
}

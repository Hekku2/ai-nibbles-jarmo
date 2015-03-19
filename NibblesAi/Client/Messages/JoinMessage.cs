namespace Client.Messages
{
    public class JoinMessage : BaseMessage
    {
        public JoinMessage(string player)
        {
            msg = "join";
            data = new { player = new { name = player } };
        }
    }
}
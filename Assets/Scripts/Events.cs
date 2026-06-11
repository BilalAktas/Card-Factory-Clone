namespace Core
{
    public struct OnCardLeftCardAreaEvent : IEvent { public Card Card; }
    public struct OnCardMoveOnContainerEndEvent : IEvent { public Card Card; }
    public struct OnGameFailedEvent : IEvent { }
    public struct OnMatchedEvent : IEvent { }
    
    public class Events
    {
    }
}
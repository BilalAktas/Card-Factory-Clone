namespace Core
{
    public struct OnCardLeftCardAreaEvent : IEvent { public Card Card; }
    public struct OnCardLeftSlotAreaEvent : IEvent { public Card Card; }
    public struct OnCardArrivedFactoryEvent : IEvent { public Card Card; }
    public struct OnCardMoveOnContainerEndEvent : IEvent { public Card Card; }
    public struct OnGameFailedEvent : IEvent { }
    public struct OnMatchedEvent : IEvent
    {
        public Card Card;
        public ProductBox Box;
    }
    
    public struct OnCardGroupLeftCardAreaEvent : IEvent { public int Amount; }
    
    public class Events
    {
    }
}
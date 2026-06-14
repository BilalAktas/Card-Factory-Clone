using System.Collections.Generic;

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
    
    public struct OnCardGroupLeftCardAreaEvent : IEvent { public List<Card> Cards; }
    
    public struct OnProductBoxFilledEvent : IEvent
    {
        public int ProductBoxAreaIndex;
    }
    public struct OnLevelCompletedEvent : IEvent { }
    
    public struct OnLevelSpawnedEvent : IEvent { }
    public struct OnNextLevelButtonClickedEvent : IEvent { }
    
    public struct OnGameResetEvent : IEvent { }
    
    public class Events
    {
    }
}
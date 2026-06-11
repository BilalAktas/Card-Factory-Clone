using UnityEngine;

namespace Core
{
    public class Slot
    {
        public Card Card { get; private set; }
        public Transform Visual {get; private set;}
        
        public Slot(Card card, Transform visual)
        {
            Card = card;
            Visual = visual;
        }

        public void SetCard(Card card, int state)
        {
            Card = card;
            if (Card == null) return;
            Card.MoveToSlot(this, state);
        }
    }
}
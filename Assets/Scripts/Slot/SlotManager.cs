using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class SlotManager : MonoBehaviour
    {
        private List<Slot> _slots = new();
        [SerializeField] private GameObject _slotPrefab;
        
        private void Start()
        {
            for (var i = 0; i < 20; i++)
            {
                var clone = Instantiate(_slotPrefab, transform);
                clone.transform.localPosition = new Vector3(i, 0, 0);
                var slot = new Slot(null, clone.transform);
                _slots.Add(slot);
            }
            
            EventBus.Subscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEnd);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEnd);
        }

        private void OnCardMoveOnContainerEnd(OnCardMoveOnContainerEndEvent data)
        {
            var attached = false;
            foreach (var slot in _slots)
            {
                if(slot.Card) continue;
                
                slot.SetCard(data.Card);
                attached = true;

                break;
            }
            
            if (!attached)
            {
                Debug.Log("FAIL");
                EventBus.Raise(new OnGameFailedEvent());
            }
        }
    }
}
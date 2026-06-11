using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class SlotManager : MonoBehaviour
    {
        private List<Slot> _slots = new();
        [SerializeField] private GameObject _slotPrefab;

        private Dictionary<int, List<Card>> _cards = new();
        private int _lastGroupIndex;
        private int _cardAmount;
        

        private void Start()
        {
            for (var i = 0; i < 20; i++)
            {
                var clone = Instantiate(_slotPrefab, transform);
                clone.transform.localPosition = new Vector3(i * 1.2f, 0, 0);
                var slot = new Slot(null, clone.transform);
                _slots.Add(slot);
            }

            EventBus.Subscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEnd);
            EventBus.Subscribe<OnCardLeftSlotAreaEvent>(OnCardLeftSlotArea);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEnd);
            EventBus.Unsubscribe<OnCardLeftSlotAreaEvent>(OnCardLeftSlotArea);
        }

        private async void OnCardLeftSlotArea(OnCardLeftSlotAreaEvent data)
        {
            
            var groupIndex = 0;
            foreach (var card in _cards)
            {
                if (card.Value.Contains(data.Card))
                {
                    groupIndex = card.Key;
                    break;
                }
            }

            await ReleaseTheGroupTask(_cards[groupIndex]);
            EventBus.Raise(new OnCardGroupLeftCardAreaEvent()
            {
                Amount = _cards[groupIndex].Count
            });
            
            _cards[groupIndex].Clear();
            var groupsAmount = _lastGroupIndex + 1;
            if (groupsAmount >= 2)
            {
                for (var i = groupIndex + 1; i < groupsAmount; i++)
                {
                    foreach (var card in _cards[i])
                    {
                        _cards[i - 1].Add(card);
                    }

                    foreach (var card in _cards[i])
                    {
                        foreach (var slot in _slots)
                        {
                            if (slot.Card == card)
                            {
                                slot.SetCard(null, 0);
                                break;
                            }
                        }
                    }

                    _cards[i].Clear();
                    foreach (var card in _cards[i - 1])
                    {
                        foreach (var slot in _slots)
                        {
                            if (slot.Card == null)
                            {
                                slot.SetCard(card, 1);
                                break;
                            }
                        }
                    }
                    
                }
                
                _lastGroupIndex--;
            }
        }

        private async Task ReleaseTheGroupTask(List<Card> cards)
        {
            foreach (var card in cards)
            {
                card.LeaveSlotArea();
                _cardAmount--;
                foreach (var slot in _slots)
                {
                    if (slot.Card == card)
                    {
                        slot.SetCard(null, 0);
                        break;
                    }
                }

                await Helpers.WaitForSecondsUnity(.05f);
            }
        }

        private bool IsSlotAreaFull()
        {
            var foundEmpty = false;
            foreach (var slot in _slots)
            {
                if (slot.Card) continue;

                foundEmpty = true;
            }

            return !foundEmpty;
        }

        private void OnCardMoveOnContainerEnd(OnCardMoveOnContainerEndEvent data)
        {
            if (IsSlotAreaFull())
            {
                Debug.Log("FAIL");
                EventBus.Raise(new OnGameFailedEvent());
                return;
            }
            
            _cardAmount++;
            var slotIndex = _cardAmount - 1;
            if (_cardAmount <= 1)
            {
                _cards[_lastGroupIndex] = new List<Card>();
                _cards[_lastGroupIndex].Add(data.Card);
                _slots[slotIndex].SetCard(data.Card, 0);
            }
            else
            {
                var s = false;

                var gIndex = -1;
                foreach (var card in _cards)
                {
                    if (card.Value.Count > 0 && card.Value != null &&
                        card.Value[0].CardData.CardColor == data.Card.CardData.CardColor)
                    {
                        gIndex = card.Key;
                        break;
                    }
                }

                if (gIndex>=0)
                {
                    var index = 0;
                    foreach (var card in _cards)
                    {
                        if (card.Value.Count > 0 && card.Value != null &&
                            card.Value[0].CardData.CardColor == data.Card.CardData.CardColor)
                        {
                            var tAmount = 0;
                            for (var i = 0; i < index; i++)
                            {
                                tAmount += _cards[i].Count;
                            }

                            tAmount += card.Value.Count;

                            var lastSlotIndex = tAmount - 1;
                            _cards[card.Key].Add(data.Card);
                            s = true;

                            var a = new List<Card>();
                            for (var i = lastSlotIndex + 1; i < _slots.Count; i++)
                            {
                                if (_slots[i].Card)
                                {
                                    a.Add(_slots[i].Card);
                                    _slots[i].SetCard(null, 0);
                                }
                            }

                            _slots[lastSlotIndex + 1].SetCard(data.Card, 0);
                            var ss = lastSlotIndex + 2;
                            for (var i = 0; i < a.Count; i++)
                            {
                                _slots[ss].SetCard(a[i], 1);
                                ss++;
                            }

                            break;
                        }

                        index++;
                    }
                }
               

                if (!s)
                {
                    _lastGroupIndex++;
                    _cards[_lastGroupIndex] = new List<Card>();
                    _cards[_lastGroupIndex].Add(data.Card);
                    _slots[slotIndex].SetCard(data.Card, 0);
                }
            }
        }
    }
}
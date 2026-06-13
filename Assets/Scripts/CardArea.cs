using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

namespace Core
{
    public class CardArea : MonoBehaviour
    {
        [SerializeField] private CardData[] _cardDatas;
        private List<Card> _cards = new();
        private List<Card> _cardsTemp = new();
        
        private Dictionary<int, List<Card>> _cardGroups = new();
        private int _lastCardGroupIndex;

        private int _currentReleaseGroupIndex;
        
        public SplineContainer Container;
        private Sequence _warnSequence;
        
        private void Start()
        {
            SpawnAndAdjustCard(_cardDatas.Length);
            EventBus.Subscribe<OnGameResetEvent>(OnGameReset);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnGameResetEvent>(OnGameReset);
        }

        private void OnGameReset(OnGameResetEvent data)
        {
            foreach (var card in _cardsTemp)
                ObjectPool.Instance.Deposit(card.gameObject, "Card");
        }

        private void SpawnAndAdjustCard(int amount)
        {
            for (var j = 0; j < amount; j++)
            {
                var clone = ObjectPool.Instance.GetFromPool("Card");
       
                clone.transform.SetParent(transform);
                clone.transform.localScale = new Vector3(4, 2, 5);
                clone.transform.localEulerAngles = Vector3.zero;
                clone.SetActive(true);
                
                var card = clone.GetComponent<Card>();

                var i = _cards.Count;
            
                _cards.Add(card);
                _cardsTemp.Add(card);
                clone.transform.localPosition = new Vector3(0, i * -.2f, i * -.5f);
                card.SetAllProperty(_cardDatas[i], this);

                if (i == 0)
                {
                    _cardGroups[_lastCardGroupIndex] = new List<Card>();
                    _cardGroups[_lastCardGroupIndex].Add(card);
                    card.SetGroupOnArea(_lastCardGroupIndex);
                }
                else
                {
                    if (card.CardData.CardColor == _cardGroups[_lastCardGroupIndex][0].CardData.CardColor)
                    {
                        _cardGroups[_lastCardGroupIndex].Add(card);
                        card.SetGroupOnArea(_lastCardGroupIndex);
                    }
                    else
                    {
                        _lastCardGroupIndex++;
                        _cardGroups[_lastCardGroupIndex] = new List<Card>();
                        _cardGroups[_lastCardGroupIndex].Add(card);
                        card.SetGroupOnArea(_lastCardGroupIndex);
                    }
                }
            }
        }

        private void AdjustCards()
        {
            var i = 0;
            foreach (var card in _cards)
            {
                var pos = new Vector3(0, i * -.2f, i * -.5f);
                card.transform.DOLocalMove(pos, .2f);
                i++;
            }
        }

        public bool CheckReleaseGroupMatches(int cardGroup)
        {
            var matched = cardGroup == _currentReleaseGroupIndex;
            if (matched)
            {
                ReleaseTheGroup(() =>
                {
                    _currentReleaseGroupIndex++;
                    AdjustCards();
                });
                
            }
            else
                WarnForFalseGroupToRelease(cardGroup);
            
            return matched;
        }

        private void WarnForFalseGroupToRelease(int cardGroup)
        {
            _warnSequence?.Complete();
            foreach (var card in _cardGroups[cardGroup])
                card.transform.localPosition = new Vector3(0,card.transform.localPosition.y, card.transform.localPosition.z);
            
            foreach (var card in _cardGroups[cardGroup])
                WarnCardBounce(card.transform, card.transform.localPosition.x);
        }

        private void WarnCardBounce(Transform t, float startX)
        {
            _warnSequence = DOTween.Sequence();

            _warnSequence.Append(t.DOLocalMoveX(startX + 1.2f, 0.15f).SetEase(Ease.OutQuad));
            _warnSequence.Append(t.DOLocalMoveX(startX, 0.08f).SetEase(Ease.InQuad));

            for (int i = 0; i < 2; i++)
            {
                _warnSequence.Append(t.DOLocalMoveX(startX + 0.4f, 0.05f).SetEase(Ease.OutQuad));
                _warnSequence.Append(t.DOLocalMoveX(startX, 0.08f).SetEase(Ease.InQuad));
            }
        }

        private async void ReleaseTheGroup(Action completed = null)
        {
            try
            {
                if (!Factory.Instance.CheckReleaseAmountToReceive(_cardGroups[_currentReleaseGroupIndex].Count))
                    return; 
                var cards = new List<Card>();
                foreach (var card in _cardGroups[_currentReleaseGroupIndex])
                {
                    cards.Add(card);
                    _cards.Remove(card);
                }

                await ReleaseTheGroupTask(cards);
                
                EventBus.Raise(new OnCardGroupLeftCardAreaEvent()
                {
                    Cards = cards
                });
                completed?.Invoke();
            }
            catch (Exception e)
            {
            }
        }

        private async Task ReleaseTheGroupTask(List<Card> cards)
        {
            var i = 0;
            foreach (var card in cards)
            {
                card.LeaveCardArea(i);
                i++;
                await Helpers.WaitForSecondsUnity(.03f);
            }
        }
    }
}
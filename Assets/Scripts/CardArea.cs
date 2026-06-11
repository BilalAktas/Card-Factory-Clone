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

        [SerializeField] private GameObject _cardPrefab;
        private Dictionary<int, List<Card>> _cardGroups = new();
        private int _lastCardGroupIndex;

        private int _currentReleaseGroupIndex;

        public Transform Factory;
        public SplineContainer Container;
        
        private void Start()
        {
            SpawnAndAdjustCard(_cardDatas.Length);
        }

        private void SpawnAndAdjustCard(int amount)
        {
            for (var j = 0; j < amount; j++)
            {
                var clone = Instantiate(_cardPrefab, transform);
                var card = clone.GetComponent<Card>();

                var i = _cards.Count;
            
                _cards.Add(card);
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
            foreach (var card in _cardGroups[cardGroup])
                WarnCardBounce(card.transform, card.transform.localPosition.x);
        }

        private void WarnCardBounce(Transform t, float startX)
        {
            var seq = DOTween.Sequence();

            seq.Append(t.DOLocalMoveX(startX + 1.2f, 0.15f).SetEase(Ease.OutQuad));
            seq.Append(t.DOLocalMoveX(startX, 0.08f).SetEase(Ease.InQuad));

            for (int i = 0; i < 2; i++)
            {
                seq.Append(t.DOLocalMoveX(startX + 0.4f, 0.05f).SetEase(Ease.OutQuad));
                seq.Append(t.DOLocalMoveX(startX, 0.08f).SetEase(Ease.InQuad));
            }
        }

        private async void ReleaseTheGroup(Action completed = null)
        {
            try
            {
                var cards = new List<Card>();
                foreach (var card in _cardGroups[_currentReleaseGroupIndex])
                {
                    cards.Add(card);
                    _cards.Remove(card);
                }

                await ReleaseTheGroupTask(cards);
                
                completed?.Invoke();
            }
            catch (Exception e)
            {
            }
        }

        private async Task ReleaseTheGroupTask(List<Card> cards)
        {
            foreach (var card in cards)
            {
                card.LeaveCardArea();
                await Task.Delay(75);
            }
        }
    }
}
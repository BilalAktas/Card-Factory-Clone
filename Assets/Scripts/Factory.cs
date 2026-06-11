using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class Factory : Singleton<Factory>
    {
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Image _amountFillImage;
        private const int MAX_CARD_AMOUNT = 20;
        private int _currentCardAmount;
        public int RealCurrentCardAmount { get; private set; }
        
        [SerializeField] private Transform _mesh;

        private List<Card> _lastGroupCards = new();
        private int _lastGroupAmount;
        
        public int CurrentCardAmount
        {
            get => _currentCardAmount;
            set
            {
                _currentCardAmount = value;
                SetFillImage();
                SetAmountText();
            }
        }

        private void Start()
        {
            EventBus.Subscribe<OnCardLeftCardAreaEvent>(OnCarLeftCardArea);
            EventBus.Subscribe<OnCardArrivedFactoryEvent>(OnCardArrivedFactory);
            EventBus.Subscribe<OnMatchedEvent>(OnMatchedEvent);
            EventBus.Subscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEndEvent);
            EventBus.Subscribe<OnCardGroupLeftCardAreaEvent>(OnCardGroupArrived);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCardLeftCardAreaEvent>(OnCarLeftCardArea);
            EventBus.Unsubscribe<OnCardArrivedFactoryEvent>(OnCardArrivedFactory);
            EventBus.Unsubscribe<OnMatchedEvent>(OnMatchedEvent);
            EventBus.Unsubscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEndEvent);
            EventBus.Unsubscribe<OnCardGroupLeftCardAreaEvent>(OnCardGroupArrived);
        }

        private void OnCarLeftCardArea(OnCardLeftCardAreaEvent data)
        {
            RealCurrentCardAmount++;
        }

        private void OnCardArrivedFactory(OnCardArrivedFactoryEvent data)
        {
            CurrentCardAmount++;
            _mesh.DOComplete();
            _mesh.DOPunchScale(Vector3.one * .1f, .2f, 1, .8f);
            
            _lastGroupCards.Add(data.Card);
            if (_lastGroupCards.Count == _lastGroupAmount)
            {
                var cards = _lastGroupCards.ToList();
                _ = LetCardsMoveOnContainerTask(cards);
                _lastGroupCards.Clear();
            }
        }

        private void OnCardGroupArrived(OnCardGroupLeftCardAreaEvent data) => _lastGroupAmount = data.Amount;

        private async Task LetCardsMoveOnContainerTask(List<Card> cards)
        {
            foreach (var card in cards)
            {
                card.StartMoveOnContainer();
                await Helpers.WaitForSecondsUnity(.05f);
            }
        }

        private void OnMatchedEvent(OnMatchedEvent data)
        {
            CurrentCardAmount--;
            RealCurrentCardAmount--;
        }

        private void OnCardMoveOnContainerEndEvent(OnCardMoveOnContainerEndEvent data)
        {
            CurrentCardAmount--;
            RealCurrentCardAmount--;
        }

        private void SetFillImage()
        {
            var fill = (1f / MAX_CARD_AMOUNT) * CurrentCardAmount;
            _amountFillImage.DOKill();
            _amountFillImage.DOFillAmount(fill, .2f);
        }
        
        private void SetAmountText() => _amountText.text = $"{CurrentCardAmount}/{MAX_CARD_AMOUNT}";
    }
}
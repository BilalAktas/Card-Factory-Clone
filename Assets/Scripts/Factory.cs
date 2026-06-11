using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class Factory : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Image _amountFillImage;
        private const int MAX_CARD_AMOUNT = 20;
        private int _currentCardAmount;

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
            EventBus.Subscribe<OnCardLeftCardAreaEvent>(OnCardLeftCardArea);
            EventBus.Subscribe<OnMatchedEvent>(OnMatchedEvent);
            EventBus.Subscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEndEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCardLeftCardAreaEvent>(OnCardLeftCardArea);
            EventBus.Unsubscribe<OnMatchedEvent>(OnMatchedEvent);
            EventBus.Unsubscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEndEvent);
        }

        private void OnCardLeftCardArea(OnCardLeftCardAreaEvent data)
        {
            CurrentCardAmount++;
        }

        private void OnMatchedEvent(OnMatchedEvent data)
        {
            CurrentCardAmount--;
        }

        private void OnCardMoveOnContainerEndEvent(OnCardMoveOnContainerEndEvent data)
        {
            CurrentCardAmount--;
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
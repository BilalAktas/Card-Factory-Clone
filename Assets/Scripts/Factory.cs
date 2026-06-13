using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private int _currentCardAmount;
        public int RealCurrentCardAmount { get; private set; }

        public bool CheckReleaseAmountToReceive(int amount)
        {
            var s = RealCurrentCardAmount + amount <=
                    LevelManager.Instance.GetCurrentLevelData().MaxCardAmountOnConveyor;
            SetColor(s ? Color.white : Color.red);
            return s;
        }

        [SerializeField] private Transform _mesh;

        private readonly Queue<List<Card>> _cardGroups = new();
        private readonly Queue<List<Card>> _arrivedCards = new();
        private bool _isProcessingGroups;
        private int _receivedCardAmount;

        private Task _processGroupsTask;
        private CancellationTokenSource _processGroupsCts;

        [SerializeField] private Material[] _colorMaterials;
        private static readonly int _colorId = Shader.PropertyToID("_AlbedoColor");

        private Color _currentColor;

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
            EventBus.Subscribe<OnCardArrivedFactoryEvent>(OnCardArrivedFactory);
            EventBus.Subscribe<OnMatchedEvent>(OnMatchedEvent);
            EventBus.Subscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEndEvent);
            EventBus.Subscribe<OnCardGroupLeftCardAreaEvent>(OnCardGroupLeftCardArea);
            EventBus.Subscribe<OnGameResetEvent>(OnGameReset);

            SetColor(Color.white);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCardLeftCardAreaEvent>(OnCardLeftCardArea);
            EventBus.Unsubscribe<OnCardArrivedFactoryEvent>(OnCardArrivedFactory);
            EventBus.Unsubscribe<OnMatchedEvent>(OnMatchedEvent);
            EventBus.Unsubscribe<OnCardMoveOnContainerEndEvent>(OnCardMoveOnContainerEndEvent);
            EventBus.Unsubscribe<OnCardGroupLeftCardAreaEvent>(OnCardGroupLeftCardArea);
            EventBus.Unsubscribe<OnGameResetEvent>(OnGameReset);
        }

        private void SetColor(Color color)
        {
            foreach (var mat in _colorMaterials)
                mat.SetColor(_colorId, color);

            if (color == Color.red)
            {
                if (_currentColor != color)
                {
                    _amountText.transform.DOComplete();
                    _amountText.transform.DOPunchScale(Vector3.one * .2f, .5f, 1, .8f).SetLoops(-1);   
                }
            }
            else
            {
                _amountText.transform.DOKill();
                _amountText.transform.localScale = Vector3.one;
            }
            
            _currentColor = color;
        }

        private void OnGameReset(OnGameResetEvent data)
        {
            _processGroupsCts?.Cancel();
            DepositAllCards();
        }

        private void DepositAllCards()
        {
            foreach (var cards in _cardGroups)
            {
                foreach (var card in cards)
                {
                    ObjectPool.Instance.Deposit(card.gameObject, "Card");
                }
            }
        }

        private void OnCardLeftCardArea(OnCardLeftCardAreaEvent data)
        {
            RealCurrentCardAmount++;
        }

        private void OnCardArrivedFactory(OnCardArrivedFactoryEvent data)
        {
            CurrentCardAmount++;
            _mesh.DOComplete();
            _mesh.DOPunchScale(Vector3.one * .1f, .1f, 1, .8f);

            _receivedCardAmount++;
            if (_receivedCardAmount == _arrivedCards.First().Count)
            {
                var l = _arrivedCards.Dequeue();
                _cardGroups.Enqueue(l);
                _receivedCardAmount = 0;

                if (!_isProcessingGroups)
                {
                    _processGroupsCts?.Cancel();
                    _processGroupsCts = new CancellationTokenSource();

                    _processGroupsTask = ProcessGroups(_processGroupsCts.Token);
                }
            }
        }

        private void OnCardGroupLeftCardArea(OnCardGroupLeftCardAreaEvent data)
        {
            _arrivedCards.Enqueue(data.Cards.ToList());
        }

        private async Task ProcessGroups(CancellationToken token)
        {
            _isProcessingGroups = true;

            try
            {
                while (_cardGroups.Count > 0 && GameManager.Instance.GameState == GameState.Playing)
                {
                    token.ThrowIfCancellationRequested();

                    var cards = _cardGroups.Dequeue();

                    var index = 0;
                    foreach (var card in cards)
                    {
                        //await Helpers.WaitForSecondsUnity(.06f);

                        token.ThrowIfCancellationRequested();

                        card.StartMoveOnContainer(index * .02f);
                        index++;
                        
                        if (token.IsCancellationRequested || GameManager.Instance.GameState == GameState.Idle)
                            return;
                    }
                    
                    await Helpers.WaitForSecondsUnity(.25f + (cards.Count * .1f));
                }
            }
            catch (OperationCanceledException)
            {
                if(GameManager.Instance.GameState == GameState.Idle) DepositAllCards();
            }
            finally
            {
                _isProcessingGroups = false;
                if(GameManager.Instance.GameState == GameState.Idle) DepositAllCards();
            }
        }

        private void OnMatchedEvent(OnMatchedEvent data)
        {
            CurrentCardAmount--;
            RealCurrentCardAmount--;
            SetColor(Color.white);
        }

        private void OnCardMoveOnContainerEndEvent(OnCardMoveOnContainerEndEvent data)
        {
            CurrentCardAmount--;
            RealCurrentCardAmount--;
            SetColor(Color.white);
        }

        private void SetFillImage()
        {
            var fill = (1f / LevelManager.Instance.GetCurrentLevelData().MaxCardAmountOnConveyor) * CurrentCardAmount;
            _amountFillImage.DOKill();
            _amountFillImage.DOFillAmount(fill, .2f);
        }

        private void SetAmountText() => _amountText.text =
            $"{CurrentCardAmount}/{LevelManager.Instance.GetCurrentLevelData().MaxCardAmountOnConveyor}";
    }
}
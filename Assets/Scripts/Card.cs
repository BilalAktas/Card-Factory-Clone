using System;
using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class Card : MonoBehaviour, IClickable
    {
        public CardData CardData { get; private set; }
        public CardState CardState { get; private set; }
        
        private MaterialPropertyBlock _materialPropertyBlock;
        private static readonly int _colorId = Shader.PropertyToID("_AlbedoColor");
        private MeshRenderer _meshRenderer;

        private int _groupIndexOnArea = -1;
        private CardArea _currentCardArea;

        private bool _canMoveOnContainer;
        private float _timer;

        [SerializeField] private AudioSource _leaveAreaSfx;
        
        
        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            _meshRenderer = GetComponent<MeshRenderer>();
            
            EventBus.Subscribe<OnGameFailedEvent>(OnGameFailed);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnGameFailedEvent>(OnGameFailed);
        }


        public void Select()
        {
            if (CardState == CardState.OnContainer) return;

            switch (CardState)
            {
                case CardState.OnCardArea:
                    //Debug.Log($"selected {CardData.CardColor} and group Index {_groupIndexOnArea}");
                    var matched = _currentCardArea.CheckReleaseGroupMatches(_groupIndexOnArea);
                    break;
                case CardState.OnSlotArea:
                    
                    break;
            }
            
        }

        public bool Clickable() => true;

        public void SetAllProperty(CardData data, CardArea cardArea)
        {
            CardData = data;
            _currentCardArea = cardArea;
            
            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor(_colorId, CardData.CardMaterialColor);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        public void SetGroupOnArea(int index) => _groupIndexOnArea = index;

        public void LeaveCardArea()
        {
            GetComponent<Collider>().enabled = false;
            _leaveAreaSfx.Play();
            
            CardState = CardState.OnContainer;
            EventBus.Raise(new OnCardLeftCardAreaEvent()
            {
                Card = this
            });
            
            transform.SetParent(null);
            var seq = DOTween.Sequence();
            
            var path = new Vector3[3];
            path[0] = _currentCardArea.Factory.position + _currentCardArea.Factory.forward * 10f + Vector3.up * 6f;
            path[1] = _currentCardArea.Factory.position + _currentCardArea.Factory.forward * 5f + Vector3.up * 25;
            path[2] = _currentCardArea.Factory.position + Vector3.up * 2f;
            seq.Append(transform.DOPath(path, 1f, PathType.CatmullRom)).SetEase(Ease.Linear);
            var r = _currentCardArea.Factory.rotation;
            seq.Insert(.3f, transform.DORotateQuaternion(r * Quaternion.Euler(90,-180,0), .25f).SetEase(Ease.Linear));
            seq.OnComplete(() =>
            {
                _canMoveOnContainer = true;
            });
        }

        private bool _checkBox = true;
        private void Update()
        {
            if (!_canMoveOnContainer) return;
            
            _timer += CardData.CardSpeedOnContainer * Time.deltaTime;

            transform.position = _currentCardArea.Container.EvaluatePosition(_timer);
            transform.position += Vector3.up * 5f;
            
            Vector3 tan = _currentCardArea.Container.EvaluateTangent(_timer);
            if (tan.sqrMagnitude > 1e-6f)
            {
                var dir = tan.normalized;
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                transform.localEulerAngles = new Vector3(90f, transform.localEulerAngles.y, transform.localEulerAngles.z);
            }

            if (Physics.Raycast(transform.position, transform.right, out var hit, 10))
            {
                if (hit.transform.TryGetComponent(out ProductBox box))
                {
                    if (box.Data.CardColor == CardData.CardColor)
                    {
                        var s = box.AttachCard(this);
                        if (s.Item1)
                        {
                            _canMoveOnContainer = false;
                            transform.SetParent(s.Item2.parent);
                            transform.DOLocalMove(s.Item2.localPosition, .25f);
                            transform.DOLocalRotateQuaternion(s.Item2.localRotation * Quaternion.Euler(0,0,90), .25f);
                        
                            EventBus.Raise(new OnMatchedEvent());   
                        }
                    }
                }
            }    
            
            
            if (_timer >= 1)
            {
                _canMoveOnContainer = false;
                EventBus.Raise(new OnCardMoveOnContainerEndEvent()
                {
                    Card = this
                });
            }
        }

        public void MoveToSlot(Slot slot)
        {
            var seq = DOTween.Sequence();
            seq.Append(transform.DOMove(slot.Visual.position + Vector3.up * 1, .25f).SetEase(Ease.Linear));
            seq.Insert(.1f, transform.DORotateQuaternion(Quaternion.Euler(0, 180, 90), .1f).SetEase(Ease.Linear));
        }
        
        private void OnGameFailed(OnGameFailedEvent data) => _canMoveOnContainer = false;
    }
}
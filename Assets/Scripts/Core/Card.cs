using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class Card : MonoBehaviour, IClickable
    {
        public CardData CardData { get; private set; }
        private CardState _cardState;

        public CardState CardState
        {
            get { return _cardState; }
            set
            {
                _cardState = value;
                _collider.enabled = _cardState != CardState.OnContainer;
            }
        }
        
        private MaterialPropertyBlock _materialPropertyBlock;
        private static readonly int _colorId = Shader.PropertyToID("_AlbedoColor");
        private static readonly int _outlineColorId = Shader.PropertyToID("_OutlineColor");
        private MeshRenderer _meshRenderer;

        private int _groupIndexOnArea = -1;
        private CardArea _currentCardArea;

        private bool _canMoveOnContainer;
        private float _timer;

        private Sequence _moveSlotSequence;
        private Collider _collider;
        [SerializeField] private AudioSource _leaveAreaSfx;
        private float _startOffset;
        
        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            _meshRenderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<Collider>();
            
            EventBus.Subscribe<OnGameFailedEvent>(OnGameFailed);
        }

        private void OnEnable()
        {
            CardState = CardState.OnCardArea;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnGameFailedEvent>(OnGameFailed);
        }


        public void Select()
        {
            switch (CardState)
            {
                case CardState.OnCardArea:
                    //Debug.Log($"selected {CardData.CardColor} and group Index {_groupIndexOnArea}");
                    var matched = _currentCardArea.CheckReleaseGroupMatches(_groupIndexOnArea);
                    break;
                case CardState.OnSlotArea:
                    EventBus.Raise(new OnCardLeftSlotAreaEvent()
                    {
                        Card = this
                    });
                    break;
            }
            
        }

        public bool Clickable() => CardState != CardState.OnContainer;

        public void SetAllProperty(CardData data, CardArea cardArea)
        {
            CardData = data;
            _currentCardArea = cardArea;

            CardState = CardState.OnCardArea;
            
            for (var i = 0; i < _meshRenderer.sharedMaterials.Length; i++)
            {
                if (i == 0)
                {
                    _meshRenderer.GetPropertyBlock(_materialPropertyBlock, i);
                    _materialPropertyBlock.SetColor(_colorId, CardData.CardMaterialColor);
                    _meshRenderer.SetPropertyBlock(_materialPropertyBlock, i);    
                }
                else
                {
                    _meshRenderer.GetPropertyBlock(_materialPropertyBlock, i);
                    _materialPropertyBlock.SetColor(_colorId, CardData.CardOutlineMeshMaterialColor);
                    _materialPropertyBlock.SetColor(_outlineColorId, CardData.CardOutLineColor);
                    _meshRenderer.SetPropertyBlock(_materialPropertyBlock, i);    
                    
                }
            }
        }

        public void SetGroupOnArea(int index) => _groupIndexOnArea = index;

        public void LeaveCardArea(float space)
        {
            EventBus.Raise(new OnCardLeftCardAreaEvent()
            {
                Card = this
            });
            GoToFactory(space);
        }

        public void LeaveSlotArea(float space)
        {
            GoToFactory(space);
        }

        private void GoToFactory(float space)
        {
            _leaveAreaSfx?.Play();

            var exState = CardState;
            CardState = CardState.OnContainer;
            transform.SetParent(null);
            var seq = DOTween.Sequence();
            
            var path = new Vector3[2];
            var factoryTransform = Factory.Instance.transform;
            path[0] = factoryTransform.position + factoryTransform.forward * 5f + Vector3.up * 25f;
            //path[1] = factoryTransform.position + factoryTransform.forward * 5f + Vector3.up * 25;
            path[1] = factoryTransform.position + factoryTransform.forward * -3f + Vector3.up * 5f;
            
            seq.Append(transform.DOPath(path, .5f + (space * .02f), PathType.CatmullRom)
                .SetEase(Ease.Linear));
            
            //seq.Append(transform.DOPath(path, 1f, PathType.CatmullRom)).SetEase(Ease.Linear);
            seq.Join(transform.DOScale(new Vector3(4, 10, 5), .25f));
            seq.AppendCallback(() =>
            {
                EventBus.Raise(new OnCardArrivedFactoryEvent()
                {
                    Card = this
                });
            });
            var r = factoryTransform.rotation;
            switch (exState)
            {
                case CardState.OnSlotArea:
                    seq.Insert(.05f, transform.DORotateQuaternion(r * Quaternion.Euler(90,-180,0), .5f).SetEase(Ease.Linear));        
                    break;
                case CardState.OnCardArea:
                    seq.Insert(.05f, transform.DORotateQuaternion(r * Quaternion.Euler(90,-180,0), .5f).SetEase(Ease.Linear));
                    break;
            }
            seq.OnComplete(() =>
            {
                transform.localScale = Vector3.zero;
                //_canMoveOnContainer = true;
            });
        }

        
        public void StartMoveOnContainer(float offset)
        {
            _startOffset = offset;
            _timer = 0;
            _canMoveOnContainer = true;

            transform.DOMove(
                _currentCardArea.Container.EvaluatePosition(0f),
                0.1f
            ).OnComplete(() =>
            {
                transform.DOScale(new Vector3(4, 10, 5), .05f);
            });
        }
        
        private void Update()
        {
            if(GameManager.Instance.GameState == GameState.Idle) return;
            if (!_canMoveOnContainer) return;
            
            _timer += CardData.CardSpeedOnContainer * Time.deltaTime;

            var t = _timer - _startOffset;

            if (t < 0)
                return;

            Vector3 pos = _currentCardArea.Container.EvaluatePosition(t);
            transform.position = pos + Vector3.up * 5.5f;

            Vector3 tan = _currentCardArea.Container.EvaluateTangent(t);

            if (tan.sqrMagnitude > 1e-6f)
            {
                var dir = tan.normalized;
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                transform.localEulerAngles =
                    new Vector3(90f, transform.localEulerAngles.y, transform.localEulerAngles.z);
            }
            
            
            if (Physics.Raycast(transform.position, transform.right, out var hit, 10))
            {
                if (hit.transform.TryGetComponent(out ProductBox box))
                {
                    var facingDot = Vector3.Dot(transform.right, box.transform.right);

                    if (facingDot < -0.99f)
                    {   
                        if (box.Data.CardColor == CardData.CardColor)
                        {
                            var s = box.AttachCard(this);
                            if (s.Item1)
                            {
                                _canMoveOnContainer = false;
                                transform.SetParent(s.Item2.parent);
                                var seq = DOTween.Sequence();
                                seq.Append(transform.DOLocalJump(s.Item2.localPosition + Vector3.up * 2.5f, 5f,1, .5f));
                                seq.Insert(.05f, transform.DOLocalRotateQuaternion(
                                    s.Item2.localRotation * Quaternion.Euler(0, 0, 90), .25f));
                                seq.Insert(.2f, transform.DOScale(new Vector3(4, 10, 3), .25f));

                                seq.OnComplete(() =>
                                {
                                    EventBus.Raise(new OnMatchedEvent()
                                    {
                                        Card = this,
                                        Box = box
                                    });
                                });
                            }
                        }
                    }
                }
            }    
            
            
            if (t >= 1)
            {
                _canMoveOnContainer = false;
                EventBus.Raise(new OnCardMoveOnContainerEndEvent()
                {
                    Card = this
                });
            }
        }

        
        public void MoveToSlot(Slot slot, int state)
        {
            if (_moveSlotSequence != null)
            {
                _moveSlotSequence.Kill(false);
                transform.rotation = Quaternion.Euler(0, 180, 90);
            }
            
            _moveSlotSequence = DOTween.Sequence();
            switch (state)
            {
                case 0:
                    var target = slot.Visual.position + Vector3.up * 2f;
                    var p = new Vector3[2];
                    p[0] = ((target + transform.position) / 2) + Vector3.up * 2f;
                    p[1] = target;
                    _moveSlotSequence.Append(transform.DOPath(p, .75f, PathType.CatmullRom).SetEase(Ease.Linear));
                    _moveSlotSequence.Append(transform.DOPunchPosition(new Vector3(0, -1f, 0), .3f, 0, .8f));
                    _moveSlotSequence.Insert(.6f, transform.DOPunchScale(Vector3.one * .4f, .2f, 1, .8f));
                    _moveSlotSequence.Insert(.25f, transform.DORotateQuaternion(Quaternion.Euler(transform.localEulerAngles.x + 360, 180, 90), .25f).SetEase(Ease.Linear));
                    break;
                case 1:
                    var path = new Vector3[2];
                    path[0] = ((slot.Visual.position + transform.position) / 2) + Vector3.up * 2;
                    path[1] = slot.Visual.position + Vector3.up * 2f;
                    _moveSlotSequence.Append(transform.DOPath(path, .5f, PathType.CatmullRom));
                    _moveSlotSequence.Insert(.6f, transform.DOPunchPosition(new Vector3(0, -1f, 0), .3f, 0, .8f));
                    //_moveSlotSequence.Insert(.6f, transform.DOPunchScale(Vector3.one * .4f, .2f, 1, .8f));
                    break;
            }
            
            _moveSlotSequence.OnComplete(() =>
            {
                CardState = CardState.OnSlotArea;
                _moveSlotSequence = null;

                transform.rotation = Quaternion.Euler(0, 180, 90);
            });
        }

        private void OnGameFailed(OnGameFailedEvent data)
        {
            _canMoveOnContainer = false;
            transform.DOKill();
        }
    }
}
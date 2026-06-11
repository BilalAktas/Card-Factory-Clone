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
                GetComponent<Collider>().enabled = _cardState != CardState.OnContainer;
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
                    EventBus.Raise(new OnCardLeftSlotAreaEvent()
                    {
                        Card = this
                    });
                    break;
            }
            
        }

        public bool Clickable() => true;

        public void SetAllProperty(CardData data, CardArea cardArea)
        {
            CardData = data;
            _currentCardArea = cardArea;
           
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

        public void LeaveCardArea()
        {
            EventBus.Raise(new OnCardLeftCardAreaEvent()
            {
                Card = this
            });
            GoToFactory();
        }

        public void LeaveSlotArea()
        {
            GoToFactory();
        }

        private void GoToFactory()
        {
            _leaveAreaSfx?.Play();

            var exState = CardState;
            CardState = CardState.OnContainer;
            transform.SetParent(null);
            var seq = DOTween.Sequence();
            
            var path = new Vector3[3];
            path[0] = _currentCardArea.Factory.position + _currentCardArea.Factory.forward * 10f + Vector3.up * 6f;
            path[1] = _currentCardArea.Factory.position + _currentCardArea.Factory.forward * 5f + Vector3.up * 25;
            path[2] = _currentCardArea.Factory.position +  _currentCardArea.Factory.forward * -4f + Vector3.up * 2f;
            seq.Append(transform.DOPath(path, 1f, PathType.CatmullRom)).SetEase(Ease.Linear);
            seq.Join(transform.DOScale(new Vector3(4, 10, 5), .25f));
            seq.AppendCallback(() =>
            {
                EventBus.Raise(new OnCardArrivedFactoryEvent()
                {
                    Card = this
                });
            });
            var r = _currentCardArea.Factory.rotation;
            switch (exState)
            {
                case CardState.OnSlotArea:
                    seq.Insert(.1f, transform.DORotateQuaternion(r * Quaternion.Euler(90,-180,0), .5f).SetEase(Ease.Linear));        
                    break;
                case CardState.OnCardArea:
                    seq.Insert(.1f, transform.DORotateQuaternion(r * Quaternion.Euler(90,-180,0), .5f).SetEase(Ease.Linear));
                    break;
            }
            seq.OnComplete(() =>
            {
                //_canMoveOnContainer = true;
            });
        }

        public void StartMoveOnContainer()
        {
            _timer = 0;
            _canMoveOnContainer = true;
        }
        
        private void Update()
        {
            if (!_canMoveOnContainer) return;
            
            _timer += CardData.CardSpeedOnContainer * Time.deltaTime;

            transform.position = _currentCardArea.Container.EvaluatePosition(_timer);
            transform.position += Vector3.up * 5.5f;
            
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
                                seq.Insert(.25f, transform.DOLocalRotateQuaternion(
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
            
            
            if (_timer >= 1)
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
        
        private void OnGameFailed(OnGameFailedEvent data) => _canMoveOnContainer = false;
    }
}
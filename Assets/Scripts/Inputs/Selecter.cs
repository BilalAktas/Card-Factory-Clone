using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Core
{
    public class Selecter : MonoBehaviour
    {
        [SerializeField] private float _coolDown;
        private float _currentTime;
        private bool _firstSelect;

        private bool _canSelect = true;
        
        private Camera _camera;
        [SerializeField] private RectTransform _handRect;
        [SerializeField] private Vector3 _handOffset;

        private void Start()
        {
            //EnhancedTouchSupport.Enable();
            _camera = Camera.main;
            EventBus.Subscribe<OnGameFailedEvent>(OnGameFailed);
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnGameFailedEvent>(OnGameFailed);
        }
        
        private void Update()
        {
            _handRect.position = Mouse.current.position.value;
            _handRect.position += _handOffset;
            
            if (!_canSelect) return;
            // if (Touch.activeTouches.Count == 0) return;
            //
            // var t = Touch.activeTouches[0];
            // if(t.phase == TouchPhase.Began) HandleTap(t.screenPosition);
            
            if(Mouse.current.leftButton.wasPressedThisFrame) HandleTap(Mouse.current.position.value);
        }
   
        private void HandleTap(Vector3 pos)
        {
            if (_firstSelect && Time.time - _currentTime < _coolDown) return;
            _handRect.DOPunchScale(Vector3.one * -.25f, .2f, 1, .8f);
            
            _currentTime = Time.time;
            var ray = _camera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.transform.TryGetComponent(out IClickable clickable))
                {
                    if (!clickable.Clickable()) return;
                    clickable.Select();
                }
            }
        }
        
        private void OnGameFailed(OnGameFailedEvent data) => _canSelect = false;
    }
}
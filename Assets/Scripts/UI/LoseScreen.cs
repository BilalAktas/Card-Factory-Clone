using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class LoseScreen : MonoBehaviour
    {
        [SerializeField] private Button _retryButton;

        private void Start()
        {
            _retryButton.onClick.AddListener(OnRetry);
        }

        private void OnDestroy()
        {
            _retryButton.onClick.RemoveAllListeners();
        }

        private void OnRetry()
        {
            gameObject.SetActive(false);
            EventBus.Raise(new OnGameResetEvent());
            DOVirtual.DelayedCall(.2f, () =>
            {
                EventBus.Raise(new OnNextLevelButtonClickedEvent());
            });
        }
    }
}
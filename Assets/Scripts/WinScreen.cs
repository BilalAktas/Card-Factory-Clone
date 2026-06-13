using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class WinScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rewardCoinText;
        [SerializeField] private Button _nextButton;

        [SerializeField] private Animation _screenAnimation;
        [SerializeField] private Animation _rewardCoinsAnimation;
        
        
        private void OnEnable()
        {
            SetRewardText();
            _nextButton.onClick.AddListener(OnNext);
        }

        private void OnDisable()
        {
            _nextButton.onClick.RemoveAllListeners();
        }

        private void SetRewardText() => _rewardCoinText.text = RewardAmount().ToString();

        private int RewardAmount() => SaveLoadManager.GetLevel() * 10;

        private void OnNext()
        {
            EventBus.Raise(new OnGameResetEvent());
            _screenAnimation.Play("anim_ui_reward_screen");
            _rewardCoinsAnimation.Play();
            DOVirtual.DelayedCall(1f, () =>
            {
                PlayerEconomyManager.Instance.AddCoin(RewardAmount(), true);
                
                SaveLoadManager.IncreaseLevel();
                DOVirtual.DelayedCall(1f, () =>
                {
                    gameObject.SetActive(false);
                    EventBus.Raise(new OnNextLevelButtonClickedEvent());
                });
            });
        }
    }
}
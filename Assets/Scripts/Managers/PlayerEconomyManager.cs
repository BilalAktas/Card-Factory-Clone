using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core
{
    public class PlayerEconomyManager : Singleton<PlayerEconomyManager>
    {
        [SerializeField] private TextMeshProUGUI _coinText;

        private void Start()
        {
            _coinText.text = Helpers.ConvertToKBM(GetCoin());
        }

        private int GetCoin() => PlayerPrefs.GetInt("Coin", 0);
        private void SetCoin(int value) => PlayerPrefs.SetInt("Coin", value);

        public void AddCoin(int amount, bool anim)
        {
            var currentCoin = GetCoin();
            var nCoin = currentCoin + amount;
            SetCoin(nCoin);
            switch (anim)
            {
                case true:
                    DOTween.To(() => currentCoin, x => _coinText.text = Helpers.ConvertToKBM(x), nCoin, 1)
                        .SetEase(Ease.Linear);
                    break;
                case false:
                    _coinText.text = Helpers.ConvertToKBM(nCoin);
                    break;
            }
        }

        public bool CanSpendCoin(int amount) => GetCoin() >= amount;

        public void SpendCoin(int amount)
        {
            SetCoin(GetCoin() - amount);
            _coinText.text = Helpers.ConvertToKBM(GetCoin());
        }
    }
}
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private LevelData[] _levelDatas;

        public LevelData GetCurrentLevelData()
        {
            var id = SaveLoadManager.GetLevelIndex() % _levelDatas.Length;
            return _levelDatas[id];
        }

        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image _levelFillImage;
        private int _filledProductBoxAmount;

        private GameObject _clonedLevel;
        
        private void Start()
        {
            UpdateLevelText();
            _levelFillImage.fillAmount = 0;
            SpawnLevel();
            
            EventBus.Subscribe<OnProductBoxFilledEvent>(OnProductBoxFilled);
            EventBus.Subscribe<OnNextLevelButtonClickedEvent>(OnNextLevelButtonClicked);
            EventBus.Subscribe<OnGameResetEvent>(OnGameReset);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnProductBoxFilledEvent>(OnProductBoxFilled);
            EventBus.Unsubscribe<OnNextLevelButtonClickedEvent>(OnNextLevelButtonClicked);
            EventBus.Unsubscribe<OnGameResetEvent>(OnGameReset);
        }
        
        private void OnGameReset(OnGameResetEvent data)
        {
            _filledProductBoxAmount = 0;
            _levelFillImage.fillAmount = 0;
            UpdateLevelText();
        }

        private void OnNextLevelButtonClicked(OnNextLevelButtonClickedEvent data) => SpawnLevel();

        private void UpdateLevelText() => _levelText.text = $"LEVEL {SaveLoadManager.GetLevel()}";

        private int GetTotalProductBoxAmount()
        {
            var amount = 0;
            foreach (var data in GetCurrentLevelData().ProductAreaForLevelDatas)
                amount += data.ProductBoxDatas.Length;
            
            return amount;
        }
        
        private void OnProductBoxFilled(OnProductBoxFilledEvent data)
        {
            _levelFillImage.DOKill();

            _filledProductBoxAmount++;
            var totalAmount = GetTotalProductBoxAmount();
            
            var fillAmount = (1f / totalAmount) * _filledProductBoxAmount;
            _levelFillImage.DOFillAmount(fillAmount, .2f);
            
            if(_filledProductBoxAmount >= totalAmount)
            {
                EventBus.Raise(new OnLevelCompletedEvent());
            }
        }

        private void SpawnLevel()
        {
            if (_clonedLevel) Destroy(_clonedLevel);

            _clonedLevel = Instantiate(GetCurrentLevelData().LevelBasePrefab);
            _clonedLevel.transform.position = Vector3.zero;
            UpdateLevelText();
            EventBus.Raise(new OnLevelSpawnedEvent());
        }
    }
}
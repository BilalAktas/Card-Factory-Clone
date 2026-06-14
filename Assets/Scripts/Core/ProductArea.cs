using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Core
{
    public class ProductArea : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _productBoxAmountText;
        private int _currentProductBoxIndex = 0;
        private List<GameObject> _spawnedProductBoxes = new();
        
        private void Start()
        {
            Invoke(nameof(SpawnProductBox), .1f);
            UpdateBoxAmountText();
            
            EventBus.Subscribe<OnProductBoxFilledEvent>(OnProductBoxFilled);
            EventBus.Subscribe<OnGameResetEvent>(OnGameReset);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnProductBoxFilledEvent>(OnProductBoxFilled);
            EventBus.Unsubscribe<OnGameResetEvent>(OnGameReset);
        }

        private void OnGameReset(OnGameResetEvent data)
        {
            foreach (var box in _spawnedProductBoxes)
                ObjectPool.Instance.Deposit(box, "ProductBox");
        }
        
        private void SpawnProductBox()
        {
            var clone = ObjectPool.Instance.GetFromPool("ProductBox");
            clone.transform.SetParent(transform);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localScale = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;
            clone.SetActive(true);
            clone.GetComponent<ProductBox>().SetAllProperty(LevelManager.Instance.GetCurrentLevelData().ProductAreaForLevelDatas[transform.GetSiblingIndex()].ProductBoxDatas[_currentProductBoxIndex]);
            _spawnedProductBoxes.Add(clone);
        }

        private void UpdateBoxAmountText() => _productBoxAmountText.text = $"{GetAllBoxAmount() - _currentProductBoxIndex}";

        private int GetAllBoxAmount() => LevelManager.Instance.GetCurrentLevelData()
            .ProductAreaForLevelDatas[transform.GetSiblingIndex()].ProductBoxDatas.Length;

        private void OnProductBoxFilled(OnProductBoxFilledEvent data)
        {
            if (data.ProductBoxAreaIndex != transform.GetSiblingIndex()) return;
            
            _currentProductBoxIndex++;
            UpdateBoxAmountText();
            if(_currentProductBoxIndex < GetAllBoxAmount()) Invoke(nameof(SpawnProductBox), .2f);
        }
    }
}
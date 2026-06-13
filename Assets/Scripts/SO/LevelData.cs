using UnityEngine;

namespace Core
{
    [System.Serializable]
    public struct ProductAreaForLevelData
    {
        public int ProductAreaId;
        public ProductBoxData[] ProductBoxDatas;
    }
    
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private ProductAreaForLevelData[] _productAreaForLevelDatas;
        [SerializeField] private int _maxCardAmountOnConveyor;
        [SerializeField] private GameObject _levelBasePrefab;
        [SerializeField] private int _slotAmount;
        
        public ProductAreaForLevelData[] ProductAreaForLevelDatas => _productAreaForLevelDatas;
        public int MaxCardAmountOnConveyor => _maxCardAmountOnConveyor;
        public GameObject LevelBasePrefab => _levelBasePrefab;
        public int SlotAmount => _slotAmount;
    }
}
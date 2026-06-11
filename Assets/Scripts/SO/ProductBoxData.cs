using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "ProductBoxData", menuName = "ScriptableObjects/ProductBox", order = 0)]
    public class ProductBoxData : ScriptableObject
    {
        [SerializeField] private CardColor _cardColor;
        [SerializeField] private Color _cardMaterialColor;
        
        public CardColor CardColor => _cardColor;
        public Color CardMaterialColor => _cardMaterialColor;
    }
}
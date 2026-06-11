using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData")]
    public class CardData : ScriptableObject
    {
        [SerializeField] private CardColor _cardColor;
        [SerializeField] private Color _cardMaterialColor;
        [SerializeField] private Color _cardOutlineMeshMaterialColor;
        [SerializeField] private Color _cardOutlineColor;
        [SerializeField] private float _cardSpeedOnContainer;
        
        public CardColor CardColor => _cardColor;
        public Color CardMaterialColor => _cardMaterialColor;
        public Color CardOutLineColor => _cardOutlineColor;
        public float CardSpeedOnContainer => _cardSpeedOnContainer;
        public Color CardOutlineMeshMaterialColor => _cardOutlineMeshMaterialColor;
    }
}
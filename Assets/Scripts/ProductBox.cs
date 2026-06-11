using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class ProductBox : MonoBehaviour
    {
        public ProductBoxData Data {get; private set;}
        private Dictionary<Transform, Card> _slots;
        
        private MaterialPropertyBlock _materialPropertyBlock;
        private static readonly int _colorId = Shader.PropertyToID("_AlbedoColor");
        private MeshRenderer[] _meshRenderer;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            _meshRenderer = GetComponentsInChildren<MeshRenderer>();
        }

        public void SetAllProperty(ProductBoxData data)
        {
            Data = data;
            
            _slots = new Dictionary<Transform, Card>();
            
            foreach (var renderer in _meshRenderer)
            {
                renderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetColor(_colorId, data.CardMaterialColor);
                renderer.SetPropertyBlock(_materialPropertyBlock);

                _slots[renderer.transform] = null;
            }
        }

        public (bool, Transform) AttachCard(Card card)
        {
            foreach (var renderer in _meshRenderer)
            {
                if (_slots[renderer.transform] == null)
                {
                    _slots[renderer.transform] = card;
                    Debug.Log($"attach c {Data.CardColor} slot name {renderer.name} card name {card.name}");
                    return (true, renderer.transform);
                }
            }

            return (false, null);
        }
    }
}
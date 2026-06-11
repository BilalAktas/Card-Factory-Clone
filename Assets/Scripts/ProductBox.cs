using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class ProductBox : MonoBehaviour
    {
        public ProductBoxData Data { get; private set; }
        private Dictionary<Transform, Card> _slots;

        private MaterialPropertyBlock _materialPropertyBlock;
        private static readonly int _colorId = Shader.PropertyToID("_AlbedoColor");
        private MeshRenderer[] _meshRenderer;

        private int _matched;
        [SerializeField] private AudioSource _filledSound;
        [SerializeField] private GameObject _filledParticlePrefab;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            _meshRenderer = GetComponentsInChildren<MeshRenderer>();

            EventBus.Subscribe<OnMatchedEvent>(OnMatched);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnMatchedEvent>(OnMatched);
        }

        private void OnMatched(OnMatchedEvent data)
        {
            if (data.Box != this) return;
            _matched++;

            transform.DOComplete();
            transform.DOPunchPosition(new Vector3(0, -.5f, 0), .6f, 1, .25f);
            
            if (_matched < _slots.Count) return;

            _filledSound.Play();

            var seq = DOTween.Sequence();
            seq.Insert(.2f, transform.DOMove(transform.position + Vector3.up * 2f, .2f));
            seq.Append(transform.DOScale(Vector3.one * .25f, .2f));
            seq.Append(transform.DOScale(Vector3.zero, .2f));

            seq.OnComplete(() =>
            {
                var clone = Instantiate(_filledParticlePrefab);
                clone.transform.position = transform.position + Vector3.up * 3f;
            });
        }

        public void SetAllProperty(ProductBoxData data)
        {
            Data = data;

            _slots = new Dictionary<Transform, Card>();

            foreach (var renderer in _meshRenderer)
            {
                for (var i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (i == 0)
                    {
                        renderer.GetPropertyBlock(_materialPropertyBlock, i);
                        _materialPropertyBlock.SetColor(_colorId, data.CardOutlineMaterialColor);
                        renderer.SetPropertyBlock(_materialPropertyBlock, i);
                    }
                    else
                    {
                        renderer.GetPropertyBlock(_materialPropertyBlock, i);
                        _materialPropertyBlock.SetColor(_colorId, data.CardMaterialColor);
                        renderer.SetPropertyBlock(_materialPropertyBlock, i);
                    }
                }
            }


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
                    //Debug.Log($"attach c {Data.CardColor} slot name {renderer.name} card name {card.name}");
                    return (true, renderer.transform);
                }
            }

            return (false, null);
        }
    }
}
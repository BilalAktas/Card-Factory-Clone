using UnityEngine;

namespace Core
{
    public class ProductArea : MonoBehaviour
    {
        [SerializeField] private ProductBoxData _productBoxData;
        [SerializeField] private GameObject _productBoxPrefab;
        
        private void Start()
        {
            var clone = Instantiate(_productBoxPrefab, transform);
            clone.GetComponent<ProductBox>().SetAllProperty(_productBoxData);
        }
    }
}
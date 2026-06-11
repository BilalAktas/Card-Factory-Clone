using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        private GameState _gameState;
        [SerializeField] private GameObject _loseScreenUI;
        [SerializeField] private AudioSource _loseAudio;

        private void Start()
        {
            Application.targetFrameRate = 60;
            
            _gameState = GameState.Playing;
            EventBus.Subscribe<OnGameFailedEvent>(OnGameFailed);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnGameFailedEvent>(OnGameFailed);
        }

        private void OnGameFailed(OnGameFailedEvent data)
        {
            _gameState = GameState.Idle;
            _loseScreenUI.SetActive(true);
            _loseAudio.Play();
        }
    }
}
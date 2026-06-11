using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        private GameState _gameState;

        private void Start()
        {
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
        }
    }
}
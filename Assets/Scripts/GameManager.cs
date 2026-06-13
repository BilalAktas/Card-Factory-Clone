using UnityEngine;

namespace Core
{
    public class GameManager : Singleton<GameManager>
    {
        public GameState GameState { get; private set; }
        [SerializeField] private GameObject _loseScreenUI;
        [SerializeField] private AudioSource _loseAudio;
        
        [SerializeField] private GameObject _winScreenUI;
        [SerializeField] private AudioSource _winAudio;

        private void Start()
        {
            Application.targetFrameRate = 60;
            
            GameState = GameState.Playing;
            EventBus.Subscribe<OnGameFailedEvent>(OnGameFailed);
            EventBus.Subscribe<OnLevelCompletedEvent>(OnLevelCompleted);
            EventBus.Subscribe<OnLevelSpawnedEvent>(OnLevelSpawned);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnGameFailedEvent>(OnGameFailed);
            EventBus.Unsubscribe<OnLevelCompletedEvent>(OnLevelCompleted);
            EventBus.Unsubscribe<OnLevelSpawnedEvent>(OnLevelSpawned);
        }

        private void OnLevelSpawned(OnLevelSpawnedEvent data) => GameState = GameState.Playing;

        private void OnGameFailed(OnGameFailedEvent data)
        {
            GameState = GameState.Idle;
            _loseScreenUI.SetActive(true);
            _loseAudio.Play();
        }

        private void OnLevelCompleted(OnLevelCompletedEvent data)
        {
            GameState = GameState.Idle;
            _winScreenUI.SetActive(true);
            _winAudio.Play();
        }
    }
}
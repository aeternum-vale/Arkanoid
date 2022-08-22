using Cysharp.Threading.Tasks;
using Gamelogic.Extensions;
using NaughtyAttributes;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using ReadOnly = NaughtyAttributes.ReadOnlyAttribute;


public class GameController : MonoBehaviour
{
    private const string HighscoreKey = "HighscoreKey";

    private Camera _mainCamera;

    [SerializeField] [ReadOnly] private int _level;
    [SerializeField] [ReadOnly] private int _score;
    [SerializeField] [ReadOnly] private int _highscore;

    [Space]
    [SerializeField] private int _maxLivesNumber = 3;
    [SerializeField] [ReadOnly] private int _lives = 3;

    [Space]
    [SerializeField] private float _powerUpIntervalSec = 5f;
    private CancellationTokenSource _powerUpCTS;

    [Space]
    [SerializeField] private BoardController _boardController;
    [SerializeField] private UIController _uiController;
    [SerializeField] private Slider _slider;
    [SerializeField] private Ball _ball;

    private SessionSaver _sessionSaver = new SessionSaver();

    private void Awake()
    {
        AddListeners();
        _mainCamera = Camera.main;
        _highscore = PlayerPrefs.GetInt(HighscoreKey, 0);

    }

    private void AddListeners()
    {
        _ball.BottomHit += OnBottomHit;

        _boardController.AllBlocksDemolished += OnAllBlocksDemolished;
        _boardController.BallHitPowerUp += OnBallHitPowerUp;
        _boardController.BallDemolishedBlock += OnBallDemolishedBlock;

        _uiController.PauseMenuBackButtonClick += OnPauseMenuBackButtonClick;
        _uiController.PauseMenuSaveButtonClick += OnPauseMenuSaveButtonClick;
        _uiController.PauseMenuQuitButtonClick += OnPauseMenuQuitButtonClick;
    }

    private void RemoveListeners()
    {
        _ball.BottomHit -= OnBottomHit;

        _boardController.AllBlocksDemolished -= OnAllBlocksDemolished;
        _boardController.BallHitPowerUp -= OnBallHitPowerUp;
        _boardController.BallDemolishedBlock -= OnBallDemolishedBlock;

        _uiController.PauseMenuBackButtonClick -= OnPauseMenuBackButtonClick;
        _uiController.PauseMenuSaveButtonClick -= OnPauseMenuSaveButtonClick;
        _uiController.PauseMenuQuitButtonClick -= OnPauseMenuQuitButtonClick;

    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(Constants.ContinueModeKey) == 1)
        {
            RestoreSessionDataAndStartLevel();
            return;
        }
        
        _level = 1;
        PrepareBoardAndStartLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
            _uiController.ShowPauseMenu();
        }
    }


    private void OnBallDemolishedBlock(Block block)
    {
        _score += block.HitPointsMaxNumber;
        UpdateLevelStatsUI();
    }

    private async void OnBallHitPowerUp()
    {
        Array values = Enum.GetValues(typeof(EPowerUpType));
        EPowerUpType powerUpType = (EPowerUpType)values.GetValue(Random.Range(1, values.Length));

        Debug.Log($"powerUpType={powerUpType}");

        switch (powerUpType)
        {
            case EPowerUpType.AlmightyBall:
                _ball.IsAlmighty = true;
                break;
            case EPowerUpType.WiderSlider:
                _slider.Width *= 1.5f;
                break;

            default:
                throw new Exception("invalid power-up type");
        }

        _powerUpCTS = new CancellationTokenSource();
        await UniTask.Delay(TimeSpan.FromSeconds(_powerUpIntervalSec), cancellationToken: _powerUpCTS.Token);

        switch (powerUpType)
        {
            case EPowerUpType.AlmightyBall:
                _ball.IsAlmighty = false;
                break;
            case EPowerUpType.WiderSlider:
                _slider.Width /= 1.5f;
                break;

            default:
                throw new Exception("invalid power-up type");
        }
    }

    private void PrepareBoardAndStartLevel()
    {
        UpdateLevelStatsUI();
        _boardController.PrepareNewBoard(_level);
        _ball.IsMoving = true;
    }


    private void OnBottomHit()
    {
        _lives--;
        UpdateLevelStatsUI();

        if (_lives <= 0)
            FinishGame();
    }


    private void OnAllBlocksDemolished()
    {
        MoveToNextLevel();
    }

    private void OnPauseMenuQuitButtonClick()
    {
        ResumeGame();
        SceneManager.LoadScene(Constants.MenuSceneIndex);
    }

    private void OnPauseMenuSaveButtonClick()
    {
        SessionData sd = new SessionData()
        {
            Level = _level,
            DemolishedBlockGridIndexes = _boardController.DemolishedBlockGridIndexes,
            Score = _score,
            BallPosition = new SimpleVector2D(_ball.transform.position.x, _ball.transform.position.y),
            BallDirection = new SimpleVector2D(_ball.Direction.x, _ball.Direction.y),
            SliderXPosition = _slider.transform.position.x
        };

        _sessionSaver.SaveSessionData(sd);

        _uiController.ShowSessionSavedMessage();
    }

    [Button]
    private void RestoreSessionDataAndStartLevel()
    {
        if (_sessionSaver.TryRestoreSessionData(out SessionData sessionData))
        {
            RestoreInititalState();

            _boardController.RestoreSession(sessionData.Level, sessionData.DemolishedBlockGridIndexes);

            _ball.transform.position = new Vector3(sessionData.BallPosition.X, sessionData.BallPosition.Y, _ball.transform.position.y);
            _ball.Direction = new Vector3(sessionData.BallDirection.X, sessionData.BallDirection.Y, 0f);

            _slider.transform.position = _slider.transform.position.WithX(sessionData.SliderXPosition);

            _score = sessionData.Score;
            _lives = sessionData.Lives;
            _level = sessionData.Level;

            UpdateLevelStatsUI();

            _ball.IsMoving = true;
        }
    }

    private void OnPauseMenuBackButtonClick()
    {
        _uiController.HidePauseMenu();
        ResumeGame();
    }


    private void FinishGame()
    {

    }

    [Button]
    private void MoveToNextLevel()
    {
        _level++;

        RestoreInititalState();
        PrepareBoardAndStartLevel();
    }

    private void RestoreInititalState()
    {
        _powerUpCTS?.Cancel();

        _ball.IsMoving = false;
        _ball.RestoreInititalState();

        _slider.RestoreInititalState();
    }

    private void UpdateLevelStatsUI()
    {
        _uiController.Level = _level;
        _uiController.Lives = _lives;
        _uiController.Score = _score;
        _uiController.Highscore = _highscore;
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
    }

    private void OnDestroy() => RemoveListeners();
}
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gamelogic.Extensions;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReadOnly = NaughtyAttributes.ReadOnlyAttribute;

public class GameController : MonoBehaviour
{
    private const string HighscoreKey = "HighscoreKey";

    [SerializeField] [ReadOnly] private int _level;
    [SerializeField] [ReadOnly] private int _score;
    [SerializeField] [ReadOnly] private int _highscore;

    [Space]
    [SerializeField] private int _maxLivesNumber = 3;
    [SerializeField] [ReadOnly] private int _lives = 3;

    [Space]
    [SerializeField] private BoardController _boardController;
    [SerializeField] private UIController _uiController;
    [SerializeField] private PowerUpController _powerUpController;
    [SerializeField] private Slider _slider;
    [SerializeField] private Ball _ball;
    [SerializeField] private SpriteRenderer _bottomHitIndicator;
    [Space]
    [SerializeField] private float _bottomHitIndicationDuration = 0.5f;


    private SessionSaver _sessionSaver = new SessionSaver();

    private Sequence _bottomHitIndicationSequence;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        _highscore = PlayerPrefs.GetInt(HighscoreKey, 0);
        AddListeners();
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
            RestoreSession();
        else
        {
            _level = 1;
            _lives = _maxLivesNumber;
            UpdateLevelStatsUI();
            RestoreInititalState();
            PrepareBoard();
        }

        PauseBeforeButtonClick();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_uiController.IsPressButtonToStartMessageShown &&
                !_uiController.IsGameOverMessageShown)
            {
                PauseGame();
                _uiController.ShowPauseMenu();
            }
        }
    }

    private void OnBallDemolishedBlock(Block block)
    {
        _score += block.HitPointsMaxNumber;
        UpdateLevelStatsUI();
    }

    private void OnBallHitPowerUp()
    {
        _powerUpController.PowerUp();
    }

    private void PrepareBoard()
    {
        _boardController.PrepareNewBoard(_level);
    }

    private void OnBottomHit()
    {
        IndicateBottomHit();

        _lives--;
        UpdateLevelStatsUI();

        if (_lives <= 0)
            FinishGame();
    }

    private void IndicateBottomHit()
    {
        _bottomHitIndicationSequence = DOTween.Sequence();
        _bottomHitIndicationSequence.Append(_bottomHitIndicator.DOFade(1f, _bottomHitIndicationDuration / 2f).SetEase(Ease.InQuad));
        _bottomHitIndicationSequence.Append(_bottomHitIndicator.DOFade(0f, _bottomHitIndicationDuration / 2f).SetEase(Ease.OutQuad));
    }

    private void OnAllBlocksDemolished() => GoToNextLevel();

    private void OnPauseMenuQuitButtonClick() => GoToMenuScene();

    private void GoToMenuScene()
    {
        ResumeGame();
        SceneManager.LoadScene(Constants.MenuSceneIndex);
    }

    private void OnPauseMenuSaveButtonClick()
    {
        SessionData sd = new SessionData()
        {
            Level = _level,
            Lives = _lives,
            Score = _score,
            DemolishedBlockGridIndexes = _boardController.DemolishedBlockGridIndexes,
            BallPosition = new SimpleVector2D(_ball.transform.position.x, _ball.transform.position.y),
            BallDirection = new SimpleVector2D(_ball.Direction.x, _ball.Direction.y),
            SliderXPosition = _slider.transform.position.x
        };

        _sessionSaver.SaveSessionData(sd);

        _uiController.ShowSessionSavedMessage();
    }

    [Button]
    private void RestoreSession()
    {
        if (_sessionSaver.TryRestoreSessionData(out SessionData sessionData))
        {
            RestoreInititalState();

            _boardController.RestoreSession(sessionData.Level, sessionData.DemolishedBlockGridIndexes);

            _ball.transform.position = new Vector3(sessionData.BallPosition.X, sessionData.BallPosition.Y, _ball.transform.position.y);
            _ball.ClearTrail();
            _ball.Direction = new Vector3(sessionData.BallDirection.X, sessionData.BallDirection.Y, 0f);

            _slider.transform.position = _slider.transform.position.WithX(sessionData.SliderXPosition);

            _score = sessionData.Score;
            _lives = sessionData.Lives;
            _level = sessionData.Level;

            UpdateLevelStatsUI();
        }
    }

    private void OnPauseMenuBackButtonClick()
    {
        _uiController.HidePauseMenu();
        ResumeGame();
    }

    private async void FinishGame()
    {
        PauseGame();

        if (_score > _highscore)
        {
            _highscore = _score;
            PlayerPrefs.SetInt(HighscoreKey, _highscore);
        }

        _uiController.ShowGameOverMessage(_score, _highscore);

        await UniTask.Delay(TimeSpan.FromSeconds(2f), DelayType.Realtime);
        await UniTask.WaitUntil(() => Input.anyKey);

        GoToMenuScene();
    }

    [Button]
    private void GoToNextLevel()
    {
        _level++;

        _ball.IsMoving = false;
        PauseBeforeButtonClick();
        PrepareBoard();
        RestoreInititalState();
    }

    private void RestoreInititalState()
    {
        _bottomHitIndicationSequence?.Kill();
        _powerUpController.RestoreInitialState();
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

    private async void PauseBeforeButtonClick()
    {
        PauseGame();
        _uiController.IsPressButtonToStartMessageShown = true;

        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        _uiController.IsPressButtonToStartMessageShown = false;
        _ball.IsMoving = true;
        ResumeGame();
    }

    private void PauseGame() => Time.timeScale = 0;

    private void ResumeGame() => Time.timeScale = 1;

    private void OnDestroy() => RemoveListeners();
}
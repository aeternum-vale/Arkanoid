using Cysharp.Threading.Tasks;
using Gamelogic.Extensions;
using NaughtyAttributes;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
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
    [SerializeField] private Slider _slider;
    [SerializeField] private BoardController _boardController;
    [SerializeField] private Ball _ball;

    [Header("UI")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _livesText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _highscoreText;

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
    }


    private void RemoveListeners()
    {
        _ball.BottomHit -= OnBottomHit;
        _boardController.AllBlocksDemolished -= OnAllBlocksDemolished;
        _boardController.BallHitPowerUp -= OnBallHitPowerUp;
        _boardController.BallDemolishedBlock -= OnBallDemolishedBlock;
    }

    private void Start()
    {
        _level = 1;
        PrepareBoardAndStartLevel();
    }
    private void OnBallDemolishedBlock(Block block)
    {
        _score += block.HitPointsMaxNumber;
        UpdateLevelUI();
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
        UpdateLevelUI();
        _boardController.PrepareNewBoard(_level);
        _ball.IsMoving = true;
    }


    private void OnBottomHit()
    {
        _lives--;
        if (_lives <= 0)
            FinishGame();
    }


    private void OnAllBlocksDemolished()
    {
        MoveToNextLevel();
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

    private void UpdateLevelUI()
    {
        _levelText.text = $"Level: {_level}";
        _livesText.text = $"Lives: {_lives}";
        _scoreText.text = $"Score: {_score}";
        _highscoreText.text = $"Highscore: {_highscore}";

    }

    private void OnDestroy() => RemoveListeners();
}
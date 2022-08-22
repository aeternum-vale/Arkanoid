using Cysharp.Threading.Tasks;
using Gamelogic.Extensions;
using NaughtyAttributes;
using System;
using System.Threading;
using UnityEngine;
using ReadOnly = NaughtyAttributes.ReadOnlyAttribute;

public class GameController : MonoBehaviour
{
    private Camera _mainCamera;

    [SerializeField] [ReadOnly] private int _currentLevel;

    [Space]
    [SerializeField] private int _maxLivesNumber = 3;
    [SerializeField] [ReadOnly] private int _livesNumber = 3;

    [Space]
    [SerializeField] private float _powerUpIntervalSec = 5f;
    private CancellationTokenSource _powerUpCTS;

    [Space]
    [SerializeField] private Slider _slider;
    [SerializeField] private BoardController _boardController;
    [SerializeField] private Ball _ball;



    private void Awake()
    {
        AddListeners();
        _mainCamera = Camera.main;
    }

    private void AddListeners()
    {
        _ball.BottomHit += OnBottomHit;
        _boardController.AllBlocksDemolished += OnAllBlocksDemolished;
        _boardController.BallHitPowerUp += OnBallHitPowerUp;
    }


    private void RemoveListeners()
    {
        _ball.BottomHit -= OnBottomHit;
        _boardController.AllBlocksDemolished -= OnAllBlocksDemolished;
        _boardController.BallHitPowerUp -= OnBallHitPowerUp;
    }

    private void Start()
    {
        _currentLevel = 1;
        PrepareBoardAndStartLevel();
    }

    private async void OnBallHitPowerUp(EPowerUpType powerUpType)
    {
        Debug.Log($"PowerUp: {powerUpType}");


        switch (powerUpType)
        {
            case EPowerUpType.AlmightyBall:
                _ball.IsAlmighty = true;
                break;
            case EPowerUpType.WiderSlider:
                _slider.Width *= 1.5f;
                break;
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
        }
    }

    private void PrepareBoardAndStartLevel()
    {
        _boardController.PrepareBoard(_currentLevel);
        _ball.IsMoving = true;
    }


    private void OnBottomHit()
    {
        _livesNumber--;
        if (_livesNumber <= 0)
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
        _currentLevel++;

        RestoreInititalState();
        PrepareBoardAndStartLevel();
    }

    private void RestoreInititalState()
    {
        _powerUpCTS.Cancel();

        _ball.IsMoving = false;
        _ball.RestoreInititalState();

        _slider.RestoreInititalState();
    }


    private void OnDestroy() => RemoveListeners();
}
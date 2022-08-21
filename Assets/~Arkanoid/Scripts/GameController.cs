using Gamelogic.Extensions;
using NaughtyAttributes;
using UnityEngine;
using ReadOnly = NaughtyAttributes.ReadOnlyAttribute;

public class GameController : MonoBehaviour
{
    private Camera _mainCamera;

    [SerializeField] [ReadOnly] private int _currentLevel;

    [SerializeField] private int _maxLivesNumber = 3;
    [SerializeField] [ReadOnly] private int _livesNumber = 3;


    [Header("Slider")]
    [SerializeField] private Transform _slider;
    [SerializeField] [ReadOnly] private float _sliderSpeed = 1f;
    [SerializeField] private float _sliderInterpolationValue = 0.5f;

    private float _sliderTargetX = 0;
    private float _sliderLeftLimit;
    private float _sliderRightLimit;
    private SpriteRenderer _sliderSpriteRenderer;

    [Header("Board")]
    [SerializeField] private BoardController _boardController;

    [Header("Ball")]
    [SerializeField] private Ball _ball;
    private Vector3 _ballInititalPosition;
    private Vector3 _ballInitialDirVector;


    private void Awake()
    {
        AddListeners();

        _mainCamera = Camera.main;
        _sliderSpriteRenderer = _slider.GetComponent<SpriteRenderer>();

        _sliderLeftLimit =
            _mainCamera.ScreenToWorldPoint(Vector2.zero).x + _sliderSpriteRenderer.size.x / 2;
        _sliderRightLimit =
            _mainCamera.ScreenToWorldPoint(new Vector2(_mainCamera.pixelWidth, 0f)).x - _sliderSpriteRenderer.size.x / 2;

        _ballInititalPosition = _ball.transform.position;
        _ballInitialDirVector = _ball.DirectionVector;
    }


    private void Start()
    {
        _currentLevel = 1;
        PrepareAndStartLevel();
    }

    private void PrepareAndStartLevel()
    {
        _boardController.PrepareBoard(_currentLevel);
        _ball.IsMoving = true;
    }

    private void AddListeners()
    {
        _ball.BottomHit += OnBottomHit;
        _boardController.AllBlocksDemolished += OnAllBlocksDemolished;
    }

    private void RemoveListeners()
    {
        _ball.BottomHit -= OnBottomHit;
        _boardController.AllBlocksDemolished -= OnAllBlocksDemolished;
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


    private void FixedUpdate()
    {
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        _sliderTargetX += Input.GetAxis("Horizontal") * _sliderSpeed;
        float newX = Mathf.Lerp(_slider.position.x, _sliderTargetX, _sliderInterpolationValue);

        if (newX <= _sliderLeftLimit)
        {
            newX = _sliderLeftLimit;
            _sliderTargetX = _sliderLeftLimit;
        }

        if (newX >= _sliderRightLimit)
        {
            newX = _sliderRightLimit;
            _sliderTargetX = _sliderRightLimit;
        }

        _slider.position = _slider.position.WithX(newX);
    }

    private void FinishGame()
    {

    }

    [Button]
    private void MoveToNextLevel()
    {
        _currentLevel++;

        _ball.IsMoving = false;
        _ball.transform.position = _ballInititalPosition;
        _ball.DirectionVector = _ballInitialDirVector;

        _sliderTargetX = 0;
        _slider.position = _slider.position.WithX(0);

        PrepareAndStartLevel();
    }

    private void OnDestroy() => RemoveListeners();
}
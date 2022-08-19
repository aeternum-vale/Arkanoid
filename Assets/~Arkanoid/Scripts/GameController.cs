using Gamelogic.Extensions;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private Camera _mainCamera;

    [Header("Slider")]
    [SerializeField] private Transform _slider;

    [SerializeField] private float _sliderSpeed = 1f;
    [SerializeField] private float _sliderInterpolationValue = 0.5f;
    private float _targetX = 0;
    private float _sliderLeftLimit;
    private float _sliderRightLimit;
    private SpriteRenderer _sliderSpriteRenderer;


    private void Awake()
    {
        _mainCamera = Camera.main;
        _sliderSpriteRenderer = _slider.GetComponent<SpriteRenderer>();

        _sliderLeftLimit =
            _mainCamera.ScreenToWorldPoint(Vector2.zero).x + _sliderSpriteRenderer.size.x / 2;
        _sliderRightLimit =
            _mainCamera.ScreenToWorldPoint(new Vector2(_mainCamera.pixelWidth, 0f)).x - _sliderSpriteRenderer.size.x / 2;
    }

    private void FixedUpdate()
    {
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        _targetX += Input.GetAxis("Horizontal") * _sliderSpeed;
        float newX = Mathf.Lerp(_slider.position.x, _targetX, _sliderInterpolationValue);

        if (newX <= _sliderLeftLimit)
        {
            newX = _sliderLeftLimit;
            _targetX = _sliderLeftLimit;
        }

        if (newX >= _sliderRightLimit)
        {
            newX = _sliderRightLimit;
            _targetX = _sliderRightLimit;
        }

        _slider.position = _slider.position.WithX(newX);
    }



}
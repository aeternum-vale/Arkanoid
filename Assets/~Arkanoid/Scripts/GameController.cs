using Gamelogic.Extensions;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Transform _slider;

    [SerializeField] private float _sliderSpeed = 1f;
    [SerializeField] private float _sliderInterpolationValue = 0.5f;
    private float _targetX = 0;
    private float _leftLimit;
    private float _rightLimit;
    private SpriteRenderer _sliderSpriteRenderer;

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _sliderSpriteRenderer = _slider.GetComponent<SpriteRenderer>();


        _leftLimit = _mainCamera.ScreenToWorldPoint(Vector2.zero).x + _sliderSpriteRenderer.size.x / 2;
        _rightLimit = _mainCamera.ScreenToWorldPoint(new Vector2(_mainCamera.pixelWidth, 0f)).x - _sliderSpriteRenderer.size.x / 2;
    }

    private void FixedUpdate()
    {
        Debug.Log(Input.GetAxis("Horizontal"));
        _targetX += Input.GetAxis("Horizontal") * _sliderSpeed;
        float newX = Mathf.Lerp(_slider.position.x, _targetX, _sliderInterpolationValue);

        if (newX <= _leftLimit)
        {
            newX = _leftLimit;
            _targetX = _leftLimit;
        }

        if (newX >= _rightLimit)
        {
            newX = _rightLimit;
            _targetX = _rightLimit;
        }

        _slider.position = _slider.position.WithX(newX);
    }
}
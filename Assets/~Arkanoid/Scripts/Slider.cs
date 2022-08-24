using Gamelogic.Extensions;
using UnityEngine;

public class Slider : MonoBehaviour
{
    private Camera _mainCamera;

    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _moveInterpolation = 0.5f;

    private float _targetX = 0;
    private float _leftLimit;
    private float _rightLimit;
    private SpriteRenderer _spriteRenderer;
    private float _initialWidth;
    private float _boostedWidth;

    public float Width
    {
        get => _spriteRenderer.size.x;
        set
        {
            _spriteRenderer.size =
                new Vector2(value, _spriteRenderer.size.y);
            CalculateSliderLimits();
        }
    }

    public float InitialWidth => _initialWidth;
    public float BoostedWidth => _boostedWidth;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialWidth = Width;
        _boostedWidth = _mainCamera.orthographicSize * 2f * _mainCamera.aspect;
        CalculateSliderLimits();
    }

    private void CalculateSliderLimits()
    {
        _leftLimit =
            _mainCamera.ScreenToWorldPoint(Vector2.zero).x + _spriteRenderer.size.x / 2;
        _rightLimit =
            _mainCamera.ScreenToWorldPoint(new Vector2(_mainCamera.pixelWidth, 0f)).x - _spriteRenderer.size.x / 2;
    }

    private void FixedUpdate()
    {
        _targetX += Input.GetAxis("Horizontal") * _speed;
        float newX = Mathf.Lerp(transform.position.x, _targetX, _moveInterpolation);

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

        transform.position = transform.position.WithX(newX);
    }

    public void RestoreInititalState()
    {
        _targetX = 0;
        transform.position = transform.position.WithX(0);

        Width = _initialWidth;
    }
}
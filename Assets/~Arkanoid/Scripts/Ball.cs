using NaughtyAttributes;
using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public event Action<GameObject> BlockHit;
    public event Action BottomHit;

    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Vector3 _direction = new Vector3(1f, 1f, 0f);
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _sliderRedirectionAngle = 80f;
    [ReadOnly] [SerializeField] private GameObject _lastCollision;

    private Vector2 _collisionNormal;
    private bool _hasCollision = false;
    private bool _hasCollisionWithSlider = false;

    public bool IsMoving = false;
    public bool IsAlmighty = false;


    private Vector3 _inititalPosition;
    private Vector3 _initialDirection;


    private void Awake()
    {
        _inititalPosition = transform.position;
        _initialDirection = _direction;

    }

    public void RestoreInititalState()
    {
        transform.position = _inititalPosition;
        _direction = _initialDirection;

        _hasCollision = false;
        _hasCollisionWithSlider = false;
        _lastCollision = null;

        IsMoving = false;
        IsAlmighty = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
            Debug.DrawRay(contact.point, contact.normal, Color.blue, 2f);

        if (collision.gameObject.Equals(_lastCollision)) return;

        var normal = collision.GetContact(0).normal;

        if (collision.gameObject.layer.Equals(Constants.SliderLayer))
            HandleCollisionWithSlider(collision);
        else
            HandleCollisionWithNotSlider(normal);

        _hasCollision = true;
        _lastCollision = collision.gameObject;
    }
    private void HandleCollisionWithSlider(Collision2D collision)
    {
        _hasCollisionWithSlider = true;

        var px = collision.GetContact(0).point.x;
        var sb = collision.collider.bounds;
        var sliderX = Mathf.Abs(px - (sb.center.x - sb.extents.x));
        float sliderRelativeX = sliderX / sb.size.x;
        _direction =
            Quaternion.Euler(0, 0, _sliderRedirectionAngle * (.5f - sliderRelativeX)) * Vector2.up;
        _direction = _direction.normalized;
    }

    private void HandleCollisionWithNotSlider(Vector2 normal)
    {
        if (IsAlmighty && _lastCollision.layer.Equals(Constants.BlocksLayer))
            BlockHit?.Invoke(_lastCollision);

        if (!_hasCollision)
            _collisionNormal = normal;
        else if (GetVectorDirectness(normal) > GetVectorDirectness(_collisionNormal))
            _collisionNormal = normal;
    }

    private void FixedUpdate()
    {
        bool isBlock = false;
        if (!_hasCollisionWithSlider && _hasCollision)
        {
            if (_lastCollision.layer.Equals(Constants.BlocksLayer))
                isBlock = true;

            if (_lastCollision.layer.Equals(Constants.BottomLayer))
                BottomHit?.Invoke();

            if (!isBlock || !IsAlmighty)
                _direction = Vector3.Reflect(_direction, _collisionNormal).normalized;
        }

        _hasCollision = false;
        _hasCollisionWithSlider = false;

        Move();

        if (isBlock) BlockHit?.Invoke(_lastCollision);
    }

    private void Move()
    {
        if (!IsMoving) return;

        Vector3 nextPosition = transform.position + _direction * _speed;
        _rigidBody.MovePosition(nextPosition);
    }

    //private bool IsInsideTheScreen(Vector3 point) =>
    //    new Rect(0, 0, 1, 1).Contains(_mainCamera.WorldToViewportPoint(point));

    private static float GetVectorDirectness(Vector2 vector)
    {
        float dotAbs = Mathf.Abs(Vector2.Dot(vector, Vector2.up));
        return 1f - Mathf.Min(dotAbs, 1f - dotAbs);
    }


}
using NaughtyAttributes;
using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public event Action<(GameObject, Vector2)> BlockHit;

    public event Action BottomHit;

    [SerializeField] private Transform _initialPosition;
    [SerializeField] private Vector3 _direction = new Vector3(1f, 1f, 0f);
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _sliderRedirectionAngle = 80f;
    [SerializeField] private float _offsetOnHit = 0.01f;

    [Space]
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private ParticleSystem _almightyParticleSystem;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TrailRenderer _trailRenderer;

    [Space]
    [ReadOnly] [SerializeField] private GameObject _lastCollision1;

    [ReadOnly] [SerializeField] private GameObject _lastCollision2;

    [Space]
    [SerializeField] private Color _normal;

    [SerializeField] private Color _trailNormal;
    [SerializeField] private Color _almighty;

    private bool _isMoving = false;
    private bool _isAlmighty = false;
    private ContactFilter2D _contactFilter2D;
    private float _radius;
    private float _initialSpeed;
    private Vector3 _nextPosition;

    public bool IsBoosted { get; set; }
    public Vector3 Direction { get => _direction; set => _direction = value; }

    public bool IsAlmighty
    {
        get => _isAlmighty;
        set
        {
            if (value)
            {
                _almightyParticleSystem.gameObject.SetActive(true);
                _almightyParticleSystem.Play();
                _trailRenderer.startColor = _almighty;
            }
            else
            {
                _almightyParticleSystem.Stop();
                _trailRenderer.startColor = _trailNormal;
            }

            _isAlmighty = value;
        }
    }

    public bool IsMoving { get => _isMoving; set => _isMoving = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public float InitialSpeed => _initialSpeed;

    private void Awake()
    {
        _initialSpeed = _speed;
        _radius = _spriteRenderer.size.x / 2f;

        int layerMask = 1 << Constants.BlocksLayer;
        layerMask |= 1 << Constants.SliderLayer;
        layerMask |= 1 << Constants.WallsLayer;
        layerMask |= 1 << Constants.BottomLayer;
        _contactFilter2D = new ContactFilter2D()
        {
            layerMask = layerMask,
            useLayerMask = true
        };
    }

    public void RestoreInititalState()
    {
        transform.position = _initialPosition.position;
        _direction = _initialPosition.up;
        _speed = _initialSpeed;

        _lastCollision1 = null;
        _lastCollision2 = null;

        IsAlmighty = false;
        IsBoosted = false;

        _almightyParticleSystem.Clear();
        _almightyParticleSystem.Stop();

        ClearTrail();
    }

    public void ClearTrail()
    {
        _trailRenderer.Clear();
    }

    private void ChangeDirectionAccordingToSlider(RaycastHit2D hit)
    {
        float px = hit.point.x;
        Bounds sb = hit.collider.bounds;
        var sliderX = Mathf.Abs(px - (sb.center.x - sb.extents.x));
        float sliderRelativeX = sliderX / sb.size.x;
        _direction =
            Quaternion.Euler(0, 0, _sliderRedirectionAngle * (.5f - sliderRelativeX)) * Vector2.up;
        _direction = _direction.normalized;
    }

    private bool IsOriginalCollider(RaycastHit2D hit)
    {
        return !hit.collider.gameObject.Equals(_lastCollision1) && !hit.collider.gameObject.Equals(_lastCollision2);
    }

    private bool IsSliderHit(RaycastHit2D hit1, RaycastHit2D hit2, int hitCount, out RaycastHit2D hit)
    {
        return IsLayerHit(Constants.SliderLayer, hit1, hit2, hitCount, out hit);
    }

    private bool IsBottomHit(RaycastHit2D hit1, RaycastHit2D hit2, int hitCount, out RaycastHit2D hit)
    {
        return IsLayerHit(Constants.BottomLayer, hit1, hit2, hitCount, out hit);
    }

    private bool IsBlockHit(RaycastHit2D hit1, RaycastHit2D hit2, int hitCount, out RaycastHit2D hit)
    {
        return IsLayerHit(Constants.BlocksLayer, hit1, hit2, hitCount, out hit);
    }

    private bool IsBlockHit(RaycastHit2D hit)
    {
        return hit.collider != null && hit.collider.gameObject.layer.Equals(Constants.BlocksLayer);
    }

    private bool IsLayerHit(int layer, RaycastHit2D hit1, RaycastHit2D hit2, int hitCount, out RaycastHit2D hit)
    {
        hit = new RaycastHit2D();
        if (hitCount == 0) return false;

        if (hit1.collider.gameObject.layer.Equals(layer))
        {
            hit = hit1;
            return true;
        }

        if (hitCount == 1) return false;

        if (hit2.collider.gameObject.layer.Equals(layer))
        {
            hit = hit2;
            return true;
        }

        return false;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        var initialDirection = _direction;
        float distance = _speed * Time.deltaTime;
        _nextPosition = transform.position + _direction * distance;

        if (IsOutsideOfTheScreen())
        {
            HandleScreenLeaving(distance);
            return;
        }

        RaycastHit2D[] raycastResults = new RaycastHit2D[2];
        int hitCount = Physics2D.CircleCast(transform.position, _radius, _direction, _contactFilter2D, raycastResults, distance);

        if (hitCount == 0) return;

        var hit1 = raycastResults[0];
        var hit2 = raycastResults[1];

        RefineHitCounts(ref hitCount, ref hit1, hit2);

        if (hitCount == 0) return;

        EvaluateDirectionAndNextPosition(hitCount, ref hit1, ref hit2);

        if (IsSliderHit(hit1, hit2, hitCount, out RaycastHit2D sliderHit))
            ChangeDirectionAccordingToSlider(sliderHit);

        _nextPosition += _direction * _offsetOnHit;

        HandleBlockHit(initialDirection, distance, hitCount, ref hit1, ref hit2);

        if (!IsBoosted && IsBottomHit(hit1, hit2, hitCount, out _))
            BottomHit?.Invoke();
    }

    private void HandleScreenLeaving(float distance)
    {
        _direction = (Vector3.zero - transform.position).normalized;
        _nextPosition = transform.position + _direction * distance;
        MoveTo(_nextPosition);
        Debug.Log("returned!");
    }

    private void EvaluateDirectionAndNextPosition(int hitCount, ref RaycastHit2D hit1, ref RaycastHit2D hit2)
    {
        if (hitCount == 1)
        {
            _direction = Vector3.Reflect(_direction, hit1.normal.normalized).normalized;
            _nextPosition = hit1.centroid;

            _lastCollision1 = hit1.collider.gameObject;
            _lastCollision2 = null;
        }
        else
        if (hitCount == 2)
        {
            if (Vector2.Dot(hit1.normal, hit2.normal) > 0.1f)
                _direction = Vector3.Reflect(_direction, GetMoreDirectnessVector(hit1.normal, hit2.normal)).normalized;
            else
            {
                Vector2 avgNormal = (hit1.normal + hit2.normal / 2f).normalized;
                _direction = Vector3.Reflect(_direction, avgNormal).normalized;
            }

            _nextPosition = (hit1.centroid + hit2.centroid) / 2f;

            _lastCollision1 = hit1.collider.gameObject;
            _lastCollision2 = hit2.collider.gameObject;
        }
    }

    private void HandleBlockHit(Vector3 initialDirection, float distance, int hitCount, ref RaycastHit2D hit1, ref RaycastHit2D hit2)
    {
        if (IsBlockHit(hit1, hit2, hitCount, out RaycastHit2D hit))
        {
            if (IsAlmighty)
            {
                if (IsBlockHit(hit1))
                    BlockHit?.Invoke((hit1.collider.gameObject, initialDirection));
                if (IsBlockHit(hit2))
                    BlockHit?.Invoke((hit2.collider.gameObject, initialDirection));

                _direction = initialDirection;
                _nextPosition = transform.position + _direction * distance;
            }
            else
                BlockHit?.Invoke((hit.collider.gameObject, initialDirection));
        }
    }

    private void LateUpdate()
    {
        MoveTo(_nextPosition);
    }

    private void RefineHitCounts(ref int hitCount, ref RaycastHit2D hit1, RaycastHit2D hit2)
    {
        if (hitCount == 1)
        {
            if (!IsOriginalCollider(hit1))
                hitCount = 0;
        }

        if (hitCount == 2)
        {
            bool isHit1Original = IsOriginalCollider(hit1);
            bool isHit2Original = IsOriginalCollider(hit2);

            if (!isHit1Original && !isHit2Original)
                hitCount = 0;

            if (isHit1Original && !isHit2Original)
                hitCount = 1;

            if (!isHit1Original && isHit2Original)
            {
                hitCount = 1;
                hit1 = hit2;
            }
        }
    }

    private bool IsOutsideOfTheScreen() =>
        !new Rect(0, 0, 1, 1).Contains(_mainCamera.WorldToViewportPoint(transform.position));

    private void MoveTo(Vector3 nextPosition)
    {
        if (!IsMoving) return;

        Debug.DrawLine(transform.position, nextPosition, Color.red, 0.3f);
        transform.position = nextPosition;
    }

    private static float GetVectorDirectness(Vector2 vector)
    {
        float dotAbs = Mathf.Abs(Vector2.Dot(vector, Vector2.up));
        return 1f - Mathf.Min(dotAbs, 1f - dotAbs);
    }

    private static Vector2 GetMoreDirectnessVector(Vector2 a, Vector2 b)
    {
        return GetVectorDirectness(a) > GetVectorDirectness(b) ? a : b;
    }
}
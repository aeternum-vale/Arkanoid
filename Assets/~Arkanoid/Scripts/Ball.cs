using NaughtyAttributes;
using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public event Action<GameObject> BlockHit;
    public event Action BottomHit;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [Space]
    [SerializeField] private Vector3 _direction = new Vector3(1f, 1f, 0f);
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _sliderRedirectionAngle = 80f;
    [SerializeField] private float _offsetOnHit = 0.01f;
    [Space]
    [ReadOnly] [SerializeField] private GameObject _lastCollision1;
    [ReadOnly] [SerializeField] private GameObject _lastCollision2;

    public bool IsMoving = true;
    public bool IsAlmighty = false;
    public Vector3 Direction { get => _direction; set => _direction = value; }

    private ContactFilter2D _contactFilter2D;
    private Vector3 _inititalPosition;
    private Vector3 _initialDirection;
    private float _radius;

    private const float _deltaTimeCorrection = 1f / 60f;

    private void Awake()
    {
        _inititalPosition = transform.position;
        _initialDirection = _direction;

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
        transform.position = _inititalPosition;
        _direction = _initialDirection;

        _lastCollision1 = null;
        _lastCollision2 = null;

        IsAlmighty = false;
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

        float distance = _speed * (Time.deltaTime / _deltaTimeCorrection);
        Vector3 nextPosition = transform.position + _direction * distance;

        RaycastHit2D[] raycastResults = new RaycastHit2D[2];
        int hitCount = Physics2D.CircleCast(transform.position, _radius, _direction, _contactFilter2D, raycastResults, distance);
        RaycastHit2D hit1 = raycastResults[0];
        RaycastHit2D hit2 = raycastResults[1];

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

        if (hitCount > 0)
        {

            if (hitCount == 1)
            {
                _direction = Vector3.Reflect(_direction, hit1.normal.normalized).normalized;
                nextPosition = hit1.centroid + (Vector2)_direction * _offsetOnHit;

                _lastCollision1 = hit1.collider.gameObject;
                _lastCollision2 = null;
            }

            if (hitCount == 2)
            {

                if (Vector2.Dot(hit1.normal, hit2.normal) > 0.1f)
                {
                    _direction = Vector3.Reflect(_direction, GetMoreDirectnessVector(hit1.normal, hit2.normal)).normalized;
                }
                else
                {
                    Vector2 avgNormal = (hit1.normal + hit2.normal / 2f).normalized;
                    _direction = Vector3.Reflect(_direction, avgNormal).normalized;
                }

                nextPosition = (hit1.centroid + hit2.centroid) / 2f + (Vector2)_direction * _offsetOnHit;

                _lastCollision1 = hit1.collider.gameObject;
                _lastCollision2 = hit2.collider.gameObject;
            }


            if (IsSliderHit(hit1, hit2, hitCount, out RaycastHit2D sliderHit))
                ChangeDirectionAccordingToSlider(sliderHit);
        }

        MoveTo(nextPosition);
    }


    private void MoveTo(Vector3 nextPosition)
    {
        if (!IsMoving) return;

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
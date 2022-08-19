using NaughtyAttributes;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private Vector3 _moveVector = new Vector3(0.1f, -0.1f, 0f);
    [SerializeField] private Rigidbody2D _rigidBody;

    [ReadOnly] [SerializeField] private GameObject _lastCollision;

    private Camera _mainCamera;
    private Vector2 _collisionNormal;
    private bool _hasCollision = false;

    private const int SliderLayer = 6;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
            Debug.DrawRay(contact.point, contact.normal, Color.blue, 2f);

        if (collision.gameObject.Equals(_lastCollision)) return;
        var normal = collision.GetContact(0).normal;

        if (collision.gameObject.layer.Equals(6))
        {
            if (Vector2.Dot(normal, Vector2.up) >= 0.5)
            {
                var px = collision.GetContact(0).point.x;
                var sb = collision.collider.bounds;
                var sliderX = Mathf.Abs(px - (sb.center.x - sb.extents.x));
                float sliderRelativeX = sliderX / sb.size.x;
                _collisionNormal = Quaternion.Euler(0, 0, 30f * (0.5f - sliderRelativeX)) * Vector2.up;
            }
        }
        else
        {
            if (!_hasCollision)
                _collisionNormal = normal;
            else
            {
                if (GetVectorDirectness(normal) > GetVectorDirectness(_collisionNormal))
                    _collisionNormal = normal;
            }
        }


        _hasCollision = true;
        _lastCollision = collision.gameObject;
    }

    private void Update()
    {
        if (_hasCollision)
        {
            _moveVector = Vector3.Reflect(_moveVector, _collisionNormal);
            Debug.DrawRay(transform.position, _collisionNormal, Color.red, 2f);
        }

        _hasCollision = false;

        var nextPosition = transform.position + _moveVector * Time.deltaTime;

        if (new Rect(0, 0, 1, 1).Contains(_mainCamera.WorldToViewportPoint(nextPosition)))
            transform.position = nextPosition;
        else
            _moveVector *= -1;
    }

    private float GetVectorDirectness(Vector2 vector)
    {
        float dotAbs = Mathf.Abs(Vector2.Dot(vector, Vector2.up));
        return 1f - Mathf.Min(dotAbs, 1f - dotAbs);
    }
}
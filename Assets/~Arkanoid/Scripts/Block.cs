using NaughtyAttributes;
using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _boxCollider2D;
    [SerializeField] private bool _isPowerUp;
    [SerializeField] private int _hitPointsMaxNumber;
    [SerializeField] [ReadOnly] private int _hitPointsNumber;

    public bool IsAlive { get; private set; } = true;
    public bool IsPowerUp => _isPowerUp;
    public int HitPointsMaxNumber => _hitPointsMaxNumber;
    public Indexes2D GridIndexes { get; private set; }

    private void Awake()
    {
        _hitPointsNumber = _hitPointsMaxNumber;
    }

    public void Init(Vector3 position, Vector2 size, Indexes2D gridIndexes)
    {
        GridIndexes = gridIndexes;

        transform.position = position;
        _spriteRenderer.size = size;
        _boxCollider2D.size = size;
        _boxCollider2D.offset = new Vector2(size.x / 2f, -size.y / 2f);
        gameObject.name = $"block_{gridIndexes.X}-{gridIndexes.Y}";
    }

    public void PlayHitAnimation()
    {

    }

    public void PlayDemolishAnimation(Action OnComplete = null)
    {
        OnComplete?.Invoke();
    }

    public void OnHitByBall(bool isAlmighty)
    {
        _hitPointsNumber--;
        if (_hitPointsNumber <= 0 || isAlmighty)
        {
            IsAlive = false;
            _boxCollider2D.enabled = false;
            PlayDemolishAnimation(() => gameObject.SetActive(false));
        }
        else
            PlayHitAnimation();
    }

}

using DG.Tweening;
using Gamelogic.Extensions;
using NaughtyAttributes;
using System;
using UnityEngine;
using ReadOnly = NaughtyAttributes.ReadOnlyAttribute;

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _boxCollider2D;
    [SerializeField] private bool _isPowerUp;
    [SerializeField] private int _hitPointsMaxNumber;
    [SerializeField] [ReadOnly] private int _hitPointsNumber;
    [SerializeField] private float _hitPunchForce;
    [SerializeField] private float _animationDuration = 1f;

    private Tween _punchTween;
    private Sequence _demolishSeq;
    private Vector3 _initPosition;

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

        _initPosition = position;
        transform.position = position;
        _spriteRenderer.size = size;
        _boxCollider2D.size = size;
        _boxCollider2D.offset = new Vector2(size.x / 2f, -size.y / 2f);
        gameObject.name = $"block_{gridIndexes.X}-{gridIndexes.Y}";
    }

    public void PlayHitAnimation(Vector2 ballDirection)
    {
        _punchTween?.Kill();
        transform.position = _initPosition;
        _punchTween = transform.DOPunchPosition(ballDirection * _hitPunchForce, _animationDuration, 4);
    }

    public void PlayDemolishAnimation(Vector2 ballDirection, Action OnComplete = null)
    {
        _demolishSeq?.Kill();

        _demolishSeq = DOTween.Sequence();
        _demolishSeq.Append(transform.DOMove(transform.position + (Vector3)ballDirection, _animationDuration));
        _demolishSeq.Join(_spriteRenderer.DOFade(0, _animationDuration));
        _demolishSeq.onComplete = () => OnComplete?.Invoke();
    }

    public void OnHitByBall(Vector2 ballDirection, bool isAlmighty)
    {
        _hitPointsNumber--;
        if (_hitPointsNumber <= 0 || isAlmighty)
        {
            IsAlive = false;
            _boxCollider2D.enabled = false;
            PlayDemolishAnimation(ballDirection, () => gameObject.SetActive(false));
        }
        else
            PlayHitAnimation(ballDirection);
    }

}

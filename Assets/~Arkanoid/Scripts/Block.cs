using NaughtyAttributes;
using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _boxCollider2D;
    [SerializeField] private int _hitPointsMaxNumber;
    [SerializeField] [ReadOnly] private int _hitPointsNumber;
    public bool IsAlive { get; private set; } = true;

    private void Awake()
    {
        _hitPointsNumber = _hitPointsMaxNumber;
    }

    public void Init(Vector3 position, Vector2 size, string name = "block")
    {
        transform.position = position;
        _spriteRenderer.size = size;
        _boxCollider2D.size = size;
        _boxCollider2D.offset = new Vector2(size.x / 2f, -size.y / 2f);
        gameObject.name = name;
    }

    public void PlayHitAnimation()
    {

    }

    public void PlayDemolishAnimation(Action OnComplete = null)
    {
        OnComplete?.Invoke();
    }

    public void OnHitByBall()
    {
        _hitPointsNumber--;
        if (_hitPointsNumber <= 0)
        {
            IsAlive = false;
            _boxCollider2D.enabled = false;
            PlayDemolishAnimation(() => gameObject.SetActive(false));
        }
        else
            PlayHitAnimation();
    }

}

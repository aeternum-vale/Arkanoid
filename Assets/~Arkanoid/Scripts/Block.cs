using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _boxCollider2D;


    public void SetPositionAndSize(Vector3 position, Vector2 size, string name = "block")
    {
        transform.position = position;
        _spriteRenderer.size = size;
        _boxCollider2D.size = size;
        _boxCollider2D.offset = new Vector2(size.x / 2f, -size.y / 2f);
        gameObject.name = name;
    }
}

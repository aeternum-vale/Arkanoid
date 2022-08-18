using Gamelogic.Extensions;
using NaughtyAttributes;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] Camera _mainCamera;

    [Header("Grid")]

    [SerializeField] float _topOffset;

    [SerializeField] float _blockWidth;
    [SerializeField] float _blockHeight;

    [SerializeField] int _blockColumnCount;
    [SerializeField] int _blockRowCount;

    [Header("Blocks")]
    [SerializeField] SpriteRenderer _blockPrefab;

    [SerializeField] int _debugBlockX;
    [SerializeField] int _debugBlockY;

    private float _boardWidth;
    private float _boardHeight;
    private Rect _boardBounds;

    private Vector2 _blockWorldSize;


    private void CaclulateBoardValues()
    {
        _boardWidth = _blockWidth * _blockColumnCount;
        _boardHeight = _blockHeight * _blockRowCount;

        _boardBounds =
            new Rect(
                _mainCamera.pixelWidth / 2f - _boardWidth / 2f,
                _mainCamera.pixelHeight - _boardHeight - _topOffset,
                _boardWidth,
                _boardHeight
            );

        _blockWorldSize = _mainCamera.ScreenToWorldPoint(new Vector2(_blockWidth, _blockHeight)) - _mainCamera.ScreenToWorldPoint(Vector2.zero);
    }

    private void OnDrawGizmos()
    {
        CaclulateBoardValues();

        float gridZ = 1f;

        Gizmos.color = Color.red;
        for (int i = 0; i <= _blockColumnCount; i++)
            Gizmos.DrawLine(
                _mainCamera.ScreenToWorldPoint(new Vector3(_boardBounds.x + _blockWidth * i, _boardBounds.y, gridZ)),
                _mainCamera.ScreenToWorldPoint(new Vector3(_boardBounds.x + _blockWidth * i, _boardBounds.y + _boardHeight, gridZ))
            );

        for (int i = 0; i <= _blockRowCount; i++)
            Gizmos.DrawLine(
                _mainCamera.ScreenToWorldPoint(new Vector3(_boardBounds.x, _boardBounds.y + _blockHeight * i, gridZ)),
                _mainCamera.ScreenToWorldPoint(new Vector3(_boardBounds.x + _boardWidth, _boardBounds.y + _blockHeight * i, gridZ))
            );

        Gizmos.color = Color.blue;

    }


    [Button]
    private void GenerateBlocks()
    {
        CaclulateBoardValues();

        Vector3 blockPosition =
            new Vector2(_boardBounds.x, _boardBounds.y + _boardHeight) +
            new Vector2(_debugBlockX * _blockWidth, -_debugBlockY * _blockHeight);

        SpriteRenderer block = Instantiate<SpriteRenderer>(_blockPrefab, _mainCamera.ScreenToWorldPoint(blockPosition).WithZ(0f), Quaternion.identity);
        block.size = _blockWorldSize;
    }


}

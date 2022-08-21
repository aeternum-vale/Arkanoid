using Gamelogic.Extensions;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BoardGenerator : MonoBehaviour
{
    private enum Pattern { Horizontal, Vertical, DiagRight, DiagLeft, Circle }

    [SerializeField] private Camera _mainCamera;

    [Header("Grid")]

    [SerializeField] private float _topOffset;

    [SerializeField] private float _blockWidth;
    [SerializeField] private float _blockHeight;

    [SerializeField] private int _blockColumnCount;
    [SerializeField] private int _blockRowCount;

    [Header("Blocks")]
    [SerializeField] private Transform _blocksParent;
    [SerializeField] private Block[] _blockPrefabs;

    [SerializeField] private int _seed;

    private float _boardWidth;
    private float _boardHeight;
    private Rect _boardBounds;

    private Vector2 _blockWorldSize;
    private bool[,] _blocksMask;
    private Random _random;

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

    private Block GenerateBlockInstance(int x, int y, Pattern pattern)
    {
        CaclulateBoardValues();

        Vector3 blockPosition =
            new Vector2(_boardBounds.x, _boardBounds.y + _boardHeight) +
            new Vector2(x * _blockWidth, -y * _blockHeight);
        blockPosition = _mainCamera.ScreenToWorldPoint(blockPosition).WithZ(0f);

        int prefabIndex = GetPrefabIndexViaPattern(x, y, pattern);

        Block block = Instantiate<Block>(_blockPrefabs[prefabIndex], _blocksParent);
        block.Init(blockPosition, _blockWorldSize, $"block_{x}-{y}");
        return block;
    }

    private int GetPrefabIndexViaPattern(int x, int y, Pattern pattern)
    {
        int centerX = _blockColumnCount / 2;
        int centerY = _blockRowCount / 2;

        switch (pattern)
        {
            case Pattern.Horizontal:
                return y % _blockPrefabs.Length;

            case Pattern.Vertical:
                return x % _blockPrefabs.Length;

            case Pattern.DiagRight:
                return (x + y) % _blockPrefabs.Length;

            case Pattern.DiagLeft:
                return Math.Abs(x - y) % _blockPrefabs.Length;

            case Pattern.Circle:
                return (int)Math.Round(Math.Sqrt(Sqr(centerX - x) + Sqr(centerY - y))) % _blockPrefabs.Length;

            default:
                throw new Exception("invalid pattern");
        }
    }

    [Button]
    private void GenerateRandomBoard()
    {
        GenerateRandomBoard(_seed);
    }


    public List<Block> GenerateRandomBoard(int seed)
    {
        List<Block> blockInstances = new List<Block>();

        DestroyAllBlocks();

        _random = new Random(seed);
        _blocksMask = new bool[_blockColumnCount, _blockRowCount];

        int figuresCount = _random.Next(5, 10);
        for (int i = 0; i < figuresCount; i++)
            ApplyRandomFigureToMask();

        Array values = Enum.GetValues(typeof(Pattern));
        Pattern randomPattern = (Pattern)values.GetValue(_random.Next(values.Length));

        for (int i = 0; i < _blocksMask.GetLength(0); i++)
            for (int j = 0; j < _blocksMask.GetLength(1); j++)
                if (_blocksMask[i, j])
                    blockInstances.Add(GenerateBlockInstance(i, j, randomPattern));

        return blockInstances;
    }

    private void DestroyAllBlocks()
    {
        while (_blocksParent.childCount > 0)
        {
            DestroyImmediate(_blocksParent.GetChild(0).gameObject);
        }
    }

    private void ApplyRandomFigureToMask()
    {
        bool inverse = _random.Next(2) == 0;

        switch (_random.Next(4))
        {
            case 0: ApplyCircle(inverse); break;
            case 1: ApplyRectangle(inverse); break;
        }

    }

    private void ApplyCircle(bool inverse)
    {
        int radius = _random.Next(1, Math.Min(_blockRowCount, _blockColumnCount) / 2);
        int centerX = _blockColumnCount / 2;
        int centerY = _blockRowCount / 2;

        BlockMaskMap((i, j) => Math.Sqrt(Sqr(centerX - i) + Sqr(centerY - j)) <= radius, inverse);
    }

    private void ApplyRectangle(bool inverse)
    {
        int hwidth = _random.Next(1, _blockColumnCount) / 2;
        int hheight = _random.Next(1, _blockRowCount) / 2;
        int centerX = _blockColumnCount / 2;
        int centerY = _blockRowCount / 2;

        BlockMaskMap((i, j) =>
            (i >= centerX - hwidth && i <= centerX + hwidth) &&
            (j >= centerY - hheight && j <= centerY + hheight),
            inverse);
    }

    private void BlockMaskMap(Func<int, int, bool> setter, bool inverse)
    {
        for (int i = 0; i < _blocksMask.GetLength(0); i++)
            for (int j = 0; j < _blocksMask.GetLength(1); j++)
            {
                bool val = setter.Invoke(i, j);
                if (inverse)
                    _blocksMask[i, j] = val ? !_blocksMask[i, j] : _blocksMask[i, j];
                else
                    _blocksMask[i, j] = val ? true : _blocksMask[i, j];
            }
    }

    private static int Sqr(int x) => (int)Math.Pow(x, 2);
}



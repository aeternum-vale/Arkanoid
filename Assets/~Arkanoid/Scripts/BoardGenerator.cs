using Gamelogic.Extensions;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BoardGenerator : MonoBehaviour
{
    private enum EPattern
    { Horizontal, Vertical, DiagRight, DiagLeft, Circle }

    [SerializeField] private Camera _mainCamera;

    [Header("Grid")]
    [SerializeField] private float _topOffset;

    [SerializeField] private float _blockWidth;
    [SerializeField] private float _blockHeight;

    [SerializeField] private int _blockColumnCount;
    [SerializeField] private int _blockRowCount;

    [Header("Blocks")]
    [SerializeField] private Transform _blocksParent;

    [SerializeField] private Block[] _simpleBlockPrefabs;
    [SerializeField] private Block _powerUpBlockPrefab;

    [Space]
    [SerializeField] private int _seed;

    private float _boardWidth;
    private float _boardHeight;
    private Rect _boardBounds;

    private Vector2 _blockWorldSize;
    private bool[,] _simpleBlocksMask;
    private bool[,] _powerUpBlocksMask;
    private Random _random;

    private void Awake()
    {
        CaclulateBoardValues();
    }

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

    [Button]
    private void GenerateRandomBoard()
    {
        GenerateBoard(_seed);
    }

    public List<Block> GenerateBoard(int seed, HashSet<Indexes2D> exclude = null)
    {
        List<Block> blockInstances = new List<Block>();

        DestroyAllBlocks();

        _random = new Random(seed + _seed);
        _simpleBlocksMask = new bool[_blockColumnCount, _blockRowCount];
        _powerUpBlocksMask = new bool[_blockColumnCount, _blockRowCount];

        int figuresCount = _random.Next(5, 10);
        for (int i = 0; i < figuresCount; i++)
            ApplyRandomFigureToSimpleMask();

        Array values = Enum.GetValues(typeof(EPattern));
        EPattern randomPattern = (EPattern)values.GetValue(_random.Next(values.Length));

        FillPowerUpMask();

        for (int i = 0; i < _simpleBlocksMask.GetLength(0); i++)
            for (int j = 0; j < _simpleBlocksMask.GetLength(1); j++)
            {
                if (exclude != null && exclude.Contains(new Indexes2D(i, j)))
                    continue;

                if (_powerUpBlocksMask[i, j])
                {
                    blockInstances.Add(GenerateBlockInstance(i, j, _powerUpBlockPrefab));
                    continue;
                }

                if (_simpleBlocksMask[i, j])
                    blockInstances.Add(GenerateSimpleBlockInstance(i, j, randomPattern));
            }

        return blockInstances;
    }

    private void FillPowerUpMask()
    {
        int powerUpCount = 6 * GetSimpleBlockMaskWeight() / (_blockRowCount * _blockColumnCount);
        bool even = powerUpCount % 2 == 0;
        int powerUpEvenCount = even ? powerUpCount : powerUpCount - 1;

        for (int i = 0; i < powerUpEvenCount / 2; i++)
            ApplyTwoPowerUpBlocks();

        if (!even)
            ApplyOnePowerUpBlock();
    }

    private void ApplyTwoPowerUpBlocks()
    {
        var x = _random.Next(_blockColumnCount / 2);
        var y = _random.Next(_blockRowCount / 2);

        _powerUpBlocksMask[x, y] = true;
        _powerUpBlocksMask[_blockColumnCount - 1 - x, y] = true;
    }

    private void ApplyOnePowerUpBlock()
    {
        var y = _random.Next(_blockRowCount / 2);
        _powerUpBlocksMask[_blockColumnCount / 2, y] = true;
    }

    private Block GenerateSimpleBlockInstance(int x, int y, EPattern pattern)
    {
        int prefabIndex = GetPrefabIndexViaPattern(x, y, pattern);
        return GenerateBlockInstance(x, y, _simpleBlockPrefabs[prefabIndex]);
    }

    private Block GenerateBlockInstance(int x, int y, Block prefab)
    {
        Vector3 blockPosition =
            new Vector2(_boardBounds.x, _boardBounds.y + _boardHeight) +
            new Vector2(x * _blockWidth, -y * _blockHeight);
        blockPosition = _mainCamera.ScreenToWorldPoint(blockPosition).WithZ(0f);

        Block block = Instantiate<Block>(prefab, _blocksParent);
        block.Init(blockPosition, _blockWorldSize, new Indexes2D(x, y));
        return block;
    }

    private int GetPrefabIndexViaPattern(int x, int y, EPattern pattern)
    {
        int centerX = _blockColumnCount / 2;
        int centerY = _blockRowCount / 2;

        switch (pattern)
        {
            case EPattern.Horizontal:
                return y % _simpleBlockPrefabs.Length;

            case EPattern.Vertical:
                return x % _simpleBlockPrefabs.Length;

            case EPattern.DiagRight:
                return (x + y) % _simpleBlockPrefabs.Length;

            case EPattern.DiagLeft:
                return Math.Abs(x - y) % _simpleBlockPrefabs.Length;

            case EPattern.Circle:
                return (int)Math.Round(Math.Sqrt(Sqr(centerX - x) + Sqr(centerY - y))) % _simpleBlockPrefabs.Length;

            default:
                throw new Exception("invalid pattern");
        }
    }

    private void DestroyAllBlocks()
    {
        if (Application.isPlaying)
        {
            foreach (Transform child in _blocksParent)
                Destroy(child.gameObject);
        }
        else
        {
            while (_blocksParent.childCount > 0)
                DestroyImmediate(_blocksParent.GetChild(0).gameObject);
        }
    }

    private void ApplyRandomFigureToSimpleMask()
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

        SimpleBlockMaskMap((i, j) => Math.Sqrt(Sqr(centerX - i) + Sqr(centerY - j)) <= radius, inverse);
    }

    private void ApplyRectangle(bool inverse)
    {
        int hwidth = _random.Next(1, _blockColumnCount) / 2;
        int hheight = _random.Next(1, _blockRowCount) / 2;
        int centerX = _blockColumnCount / 2;
        int centerY = _blockRowCount / 2;

        SimpleBlockMaskMap((i, j) =>
            (i >= centerX - hwidth && i <= centerX + hwidth) &&
            (j >= centerY - hheight && j <= centerY + hheight),
            inverse);
    }

    private void SimpleBlockMaskMap(Func<int, int, bool> setter, bool inverse)
    {
        for (int i = 0; i < _simpleBlocksMask.GetLength(0); i++)
            for (int j = 0; j < _simpleBlocksMask.GetLength(1); j++)
            {
                bool val = setter.Invoke(i, j);
                if (inverse)
                    _simpleBlocksMask[i, j] = val ? !_simpleBlocksMask[i, j] : _simpleBlocksMask[i, j];
                else
                    _simpleBlocksMask[i, j] = val ? true : _simpleBlocksMask[i, j];
            }
    }

    private int GetSimpleBlockMaskWeight()
    {
        int w = 0;
        for (int i = 0; i < _simpleBlocksMask.GetLength(0); i++)
            for (int j = 0; j < _simpleBlocksMask.GetLength(1); j++)
                if (_simpleBlocksMask[i, j]) w++;

        return w;
    }

    private static int Sqr(int x) => (int)Math.Pow(x, 2);

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        CaclulateBoardValues();

        float gridZ = 3f;

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

    private void OnValidate()
    {
        CaclulateBoardValues();
    }

#endif
}
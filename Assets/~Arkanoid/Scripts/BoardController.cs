using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action BallHitPowerUp;
    public event Action BallHitBlock;
    public event Action<Block> BallDemolishedBlock;
    public event Action AllBlocksDemolished;

    [SerializeField] private BoardGenerator _generator;

    private List<Block> _blocks;
    private Dictionary<GameObject, Block> _blocksGODictionary;
    private HashSet<Indexes2D> _demolishedBlockGridIndexes = new HashSet<Indexes2D>();

    [SerializeField] private Ball _ball;

    public HashSet<Indexes2D> DemolishedBlockGridIndexes => _demolishedBlockGridIndexes;

    private void Awake()
    {
        AddListeners();
    }

    public void PrepareNewBoard(int level)
    {
        _demolishedBlockGridIndexes.Clear();
        _blocks = _generator.GenerateBoard(level);
        _blocksGODictionary = _blocks.ToDictionary(block => block.gameObject);
    }

    public void RestoreSession(int level, HashSet<Indexes2D> demolishedBlockGridIndexes)
    {
        _demolishedBlockGridIndexes = demolishedBlockGridIndexes;
        _blocks = _generator.GenerateBoard(level, demolishedBlockGridIndexes);
        _blocksGODictionary = _blocks.ToDictionary(block => block.gameObject);
    }

    private void AddListeners()
    {
        _ball.BlockHit += OnBallHitBlock;
    }

    private void RemoveListeners()
    {
        _ball.BlockHit -= OnBallHitBlock;
    }

    private void OnBallHitBlock((GameObject, Vector2) args)
    {
        GameObject blockGameObject = args.Item1;
        Vector2 initialDirection = args.Item2;

        Block block = _blocksGODictionary[blockGameObject];

        block.OnHitByBall(initialDirection, _ball.IsAlmighty);

        if (!block.IsAlive)
        {
            _demolishedBlockGridIndexes.Add(block.GridIndexes);
            BallDemolishedBlock?.Invoke(block);

            if (AreAllBlocksDemolished())
            {
                AllBlocksDemolished?.Invoke();
                return;
            }

            if (block.IsPowerUp)
                BallHitPowerUp?.Invoke();
        }
        else
            BallHitBlock?.Invoke();
    }

    private bool AreAllBlocksDemolished()
    {
        for (int i = 0; i < _blocks.Count; i++)
            if (_blocks[i].IsAlive) return false;
        return true;
    }

    private void OnDestroy() => RemoveListeners();


}

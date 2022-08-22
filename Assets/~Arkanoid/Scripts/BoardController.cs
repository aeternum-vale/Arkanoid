using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action BallHitPowerUp;
    public event Action<Block> BallDemolishedBlock;
    public event Action AllBlocksDemolished;

    [SerializeField] private BoardGenerator _generator;

    private List<Block> _blocks;
    private Dictionary<GameObject, Block> _blocksGODictionary;
    private List<Indexes2D> _demolishedBlockGridIndexes = new List<Indexes2D>();

    [SerializeField] private Ball _ball;

    private void Awake()
    {
        AddListeners();
    }

    public void PrepareNewBoard(int level)
    {
        _demolishedBlockGridIndexes.Clear();
        _blocks = _generator.GenerateRandomBoard(level);
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

    private void OnBallHitBlock(GameObject blockGameObject)
    {
        Block block = _blocksGODictionary[blockGameObject];

        block.OnHitByBall(_ball.IsAlmighty);

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

    }

    private bool AreAllBlocksDemolished()
    {
        for (int i = 0; i < _blocks.Count; i++)
            if (_blocks[i].IsAlive) return false;
        return true;
    }

    private void OnDestroy() => RemoveListeners();

}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action AllBlocksDemolished;

    [SerializeField] private BoardGenerator _generator;

    private List<Block> _blocks;
    private Dictionary<GameObject, Block> _blocksDictionary;

    [SerializeField] private Ball _ball;

    private void Awake()
    {
        AddListeners();
    }

    public void PrepareBoard(int level)
    {
        _blocks = _generator.GenerateRandomBoard(level);
        _blocksDictionary = _blocks.ToDictionary(block => block.gameObject);
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
        Block block = _blocksDictionary[blockGameObject];

        block.OnHitByBall();

        if (!block.IsAlive && AreAllBlocksDemolished())
            AllBlocksDemolished?.Invoke();
    }

    private bool AreAllBlocksDemolished()
    {
        for (int i = 0; i < _blocks.Count; i++)
            if (_blocks[i].IsAlive) return false;
        return true;
    }

    private void OnDestroy() => RemoveListeners();

}

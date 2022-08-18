using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] GameObject _blockPrefab;


    private void OnDrawGizmos()
    {
        float boardWidth = _blockWidth * _blockColumnCount;
        float boardHeight = _blockHeight * _blockRowCount;
        float z = 1f;

        Rect boardBounds =
            new Rect(
                _mainCamera.pixelWidth / 2f - boardWidth / 2f,
                _mainCamera.pixelHeight - boardHeight - _topOffset,
                boardWidth,
                boardHeight
            );

        Gizmos.color = Color.red;
        for (int i = 0; i <= _blockColumnCount; i++)
            Gizmos.DrawLine(
                _mainCamera.ScreenToWorldPoint(new Vector3(boardBounds.x + _blockWidth * i, boardBounds.y, z)),
                _mainCamera.ScreenToWorldPoint(new Vector3(boardBounds.x + _blockWidth * i, boardBounds.y + boardHeight, z))
            );

        for (int i = 0; i <= _blockRowCount; i++)
            Gizmos.DrawLine(
                _mainCamera.ScreenToWorldPoint(new Vector3(boardBounds.x, boardBounds.y + _blockHeight * i, z)),
                _mainCamera.ScreenToWorldPoint(new Vector3(boardBounds.x + boardWidth, boardBounds.y + _blockHeight * i, z))
            );
    }



}

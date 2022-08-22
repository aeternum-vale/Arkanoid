using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public const int SliderLayer = 6;
    public const int BlocksLayer = 7;
    public const int BottomLayer = 8;
}

public enum EPowerUpType { None = 0, AlmightyBall, WiderSlider }

public struct Indexes2D
{
    public int X;
    public int Y;

    public Indexes2D(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public struct SessionData
{
    public int Level;
    public List<Indexes2D> DemolishedBlockGridIndexes;
    public int Score;
    public Vector2 _ballPosition;
    public Vector2 _ballDirection;
    public float _sliderXPosition;
}
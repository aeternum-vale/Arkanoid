using System.Collections.Generic;

public static class Constants
{
    public const int SliderLayer = 6;
    public const int BlocksLayer = 7;
    public const int BottomLayer = 8;
    public const int WallsLayer = 9;

    public const string ContinueModeKey = "ContinueModeKey";

    public const int MenuSceneIndex = 0;
    public const int GameSceneIndex = 1;

}

public enum EPowerUpType { None = 0, AlmightyBall, WiderSlider, Boost }

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

public struct SimpleVector2D
{
    public float X;
    public float Y;

    public SimpleVector2D(float x, float y)
    {
        X = x;
        Y = y;
    }
}

public struct SessionData
{
    public int Level;
    public int Lives;
    public int Score;
    public HashSet<Indexes2D> DemolishedBlockGridIndexes;
    public SimpleVector2D BallPosition;
    public SimpleVector2D BallDirection;
    public float SliderXPosition;
}
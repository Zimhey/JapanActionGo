using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeNode
{
    public MazeNode Left;
    public MazeNode Right;
    public MazeNode Forward;
    public MazeNode Backward;
    public MazeNode Above;
    public MazeNode Below;

    public bool ExitNode;

    public int Row;
    public int Col;

}

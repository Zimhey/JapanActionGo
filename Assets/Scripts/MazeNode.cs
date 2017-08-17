using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Orphan,
    DeadEnd,
    Hallway,
    Corner,
    Threeway,
    Fourway,
};

public class MazeNode
{
    public MazeNode Left;
    public MazeNode Right;
    public MazeNode Forward;
    public MazeNode Backward;
    public MazeNode Above;
    public MazeNode Below;

    public bool OnExitPath;
    public bool Intersection;
    public bool OnDeadEndPath;
    public bool OnLoopPath;

    public int leftNodes = 0;
    public int rightNodes = 0;
    public int forwardNodes = 0;
    public int backwardNodes = 0;

    public ActorType actor;

    public int Row;
    public int Col;


    public MazeNode()
    {
    }

    public MazeNode(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public void connectLeft(MazeNode n)
    {
        this.Left = n;
    }

    public void connectRight(MazeNode n)
    {
        this.Right = n;
    }

    public void connectForward(MazeNode n)
    {
        this.Forward = n;
    }

    public void connectBackward(MazeNode n)
    {
        this.Backward = n;
    }

    public void connectAbove(MazeNode n)
    {
        this.Above = n;
    }

    public void connectBelow(MazeNode n)
    {
        this.Below = n;
    }

    public void DisconnectLeft()
    {
        this.Left = null;
    }

    public void DisconnectRight()
    {
        this.Right = null;
    }

    public void DisconnectForward()
    {
        this.Forward = null;
    }

    public void DisconnectBackward()
    {
        this.Backward = null;
    }

    public bool AddEdge(MazeNode other)
    {
        if (Row == other.Row)
        {
            if (Col + 1 == other.Col)
            {
                Right = other;
                other.Left = this;
            }
            else if (Col - 1 == other.Col)
            {
                Left = other;
                other.Right = this;
            }
            else
                return false;
        }
        else if (Col == other.Col)
        {
            if (Row + 1 == other.Row)
            {
                Forward = other;
                other.Backward = this;
            }
            else if (Row - 1 == other.Row)
            {
                Backward = other;
                other.Forward = this;
            }
            else
                return false;
        }
        else
            return false;
        return true;
    }

    public bool RemoveEdge(MazeNode other)
    {
        if (Row == other.Row)
        {
            if (Col + 1 == other.Col)
            {
                Right = null;
                other.Left = null;
            }
            else if (Col - 1 == other.Col)
            {
                Left = null;
                other.Right = null;
            }
            else
                return false;
        }
        else if (Col == other.Col)
        {
            if (Row + 1 == other.Row)
            {
                Forward = null;
                other.Backward = null;
            }
            else if (Row - 1 == other.Row)
            {
                Backward = null;
                other.Forward = null;
            }
            else
                return false;
        }
        else
            return false;
        return true;
    }

    public List<MazeNode> GetAdjacentNodes()
    {
        List<MazeNode> nodes = new List<MazeNode>();

        if (Left != null)
            nodes.Add(Left);
        if (Right != null)
            nodes.Add(Right);
        if (Forward != null)
            nodes.Add(Forward);
        if (Backward != null)
            nodes.Add(Backward);
        if (Above != null)
            nodes.Add(Above);
        if (Below != null)
            nodes.Add(Below);

        return nodes;
    }

    public CellType GetCellType()
    {
        switch (GetAdjacentNodes().Count)
        {
            case 1:
                return CellType.DeadEnd;
            case 2:
                if (isHallway())
                    return CellType.Hallway;
                else
                    return CellType.Corner;
            case 3:
                return CellType.Threeway;
            case 4:
                return CellType.Fourway;
            default:
                return CellType.Orphan;
        }
    }

    public string GetPrefabName()
    {
        switch (GetCellType())
        {
            case CellType.DeadEnd:
                return "Prefabs/ThreeWallPrefab";
            case CellType.Hallway:
                return "Prefabs/TwoWallPrefab";
            case CellType.Corner:
                return "Prefabs/CornerWallPrefab";
            case CellType.Threeway:
                return "Prefabs/OneWallPrefab";
            case CellType.Fourway:
                return "Prefabs/NoWallPrefab";
            default:
                return "Prefabs/OrphanTextPrefab";
        }
    }

    public Quaternion GetRotation()
    {
        switch(GetCellType())
        {
            case CellType.DeadEnd:
                return getDeadEndRot();
            case CellType.Corner:
                return getCornerRot();
            case CellType.Hallway:
                return getHallwayRot();
            case CellType.Threeway:
                return getThreewayRot();
        }

        return YRotToQuat(0);
    }

    private Quaternion YRotToQuat(int rot)
    {
        return Quaternion.Euler(new Vector3(0, rot, 0));
    }

    private Quaternion getDeadEndRot()
    {
        if (Forward != null)
            return YRotToQuat(90);
        if (Right != null)
            return YRotToQuat(180);
        if (Backward != null)
            return YRotToQuat(-90);
        return YRotToQuat(0);
    }

    private Quaternion getCornerRot()
    {
        if (Forward != null)
            if (Right != null)
                return YRotToQuat(90);
            else
                return YRotToQuat(0);
        if (Right != null)
            return YRotToQuat(180);
        return YRotToQuat(-90);
    }

    private Quaternion getHallwayRot()
    {
        if (Forward != null)
            return YRotToQuat(90);
        return YRotToQuat(0);
    }

    private Quaternion getThreewayRot()
    {
        if (Left == null)
            return YRotToQuat(90);
        if (Forward == null)
            return YRotToQuat(180);
        if (Right == null)
            return YRotToQuat(-90);
        else
            return YRotToQuat(0);
    }

    private bool isCornerPiece()
    {
        if (GetAdjacentNodes().Count == 2 && !isHallway())
            return true;
        return false;
    }

    private bool isHallway()
    {
        if (GetAdjacentNodes().Count == 2)
            if (Forward != null && Backward != null || Left != null && Right != null)
                return true;
        return false;
    }

    private bool isDeadEnd()
    {
        return GetAdjacentNodes().Count == 1;
    }

}

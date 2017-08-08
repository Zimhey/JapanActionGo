using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeNode : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public MazeNode Left;
    public MazeNode Right;
    public MazeNode Forward;
    public MazeNode Backward;
    public MazeNode Above;
    public MazeNode Below;

    public bool ExitNode;

    public int Row;
    public int Col;

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
}

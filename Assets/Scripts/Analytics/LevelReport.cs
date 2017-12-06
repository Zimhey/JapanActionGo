using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelReport
{
    int LevelID;
    int Seed;
    int Difficulty;

    List<Section> Sections;
}

public struct Section
{
    int SectionID;
    int Index;
    int Floor;
}

public struct Cell
{
    int SectionID;
    int CellID;
    int CellRow;
    int CellCol;
}
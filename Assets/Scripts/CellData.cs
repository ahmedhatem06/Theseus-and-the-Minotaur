using UnityEngine;

[System.Serializable]
public class CellData
{
    public bool wallUp;
    public bool wallDown;
    public bool wallLeft;
    public bool wallRight;
    public bool hasExit;
    
    public CellData()
    {
        wallUp = false;
        wallDown = false;
        wallLeft = false;
        wallRight = false;
        hasExit = false;
    }
    
    public CellData(bool up, bool down, bool left, bool right, bool exit = false)
    {
        wallUp = up;
        wallDown = down;
        wallLeft = left;
        wallRight = right;
        hasExit = exit;
    }
    
    public void CopyTo(Cell cell)
    {
        cell.wallUp = wallUp;
        cell.wallDown = wallDown;
        cell.wallLeft = wallLeft;
        cell.wallRight = wallRight;
        cell.hasExit = hasExit;
    }
}
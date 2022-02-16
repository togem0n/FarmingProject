using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPropertyDetail : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public bool isDiaggable = false;
    public bool canDropItem = false;
    public bool canPlaceFurniture = false;
    public bool isPath = false;
    public bool isNPCObstacle = false;
    public int daysSinceDug = -1;
    public int daysSinceWatered = -1;
    public int seedItemCode = -1;
    public int growthDays = -1;
    public int daysSinceLastHarvest = -1;

    public GridPropertyDetail()
    {

    }
}

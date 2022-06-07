using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _point;
    bool areLinesDrawn = false;
    bool arePointsDrawn = true;

    public void DrawAllLines()
    {
        if(areLinesDrawn)
            PointsManager.DeleteLines();
        else
            PointsManager.DrawAllLines();
        areLinesDrawn = !areLinesDrawn;
    }

    public void DrawAllPoints()
    {
        if(arePointsDrawn)
            PointsManager.SetActivePoints(false);
        else
            PointsManager.SetActivePoints(true);
        arePointsDrawn = !arePointsDrawn;
    }

    public void ExecuteChaikinAlgo()
    {
        PointsManager.DeleteLines();
        PointsManager.ExecuteChaikinAlgorithm(_point);
        if(areLinesDrawn)
            PointsManager.DrawAllLines();
        if(!arePointsDrawn)
            PointsManager.SetActivePoints(false);
    }

    public void DeletePoints()
    {
        PointsManager.DeleteLines();
        PointsManager.DeletePoints();
        areLinesDrawn = false;
        arePointsDrawn = true;
    }
}

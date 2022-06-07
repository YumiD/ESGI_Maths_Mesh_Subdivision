using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public void DrawAllLines()
    {
        PointsManager.DrawAllLines();
    }

    public void ExecuteChaikinAlgo()
    {
        PointsManager.ExecuteChaikinAlgorithm();
    }

    public void DeletePoints()
    {
        PointsManager.DeleteLines();
        PointsManager.DeletePoints();
    }

    public void CancelPoints()
    {
        PointsManager.DeleteLines();
        PointsManager.CancelPoints();
    }
}

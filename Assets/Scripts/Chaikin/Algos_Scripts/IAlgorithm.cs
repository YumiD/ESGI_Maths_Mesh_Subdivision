using System.Collections.Generic;
using UnityEngine;


public interface IAlgorithm
{
    void MainAlgorithm(List<GameObject> points);
    void ExecuteAlgorithm();
}
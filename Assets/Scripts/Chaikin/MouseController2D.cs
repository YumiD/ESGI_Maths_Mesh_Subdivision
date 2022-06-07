using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController2D : MonoBehaviour
{
    [SerializeField] private GameObject point;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var mp = Input.mousePosition;
            mp.z = 10f;
            Vector2 pos = Camera.main.ScreenToWorldPoint(mp);
            var p = Instantiate(point, pos, Quaternion.identity);
            PointsManager.Points.Add(p);
        }
    }
}

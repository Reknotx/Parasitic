/*
 * Author: Ryan Dangerfield
 * Date: 10/7/2020
 * 
 * Brief: Set Path of enemy;
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public GameObject enemyPathLine;
    public GameObject endPoint;
    public static EnemyPath Instance;
    LineRenderer lineRenderer;
    
    float pathHeight = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        lineRenderer = enemyPathLine.GetComponent<LineRenderer>();
    }


    public void DrawPath(List<Tile> path, Enemy enemyUnit)
    {
        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(enemyUnit.transform.position.x, pathHeight, enemyUnit.transform.position.z));
        foreach (Tile tile in path)
        {
            points.Add(new Vector3(tile.transform.position.x, pathHeight, tile.transform.position.z));
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        endPoint.transform.position = points[points.Count - 1];
        enemyPathLine.SetActive(true);
        endPoint.SetActive(true);
    }

    public void HidePath()
    {
        enemyPathLine.SetActive(false);
        endPoint.SetActive(false);
    }
}

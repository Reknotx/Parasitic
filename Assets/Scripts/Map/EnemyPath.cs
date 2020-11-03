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


    public void DrawPath(List<Tile> path, Vector3 startPoint, Vector3 p1 = default(Vector3))
    {

        List<Vector3> points = new List<Vector3>();
        points.Add(startPoint + Vector3.up * pathHeight);
        if (p1 != Vector3.zero)
        {
            points.Add(p1 + Vector3.up * pathHeight);
        }
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].slope)
            {
                bool fromSlope = false;
                if (i > 0)
                {
                    fromSlope = path[i - 1].slope;
                }
                //points is used instead of path here because there is always a previos point
                if (i!=0 && !fromSlope)
                points.Add(new Vector3((points[points.Count - 1].x - path[i].transform.position.x) / 2 + path[i].transform.position.x,
                    points[points.Count - 1].y,
                    (points[points.Count - 1].z - path[i].transform.position.z) / 2 + path[i].transform.position.z));
                //print((path[i - 1].transform.position.x - path[i].transform.position.x) / 2f + path[i].transform.position.x);
                points.Add(new Vector3(path[i].transform.position.x, path[i].Elevation + MapGrid.Instance.tileHeight / 2 + pathHeight, path[i].transform.position.z));
                if (path[i].slope && i != path.Count - 1)
                {
                    if (!path[i + 1].slope)
                    points.Add(new Vector3((path[i + 1].transform.position.x - path[i].transform.position.x) / 2 + path[i].transform.position.x,
                        path[i + 1].Elevation + pathHeight,
                        (path[i + 1].transform.position.z - path[i].transform.position.z) / 2 + path[i].transform.position.z));
                }
            }
            else
            {
                points.Add(path[i].transform.position + Vector3.up * (pathHeight + path[i].Elevation));
            }


        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        endPoint.transform.position = points[points.Count - 1];
        if (path[path.Count - 1].slope)
        {
            endPoint.transform.rotation = Quaternion.Euler(path[path.Count - 1].tilt.eulerAngles.x, endPoint.transform.rotation.eulerAngles.y, path[path.Count - 1].tilt.eulerAngles.z);
        }
        else
        {
            endPoint.transform.rotation = Quaternion.Euler(0, endPoint.transform.rotation.eulerAngles.y, 0);
        }
        enemyPathLine.SetActive(true);
        endPoint.SetActive(true);
    }

    public void HidePath()
    {
        enemyPathLine.SetActive(false);
        endPoint.SetActive(false);
    }
}

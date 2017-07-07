using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class DataPlotter : MonoBehaviour
{
    public string inputfile;
    private List<Dictionary<string, object>> points;
    public GameObject PointPrefab;
    public GameObject PointHolder;
    public float plotScale = 10;

    // Use this for initialization
    void Start()
    {
        points = CSVReader.Read(inputfile);

        List<string> columns = new List<string>(points[1].Keys);
        Debug.Log("There are " + columns.Count + " columns in: " + inputfile);
        foreach (string key in columns) Debug.Log("Column name is " + key);

        string xName = columns[0];
        string yName = columns[1];
        string zName = columns[2];

        float xMax = FindMaxValue(xName);
        float yMax = FindMaxValue(yName);
        float zMax = FindMaxValue(zName);

        float xMin = FindMinValue(xName);
        float yMin = FindMinValue(yName);
        float zMin = FindMinValue(zName);

        foreach (var point in points)
        {
            // Get value in poinList at each row in columm, then normalize
            float x = (Convert.ToSingle(point[xName]) - xMin) / (xMax - xMin);
            float y = (Convert.ToSingle(point[yName]) - yMin) / (yMax - yMin);
            float z = (Convert.ToSingle(point[zName]) - zMin) / (zMax - zMin);

            string species = Convert.ToString(point[columns[5]]);
            Debug.Log(species);

            // Instantiate as gameobject variable so that it can be manipulated within loop
            GameObject dataPoint = Instantiate(
                PointPrefab,
                new Vector3(x, y, z) * plotScale,
                Quaternion.identity
            );

            dataPoint.transform.parent = PointHolder.transform;
            string dataPointName = point[xName] + " " + point[yName] + " " + point[zName];
            dataPoint.transform.name = dataPointName;

            switch (species)
            {
                case "setosa":
                    dataPoint.GetComponent<Renderer>().material.color = Color.magenta;
                    break;
                case "versicolor":
                    dataPoint.GetComponent<Renderer>().material.color = Color.blue;
                    break;
                case "virginica":
                    dataPoint.GetComponent<Renderer>().material.color = Color.green;
                    break;
            }
        }
    }

    private float FindMaxValue(string columnName)
    {
        float maxValue = Convert.ToSingle(points[0][columnName]);
        return points.Select(t => Convert.ToSingle(t[columnName])).Concat(new[] {maxValue}).Max();
    }

    private float FindMinValue(string columnName)
    {
        float minValue = Convert.ToSingle(points[0][columnName]);
        return points.Select(t => Convert.ToSingle(t[columnName])).Concat(new[] {minValue}).Min();
    }

}

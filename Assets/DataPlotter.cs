using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class DataPlotter : MonoBehaviour
{
    private List<Dictionary<string, object>> _dataPoints;

    public GameObject PointPrefab;
    public GameObject PointHolder;
    public TextMesh XAxis;
    public TextMesh YAxis;
    public TextMesh ZAxis;
    public float PlotScale = 10;

    private readonly float _speed = 3;
    private int _count;

    private readonly Dictionary<string, Color> _dataTypes = new Dictionary<string, Color>();

    private string _xName, _yName, _zName, _typeName, _scaleName;
    private float _xMax, _yMax, _zMax, _sMax, _xMin, _yMin, _zMin, _sMin;
    private int _maxCount;

    private List<Dictionary<string, object>> _setOfPoints, nextSetOfPoints;
    private List<GameObject> _gameObjects, nextGameObjects;

    // Use this for initialization
    void Start()
    {
        InitData();
        _setOfPoints = GetSetOfPoints(_dataPoints);
        _gameObjects = ConvertToGameObjects(_setOfPoints);
        PlotObjects(_gameObjects, _setOfPoints);
    }

    void Update()
    {
        if (_count > _maxCount /_zMax) return;

        if (nextSetOfPoints == null || _gameObjects[0].transform.position == nextGameObjects[0].transform.position)
        {
            nextSetOfPoints = GetSetOfPoints(_dataPoints);
            nextGameObjects = ConvertToGameObjects(nextSetOfPoints);
        }
        else
        {
            for (var i = 0; i < _gameObjects.Count; i++)
            {
                var step = _speed*Time.deltaTime;
                _gameObjects[i].transform.position = Vector3.MoveTowards(_gameObjects[i].transform.position, nextGameObjects[i].transform.position, step);
            }
        }
    }

    private void InitData()
    {
        const string inputfile = "fatalities";
        _dataPoints = CSVReader.Read(inputfile);
        _maxCount = _dataPoints.Count;

        var columns = new List<string>(_dataPoints[1].Keys);

        _xName = columns[0];
        _yName = columns[1];
        _zName = columns[2];
        _typeName = columns[3];
        _scaleName = columns[4];

        _xMax = FindMaxValue(_xName);
        _yMax = FindMaxValue(_yName);
        _zMax = FindMaxValue(_zName);
        _sMax = FindMaxValue(_scaleName);

        _xMin = FindMinValue(_xName);
        _yMin = FindMinValue(_yName);
        _zMin = FindMinValue(_zName);
        _sMin = FindMinValue(_scaleName);

        var dataTypeName = Convert.ToString(_dataPoints[1][_typeName]);
        _dataTypes.Add(dataTypeName, RandomColor());

        XAxis.text = _xName;
        YAxis.text = _yName;
        ZAxis.text = _zName;
    }

    private float FindMaxValue(string columnName)
    {
        var maxValue = Convert.ToSingle(_dataPoints[0][columnName]);
        return _dataPoints.Select(t => Convert.ToSingle(t[columnName])).Concat(new[] { maxValue }).Max();
    }

    private float FindMinValue(string columnName)
    {
        var minValue = Convert.ToSingle(_dataPoints[0][columnName]);
        return _dataPoints.Select(t => Convert.ToSingle(t[columnName])).Concat(new[] { minValue }).Min();
    }

    private static Color RandomColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
    }

    private List<Dictionary<string, object>> GetSetOfPoints(IList<Dictionary<string, object>> dataSet)
    {
        var output = new List<Dictionary<string, object>>();
        var step = Convert.ToInt32(_zMax);

        for (var i = step * _count; i < step * (_count + 1); i++)
        {
            if (i == _maxCount - 1) return output;
            output.Add(dataSet[i]);
        }

        _count++;
        return output;
    }

    private List<GameObject> ConvertToGameObjects(IEnumerable<Dictionary<string, object>> points)
    {
        return (from point in points
                let x = (Convert.ToSingle(point[_xName]) - _xMin) / (_xMax - _xMin)
                let y = (Convert.ToSingle(point[_yName]) - _yMin) / (_yMax - _yMin)
                let z = (Convert.ToSingle(point[_zName]) - _zMin) / (_zMax - _zMin)
                select Instantiate(PointPrefab, new Vector3(x, y, z) * PlotScale, Quaternion.identity))
                .ToList();
    }

    private void PlotObjects(IList<GameObject> objects, IList<Dictionary<string, object>> points)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var type = Convert.ToString(points[i][_typeName]);
            var scale = (Convert.ToSingle(points[i][_scaleName]) - _sMin) / (_sMax - _sMin);

            if (!_dataTypes.ContainsKey(type))
            {
                _dataTypes.Add(type, RandomColor());
            }

            objects[i].transform.parent = PointHolder.transform;
            objects[i].transform.localScale += new Vector3(scale, scale, scale);
            var dataPointName = points[i][_xName] + ", " + points[i][_yName] + ", " + points[i][_zName];
            objects[i].transform.name = dataPointName;
            objects[i].GetComponent<Renderer>().material.color = _dataTypes[type];
        }

    }


}

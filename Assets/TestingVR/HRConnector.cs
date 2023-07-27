using Editor;
using System;
using UnityEngine;

public class HRConnector : MonoBehaviour
{
    [SerializeField]
    private Graph _graph;
    [SerializeField]
    SoundInstanceManager _soundInstanceManager;


    private void Awake()
    {
        HeartBeatWindow.OnNotifyHeartBeat += CountUp;
        
        Debug.Log(Remap(50, 65, 40, 0.5f, 1f));
        Debug.Log(Remap(120, 75, 140, 0.5f, 0f));
    }
    
    private void CountUp(DateTime time, int value)
    {
        AdjustStressManipulator(value);
        
        var data = _graph.data;

        var newData = new int[data.Length + 1];

        for (var i = 0; i != data.Length; i++)
        {
            newData[i] = data[i];
        }

        newData[data.Length] = value;

        _graph.data = newData;
    }

    private void AdjustStressManipulator(int value)
    {
        if (value <= 65)
        {
            _soundInstanceManager.SetManagerLevel(true, Remap(value, 65, 40, 0.5f, 1f));
        }
        if (value >= 75)
        {
            _soundInstanceManager.SetManagerLevel(true, Remap(value, 75, 140, 0.5f, 0f));
        }
    }
    
    private static float Remap(float value, float from1, float to1, float from2, float to2) 
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    private void Update()
    {
        if(_graph.data.Length > 2)
            Debug.Log("SLOPE " + CalculateSlope(new Vector2(0, _graph.data[0]), new Vector2(_graph.data.Length, _graph.data[^1])));
    }
    
    private float CalculateSlope(Vector2 pointA, Vector2 pointB)
    {
        if (pointB.x - pointA.x == 0)
        {
            Debug.Log("Division durch null. Die Punkte haben dieselbe X-Koordinate.");
            return float.PositiveInfinity;
        }

        float slope = (pointB.y - pointA.y) / (pointB.x - pointA.x);
        return slope;
    }

    private void OnDestroy()
    {
        HeartBeatWindow.OnNotifyHeartBeat -= CountUp;
    }
}

using Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HVRConnector : MonoBehaviour
{
    [SerializeField]
    private Graph _graph;


    private void Awake()
    {
        HeartBeatWindow.OnNotifyHeartBeat += CountUp;
    }



    private void CountUp(DateTime time, int value)
    {
        var data = _graph.data;

        var newData = new int[data.Length + 1];

        for (var i = 0; i != data.Length; i++)
        {
            newData[i] = data[i];
        }

        newData[data.Length] = value;

        _graph.data = newData;
    }

    private void OnDestroy()
    {
        HeartBeatWindow.OnNotifyHeartBeat -= CountUp;
    }
}

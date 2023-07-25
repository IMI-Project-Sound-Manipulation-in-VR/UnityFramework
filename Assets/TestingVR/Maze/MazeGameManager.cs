using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> marbles;

    private readonly List<Vector3> _marbleStartPos;
    private void Start()
    {
        foreach (var marble in marbles)
        {
            _marbleStartPos.Add(marble.transform.position);
        }
    }

    public void ResetGame()
    {
        for (var i = 0; i < marbles.Count; i++)
        {
            marbles[i].transform.position = _marbleStartPos[i];
        }
    }
}

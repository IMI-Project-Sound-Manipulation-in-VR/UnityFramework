using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MazeGameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> marbles;

    private readonly List<Vector3> _marbleStartPos = new List<Vector3>();
    private void Start()
    {
        foreach (var marble in marbles)
        {
            _marbleStartPos.Add(marble.transform.position);
        }
    }

    public async Task ResetGame()
    {
        for (var i = 0; i < marbles.Count; i++)
        {
            await Task.Delay(100);
            marbles[i].transform.position = _marbleStartPos[i];
        }
    }
}

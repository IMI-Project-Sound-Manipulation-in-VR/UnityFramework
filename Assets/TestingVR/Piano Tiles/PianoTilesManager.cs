using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PianoTilesManager : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSpeed;
    [SerializeField] private float speedIncreasedBy;
    
    [SerializeField] private float maxDelay;
    [SerializeField] private float minDelay;
    [SerializeField] private float startMinDelay;
    [SerializeField] private float delayDecreasedBy;

    [SerializeField] private int restartDelay = 1;
    
    [SerializeField] private GameObject key;
    [SerializeField] private List<Transform> spawnPoints;

    private bool gameOver;
    private float speed = 1;
    private float delay;
    private float currMaxDelay;
    private float currMinDelay;
    private int score = 0;
    
    // Start is called before the first frame update
    private void Start()
    {
        speed = minSpeed;
        currMaxDelay = maxDelay;
        currMinDelay = startMinDelay;
    }

    public void GotTap()
    {
        score++;

        if (score == 4)
            StartCoroutine(SpawnKeys());
    }

    public void GameOver()
    {
        if(gameOver) return;
        
        gameOver = true;

        var keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in keys)
        {
            key.GetComponent<KeyBehavior>().SetSpeed(0);
            Destroy(key, restartDelay);
        }
        
        StartCoroutine(ResetAll());
    }

    private IEnumerator SpawnKeys()
    {
        while (!gameOver && score > 3)
        {
            delay = Random.Range(currMinDelay, currMaxDelay);

            var randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            var keyObject = Instantiate(key, randomSpawnPoint.position, Quaternion.identity);
            keyObject.GetComponent<KeyBehavior>().SetSpeed(speed);

            if (speed < maxSpeed)
                speed += speedIncreasedBy;

            if (currMinDelay > minDelay)
            {
                currMaxDelay -= delayDecreasedBy;
                currMinDelay -= delayDecreasedBy;
            }
            
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator ResetAll()
    {
        yield return new WaitForSeconds(restartDelay);

        score = 0;
        gameOver = false;
        currMaxDelay = maxDelay;
        currMinDelay = startMinDelay;
        speed = minSpeed;

        foreach (var spawnPoint in spawnPoints)
        {
            var startSpawnPoint = new Vector3(spawnPoint.position.x, spawnPoint.position.y + .025f, 0);
            var startKey = Instantiate(key, startSpawnPoint, Quaternion.identity);
            startKey.GetComponent<KeyBehavior>().SetSpeed(0);
        }
    }
}

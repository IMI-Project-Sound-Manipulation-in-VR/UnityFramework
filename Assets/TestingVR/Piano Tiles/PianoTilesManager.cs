using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

public class PianoTilesManager : MonoBehaviour
{
    public int level = 0;
    public float playTime;

    [SerializeField] private float timeLimit = 60;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSpeed;
    [SerializeField] private float speedIncreasedBy;
    
    [SerializeField] private float maxDelay;
    [SerializeField] private float minDelay;
    [SerializeField] private float startMinDelay;
    
    [SerializeField] private float resetBy = 0.5f;
    
    [SerializeField] private int restartDelay = 1;
    
    [SerializeField] private GameObject key;
    [SerializeField] private Text levelText;
    [SerializeField] private List<Transform> spawnPoints;

    private bool gameOver;
    private bool gameStarted;
    private float speed = 1;
    private float delay;
    private float currMaxDelay;
    private float currMinDelay;
    private int score;
    
    // Start is called before the first frame update
    private void Start()
    {
        speed = minSpeed;
        currMaxDelay = maxDelay;
        currMinDelay = startMinDelay;
    }

    private void Update()
    {
        if (gameStarted)
            playTime += Time.deltaTime;

        if (playTime >= timeLimit)
            GameOver();
    }

    public void GotTap()
    {
        score++;

        if (score == 4)
            StartCoroutine(SpawnKeys());
    }

    public void FailedTap()
    {
        level -= Convert.ToInt32(resetBy / speedIncreasedBy);
        level = Math.Max(0, level);
        UpdateLevel();
        
        speed -= resetBy;
        speed = Math.Max(minSpeed, speed);
        
        currMaxDelay += resetBy;
        currMaxDelay = Math.Min(maxDelay, currMaxDelay);
        
        currMinDelay += resetBy;
        currMinDelay = Math.Min(startMinDelay, currMinDelay);

        var keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in keys)
        {
            key.GetComponent<KeyBehavior>().SetSpeed(speed);
        }
    }

    private void GameOver()
    {
        if(gameOver) return;
        
        gameOver = true;
        gameStarted = false;
        playTime = 0;
        level = 0;

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
        gameStarted = true;
        while (!gameOver && score > 3)
        {
            level++;
            UpdateLevel();
            
            delay = Random.Range(currMinDelay, currMaxDelay);

            var spawnPos = Random.Range(0, spawnPoints.Count);
            var randomSpawnPoint = spawnPoints[spawnPos];

            var keyObject = Instantiate(key, randomSpawnPoint.position, randomSpawnPoint.rotation);
            var keyBehavior = keyObject.GetComponent<KeyBehavior>();
            keyBehavior.SetSpeed(speed);
            keyBehavior.SetSound(spawnPos);

            if (speed <= maxSpeed)
                speed += speedIncreasedBy;

            if (currMinDelay >= minDelay)
            {
                currMaxDelay -= speedIncreasedBy;
                currMinDelay -= speedIncreasedBy;
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

        for (var i = 0; i < spawnPoints.Count; i++)
        {
            var startSpawnPoint = new Vector3(spawnPoints[i].position.x, spawnPoints[i].position.y + .025f, 0);
            var startKey = Instantiate(key, startSpawnPoint, Quaternion.identity);
            var keyBehavior = startKey.GetComponent<KeyBehavior>();
            keyBehavior.SetSpeed(0);
            keyBehavior.SetSound(i);
        }
    }

    private void UpdateLevel()
    {
        levelText.text = level.ToString();
    }
}

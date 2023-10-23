using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PianoTilesManager : MonoBehaviour
{
    public int score = 0;
    public int highScore = 0;
    
    [SerializeField] private float timeLimit = 60;
    [SerializeField] private float playTime;
    
    [SerializeField] private float startSpeed = 1;
    
    [SerializeField] private float startMaxDelay;
    [SerializeField] private float startMinDelay;
    
    [SerializeField] private float increasedBy;

    [SerializeField] private GameObject key;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Transform startKeys;
    [SerializeField] private List<Transform> spawnPoints;

    private float _currSpeed;
    private float _currMaxDelay;
    private float _currMinDelay;
    private int _taps;
    
    // Reset all keys by start
    private void Start()
    {
        ResetAll(0);
    }

    // invoked when a key got tapped
    public void GotTap()
    {
        _taps++;
        SetValues();

        // the first four keys doesn't count
        if (_taps > 4)
        {
            score++;
            UpdateScore();
        }
        
        // set the current highscore if its beaten
        if (highScore < score)
        {
            highScore = score;
            UpdateHighScore();
        }
        
        // starts the game after four taps (the first four keys on the piano)
        if (_taps == 4)
            StartCoroutine(SpawnKeys());
    }

    // invoked when the player couldn't tap a key on time
    public void FailedTap()
    {
        // score and speed decreases
        score--;
        score = Math.Max(0, score);
        UpdateScore();
        
        SetValues();

        // speed of the keys already spawned decreases
        var keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in keys)
        {
            key.GetComponent<KeyBehavior>().SetSpeed(_currSpeed);
        }
    }

    // restarts the game by stopping and destroying all keys after a delay
    public void RestartGame(int delay)
    {
        if(_taps == 0) return;

        var keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in keys)
        {
            key.GetComponent<KeyBehavior>().SetSpeed(0);
            Destroy(key, delay);
        }

        ResetAll(delay);
    }

    // Coroutine to Spawn the keys with a delay
    private IEnumerator SpawnKeys()
    {
        while (_taps > 3)
        {
            // get a random delay depend on the current score
            var currDelay = Random.Range(_currMinDelay, _currMaxDelay);

            // get a random position to spawn
            var spawnPos = Random.Range(0, spawnPoints.Count);
            var randomSpawnPoint = spawnPoints[spawnPos];

            // Instantiate key on the random position and set his speed and random sound
            var keyObject = Instantiate(key, randomSpawnPoint.position, randomSpawnPoint.rotation, randomSpawnPoint);
            var keyBehavior = keyObject.GetComponent<KeyBehavior>();
            keyBehavior.SetSpeed(_currSpeed);
            keyBehavior.SetSound(spawnPos);
            
            // delay between two keys to spawn
            yield return new WaitForSeconds(currDelay);
        }
    }
    
    // reset all values of the game to his startvalues
    private async Task ResetAll(int delay)
    {
        await Task.Delay(delay);

        _taps = 0;
        playTime = 0;
        score = 0;
        UpdateScore();
        _currMaxDelay = startMaxDelay;
        _currMinDelay = startMinDelay;
        _currSpeed = startSpeed;

        // spawn the startkeys again on the piano
        for (var i = 0; i < spawnPoints.Count; i++)
        {
            var startSpawnPoint = new Vector3(0, spawnPoints[i].position.y + .025f, spawnPoints[i].position.z);
            var startKey = Instantiate(key, startSpawnPoint, spawnPoints[i].rotation, startKeys);
            
            var keyBehavior = startKey.GetComponent<KeyBehavior>();
            keyBehavior.SetSpeed(0);
            keyBehavior.SetSound(i);
        }
    }

    // sets the values of the game depending on the current score
    private void SetValues()
    {
        _currSpeed = startSpeed + (score * increasedBy);
        _currMaxDelay = startMaxDelay - (score * increasedBy);
        _currMinDelay = startMinDelay -  (score * increasedBy);
        _currSpeed = Math.Min(_currSpeed, 3.8f);
        _currMaxDelay = Math.Max(_currMaxDelay, .7f);
        _currMinDelay = Math.Max(_currMinDelay, .2f);
    }

    // update score on UI
    private void UpdateScore()
    {
        scoreText.text = score.ToString();
    }
    
    // update highscore on UI
    private void UpdateHighScore()
    {
        highScoreText.text = highScore.ToString();
    }
}

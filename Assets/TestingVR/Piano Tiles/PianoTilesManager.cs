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
    
    private void Start()
    {
        ResetAll(0);
    }

    /* private void Update()
    {
        if (_taps > 3)
            playTime += Time.deltaTime;

        if (playTime >= timeLimit)
            RestartGame(restartDelay);
    }*/

    public void GotTap()
    {
        _taps++;
        SetValues();

        if (_taps > 4)
        {
            score++;
            UpdateScore();
        }
        
        if (highScore < score)
        {
            highScore = score;
            UpdateHighScore();
        }
        
        if (_taps == 4)
            StartCoroutine(SpawnKeys());
    }

    public void FailedTap()
    {
        score--;
        score = Math.Max(0, score);
        UpdateScore();
        
        SetValues();

        var keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in keys)
        {
            key.GetComponent<KeyBehavior>().SetSpeed(_currSpeed);
        }
    }

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

    private IEnumerator SpawnKeys()
    {
        while (_taps > 3)
        {
            var currDelay = Random.Range(_currMinDelay, _currMaxDelay);

            var spawnPos = Random.Range(0, spawnPoints.Count);
            var randomSpawnPoint = spawnPoints[spawnPos];

            var keyObject = Instantiate(key, randomSpawnPoint.position, randomSpawnPoint.rotation, randomSpawnPoint);
            var keyBehavior = keyObject.GetComponent<KeyBehavior>();
            keyBehavior.SetSpeed(_currSpeed);
            keyBehavior.SetSound(spawnPos);

            yield return new WaitForSeconds(currDelay);
        }
    }

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

        for (var i = 0; i < spawnPoints.Count; i++)
        {
            var startSpawnPoint = new Vector3(0, spawnPoints[i].position.y + .025f, spawnPoints[i].position.z);
            var startKey = Instantiate(key, startSpawnPoint, spawnPoints[i].rotation, startKeys);
            
            var keyBehavior = startKey.GetComponent<KeyBehavior>();
            keyBehavior.SetSpeed(0);
            keyBehavior.SetSound(i);
        }
    }

    private void SetValues()
    {
        _currSpeed = startSpeed + (score * increasedBy);
        _currMaxDelay = startMaxDelay - (score * increasedBy);
        _currMinDelay = startMinDelay -  (score * increasedBy);
        _currSpeed = Math.Min(_currSpeed, 3.8f);
        _currMaxDelay = Math.Max(_currMaxDelay, .7f);
        _currMinDelay = Math.Max(_currMinDelay, .2f);
    }

    private void UpdateScore()
    {
        scoreText.text = score.ToString();
    }
    
    private void UpdateHighScore()
    {
        highScoreText.text = highScore.ToString();
    }
}

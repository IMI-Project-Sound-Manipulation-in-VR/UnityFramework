using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
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
    [SerializeField] private Transform startKeys;
    [SerializeField] private List<Transform> spawnPoints;

    private float _speed = 1;
    private float _delay;
    private float _currMaxDelay;
    private float _currMinDelay;
    private int _score;
    
    private void Start()
    {
        ResetAll(0);
    }

    private void Update()
    {
        if (_score > 3)
            playTime += Time.deltaTime;

        if (playTime >= timeLimit)
            RestartGame(restartDelay);
    }

    public void GotTap()
    {
        _score++;

        if (_score == 4)
            SpawnKeys();
    }

    public void FailedTap()
    {
        level -= Convert.ToInt32(resetBy / speedIncreasedBy);
        level = Math.Max(0, level);
        UpdateLevel();
        
        _speed -= resetBy;
        _speed = Math.Max(minSpeed, _speed);
        
        _currMaxDelay += resetBy;
        _currMaxDelay = Math.Min(maxDelay, _currMaxDelay);
        
        _currMinDelay += resetBy;
        _currMinDelay = Math.Min(startMinDelay, _currMinDelay);

        var keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in keys)
        {
            key.GetComponent<KeyBehavior>().SetSpeed(_speed);
        }
    }

    public void RestartGame(int delay)
    {
        if(_score == 0) return;
        
        playTime = 0;
        level = 0;

        var keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in keys)
        {
            key.GetComponent<KeyBehavior>().SetSpeed(0);
            Destroy(key, delay);
        }

        ResetAll(delay);
    }

    private async Task SpawnKeys()
    {
        while (_score > 3)
        {
            level++;
            UpdateLevel();
            
            _delay = Random.Range(_currMinDelay, _currMaxDelay);

            var spawnPos = Random.Range(0, spawnPoints.Count);
            var randomSpawnPoint = spawnPoints[spawnPos];

            var keyObject = Instantiate(key, randomSpawnPoint.position, randomSpawnPoint.rotation, randomSpawnPoint);
            var keyBehavior = keyObject.GetComponent<KeyBehavior>();
            keyBehavior.SetSpeed(_speed);
            keyBehavior.SetSound(spawnPos);

            if (_speed <= maxSpeed)
                _speed += speedIncreasedBy;

            if (_currMinDelay >= minDelay)
            {
                _currMaxDelay -= speedIncreasedBy;
                _currMinDelay -= speedIncreasedBy;
            }
            
            await Task.Delay(Convert.ToInt32(_delay * 1000));
        }
    }

    private async Task ResetAll(int delay)
    {
        await Task.Delay(delay);

        _score = 0;
        _currMaxDelay = maxDelay;
        _currMinDelay = startMinDelay;
        _speed = minSpeed;

        for (var i = 0; i < spawnPoints.Count; i++)
        {
            var startSpawnPoint = new Vector3(0, spawnPoints[i].position.y + .025f, spawnPoints[i].position.z);
            var startKey = Instantiate(key, startSpawnPoint, spawnPoints[i].rotation, startKeys);
            
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

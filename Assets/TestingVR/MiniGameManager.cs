using System;
using System.Collections;
using System.Collections.Generic;
using TestingVR.Tower_of_Hanoi;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    [SerializeField] 
    private MiniGame startMiniGame;
    
    [SerializeField] 
    private GameObject towerOfHanoi;
    
    [SerializeField] 
    private GameObject mazeGame;
    
    [SerializeField] 
    private GameObject pianoTiles;
    
    private MiniGame _currentMiniGame;

    private PianoTilesManager _pianoTilesManager;
    private TowerOfHanoiManager _towerOfHanoiManager;
        
    // Start is called before the first frame update
    private void Start()
    {
        _pianoTilesManager = pianoTiles.GetComponent<PianoTilesManager>();
        _towerOfHanoiManager = towerOfHanoi.GetComponent<TowerOfHanoiManager>();
        
        switch (startMiniGame)
        {
            case MiniGame.TowerOfHanoi: EnableTowerOfHanoi(); break;
            case MiniGame.MazeGame: EnableMazeGame(); break;
            case MiniGame.PianoTiles: EnablePianoTiles(); break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SwitchToNextGame();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SwitchToPreviousGame();
        }
    }

    private void SwitchToNextGame()
    {
        switch (_currentMiniGame)
        {
            case MiniGame.TowerOfHanoi: EnableMazeGame(); break;
            case MiniGame.MazeGame: EnablePianoTiles(); break;
            case MiniGame.PianoTiles: EnableTowerOfHanoi(); break;
        }
    }
    
    private void SwitchToPreviousGame()
    {
        switch (_currentMiniGame)
        {
            case MiniGame.TowerOfHanoi: EnablePianoTiles(); break;
            case MiniGame.MazeGame: EnableTowerOfHanoi(); break;
            case MiniGame.PianoTiles: EnableMazeGame(); break;
        }
    }

    private void EnableTowerOfHanoi()
    {
        _currentMiniGame = MiniGame.TowerOfHanoi;
        
        towerOfHanoi.SetActive(true);
        mazeGame.SetActive(false);
        pianoTiles.SetActive(false);
        
        _towerOfHanoiManager.SpawnSlices();
    }

    private void EnableMazeGame()
    {
        _currentMiniGame = MiniGame.MazeGame;
        
        towerOfHanoi.SetActive(false);
        mazeGame.SetActive(true);
        pianoTiles.SetActive(false);
    }

    private void EnablePianoTiles()
    {
        _currentMiniGame = MiniGame.PianoTiles;

        towerOfHanoi.SetActive(false);
        mazeGame.SetActive(false);
        pianoTiles.SetActive(true);
        
        _pianoTilesManager.RestartGame(0);
    }
}

internal enum MiniGame
{
    TowerOfHanoi,
    MazeGame,
    PianoTiles
}
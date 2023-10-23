using System;
using System.Collections;
using System.Collections.Generic;
using TestingVR.Tower_of_Hanoi;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static event Action OnNotifyMiniGameChange;

    [SerializeField] 
    private MiniGame startMiniGame;
    
    [SerializeField] 
    private GameObject towerOfHanoi;
    
    [SerializeField] 
    private GameObject mazeGame;
    
    [SerializeField] 
    private GameObject pianoTiles;
    
    private MiniGame _currentMiniGame;

    private TowerOfHanoiManager _towerOfHanoiManager;
    private MazeGameManager _mazeGameManager;
    private PianoTilesManager _pianoTilesManager;
        
    // Start is called before the first frame update
    private void Start()
    {
        _towerOfHanoiManager = towerOfHanoi.GetComponent<TowerOfHanoiManager>();
        _mazeGameManager = mazeGame.GetComponent<MazeGameManager>();
        _pianoTilesManager = pianoTiles.GetComponent<PianoTilesManager>();

        // Enable the one which ist chosen in startMiniGame
        switch (startMiniGame)
        {
            case MiniGame.TowerOfHanoi: EnableTowerOfHanoi(); break;
            case MiniGame.MazeGame: EnableMazeGame(); break;
            case MiniGame.PianoTiles: EnablePianoTiles(); break;
        }
    }

    private void Update()
    {
        // Use Arrow Keys to switch between the minigames
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SwitchToNextGame();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SwitchToPreviousGame();
        }
    }

    // Enables the next minigame from the current minigame (order: Tower of Hanoi -> Maze Game -> Piano Tiles -> Tower of Hanoi)
    private void SwitchToNextGame()
    {
        switch (_currentMiniGame)
        {
            case MiniGame.TowerOfHanoi: EnableMazeGame(); break;
            case MiniGame.MazeGame: EnablePianoTiles(); break;
            case MiniGame.PianoTiles: EnableTowerOfHanoi(); break;
        }
        OnNotifyMiniGameChange?.Invoke();
    }
    
    // Enables the previous minigame from the current minigame (order: Tower of Hanoi -> Piano Tiles -> Maze Game -> Tower of Hanoi)
    private void SwitchToPreviousGame()
    {
        switch (_currentMiniGame)
        {
            case MiniGame.TowerOfHanoi: EnablePianoTiles(); break;
            case MiniGame.MazeGame: EnableTowerOfHanoi(); break;
            case MiniGame.PianoTiles: EnableMazeGame(); break;
        }
        OnNotifyMiniGameChange?.Invoke();
    }

    // Enables Tower of Hanoi minigame by activating it and deactivation the others
    private void EnableTowerOfHanoi()
    {
        _currentMiniGame = MiniGame.TowerOfHanoi;
        
        towerOfHanoi.SetActive(true);
        mazeGame.SetActive(false);
        pianoTiles.SetActive(false);
        
        // Starts the game
        _towerOfHanoiManager.SpawnSlices();
    }

    // Enables maze minigame by activating it and deactivation the others
    private void EnableMazeGame()
    {
        _currentMiniGame = MiniGame.MazeGame;
        
        towerOfHanoi.SetActive(false);
        mazeGame.SetActive(true);
        pianoTiles.SetActive(false);
        
        // Starts the game
        _mazeGameManager.ResetGame();
    }

    // Enables piano tiles minigame by activating it and deactivation the others
    private void EnablePianoTiles()
    {
        _currentMiniGame = MiniGame.PianoTiles;

        towerOfHanoi.SetActive(false);
        mazeGame.SetActive(false);
        pianoTiles.SetActive(true);
        
        // Starts the game
        _pianoTilesManager.RestartGame(0);
    }
}

// Enum for the three minigame Types
internal enum MiniGame
{
    TowerOfHanoi,
    MazeGame,
    PianoTiles
}
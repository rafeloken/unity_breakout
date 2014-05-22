using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameManager : UnitySingleton<GameManager> {
    public enum State { Initialize, MainMenu, SetupNewGame, Game, GameOver, Quit }
    public enum GameState { Idle, Playing, Dead, NextStage }

    public FiniteStateMachine<State> mainFSM;
    public FiniteStateMachine<GameState> gameFSM;

    public event EventHandler SettingUpNewGame;
    public event EventHandler GameStarted;
    public event EventHandler StartPlaying;
    public event EventHandler PlayerDied;
    public event EventHandler PlayerRespawn;
    public event EventHandler NextStage;
    public event EventHandler GameIsOver;
    public event EventHandler BackToMainMenu;

    LevelBuilder levelBuilder;

    Player player;
    Score score;

    int highScore;
    GUIText highScoreText;

    GUIText startMessage;
    GUIText respawnMessage;
    GUIText nextStageMessage;
    GUIText gameOverMessage;
    GUIText gameOverActionMessage;

    bool isReadyToExit = false;

	protected override void Init() {
        // We want this class to persist through new scenes being loaded.
        Persist = true;

        mainFSM = new FiniteStateMachine<State>();        
        mainFSM.AddTransition(State.Initialize, State.MainMenu, null, () => Application.LoadLevel("main_menu"), null);
        mainFSM.AddTransition(State.MainMenu, State.SetupNewGame, null, InitializeNewGame, OnSettingUpNewGame);
        mainFSM.AddTransition(State.SetupNewGame, State.Game, null, () => StartCoroutine(InitiateGameLoop()), OnGameStarted);
        mainFSM.AddTransition(State.Game, State.GameOver, SaveStats, null, OnGameIsOver);
        mainFSM.AddTransition(State.GameOver, State.MainMenu, null, ReturnToMainMenu, OnBackToMainMenu);
        mainFSM.StateChanged += (object s, EventArgs e) => {
            Debug.Log("state: " + mainFSM.CurrentState.ToString() + " | game state: " + gameFSM.CurrentState.ToString());
        };

        gameFSM = new FiniteStateMachine<GameState>();
        gameFSM.AddTransition(GameState.Idle, GameState.Playing, null, null, OnStartPlaying);
        gameFSM.AddTransition(GameState.Playing, GameState.NextStage, null, null, OnNextStage);
        gameFSM.AddTransition(GameState.Playing, GameState.Dead, null, null, OnPlayerDied);
        gameFSM.AddTransition(GameState.NextStage, GameState.Idle, PrepareNextStage, null, null);
        gameFSM.AddTransition(GameState.Dead, GameState.Playing, null, null, OnPlayerRespawn);
        gameFSM.AddTransition(GameState.Dead, GameState.Idle, null, null, null);        
        gameFSM.StateChanged += (object s, EventArgs e) => {
            Debug.Log("state: " + mainFSM.CurrentState.ToString() + " | game state: " + gameFSM.CurrentState.ToString());
        };

        mainFSM.ChangeState(State.MainMenu);
	}

    IEnumerator Start() {
        while(true) {
            if(mainFSM.CurrentState == State.SetupNewGame && !Application.isLoadingLevel) {
                isReadyToExit = false;                
                levelBuilder = GameObject.FindGameObjectWithTag("Level").GetComponent<LevelBuilder>();
                player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
                score = GameObject.Find("score").GetComponent<Score>();
                Score.ResetScore();
                score.UpdateScore();
                GUIText[] texts = GameObject.Find("UI").GetComponentsInChildren<GUIText>(true);
                foreach(var t in texts) {
                    if(t.name == "high score") {
                        highScoreText = t;
                    } else if(t.name == "start message") {
                        startMessage = t;
                    } else if(t.name == "respawn message") {
                        respawnMessage = t;
                    } else if(t.name == "next stage message") {
                        nextStageMessage = t;
                    } else if(t.name == "gameover message") {
                        gameOverMessage = t;
                    } else if(t.name == "gameover action message") {
                        gameOverActionMessage = t;
                    }
                }
                LoadStats();

                mainFSM.ChangeState(State.Game);
                if(gameFSM.CurrentState != GameState.Idle)
                    gameFSM.ChangeState(GameState.Idle);
            }
            
            yield return null;
        }
    }

    IEnumerator InitiateGameLoop() {
        while(true) {
            switch(mainFSM.CurrentState) {
                case State.Game:
                    InGame();

                    if(gameFSM.CurrentState == GameState.NextStage) {
                        DisplayText(nextStageMessage);
                        yield return new WaitForSeconds(4);
                        nextStageMessage.gameObject.SetActive(false);
                        gameFSM.ChangeState(GameState.Idle);
                        DisplayText(startMessage);
                    }

                    break;
                case State.GameOver:
                    DisplayText(gameOverMessage);
                    // If player stats is done saving, delay allowing the exit to main menu.
                    if(!gameOverActionMessage.gameObject.activeSelf && isReadyToExit) {
                        yield return new WaitForSeconds(3);
                        DisplayText(gameOverActionMessage);
                    }
                    // Only allow exit after player stats has been saved.
                    if(isReadyToExit && Input.GetButtonDown("Exit")) {
                        mainFSM.ChangeState(State.MainMenu);
                    }
                    break;
                default:
                    break;
            }            
            yield return null;
        }
    }

    void InGame() {
        switch(gameFSM.CurrentState) {
            case GameState.Idle:
                if(Input.GetButtonDown("Jump")) {
                    startMessage.gameObject.SetActive(false);
                    gameFSM.ChangeState(GameState.Playing);
                }
                break;
            case GameState.Playing:
                player.UpdatePlayer();
                break;
            case GameState.NextStage:                
                break;
            case GameState.Dead:
                if(Input.GetButtonDown("Jump")) {
                    respawnMessage.gameObject.SetActive(false);
                    gameFSM.ChangeState(GameState.Playing);
                }
                break;
            default:
                break;
        }
    }

    void InitializeNewGame() {
        Application.LoadLevel("game");
    }

    void PrepareNextStage() {
        levelBuilder.GenerateLevel(12, 18);
    }

    void LoadStats() {
        if(File.Exists(Application.persistentDataPath + "/playerStats.dat")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerStats.dat", FileMode.Open);
            PlayerStats stats = (PlayerStats)bf.Deserialize(file);
            file.Close();
            highScore = stats.highScore;
            highScoreText.text = "High Score: " + highScore;
        } else {
            highScore = 0;
            highScoreText.text = "High Score: " + highScore;
        }
    }

    void SaveStats() {
        if(Score.CurrentScore > highScore) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerStats.dat", FileMode.OpenOrCreate);
            PlayerStats stats = new PlayerStats();
            // TODO: Announce that player got a new high score.
            stats.highScore = highScore = Score.CurrentScore;
            bf.Serialize(file, stats);
            file.Close();
        }
        isReadyToExit = true;
    }

    void ReturnToMainMenu() {
        Application.LoadLevel("main_menu");
    }

    void DisplayText(GUIText gt) {
        if(!gt.gameObject.activeSelf)
            gt.gameObject.SetActive(true);
    }

    void OnSettingUpNewGame(EventArgs e) {
        var settingUpNewGame = SettingUpNewGame;
        if(settingUpNewGame != null)
            settingUpNewGame(this, e);
    }

    void OnGameStarted(EventArgs e) {
        DisplayText(startMessage);

        var gameStarted = GameStarted;
        if(gameStarted != null)
            gameStarted(this, e);
    }

    void OnStartPlaying(EventArgs e) {
        var startPlaying = StartPlaying;
        if(startPlaying != null)
            startPlaying(this, e);
    }

    void OnNextStage(EventArgs e) {
        var nextStage = NextStage;
        if(nextStage != null)
            nextStage(this, e);
    }

    void OnPlayerDied(EventArgs e) {
        if(player.NumLives > 0) {
            DisplayText(respawnMessage);
        } else {
            DisplayText(gameOverMessage);
        }

        var playerDied = PlayerDied;
        if(playerDied != null)
            playerDied(this, e);
    }

    void OnPlayerRespawn(EventArgs e) {
        var playerRespawn = PlayerRespawn;
        if(playerRespawn != null)
            playerRespawn(this, e);
    }

    void OnGameIsOver(EventArgs e) {
        var gameOver = GameIsOver;
        if(gameOver != null)
            gameOver(this, e);
    }

    void OnBackToMainMenu(EventArgs e) {
        SettingUpNewGame = null;
        PlayerDied = null;
        PlayerRespawn = null;
        NextStage = null;
        StartPlaying = null;

        var backToMainMenu = BackToMainMenu;
        if(backToMainMenu != null)
            backToMainMenu(this, e);
    }
}

// Simple object used for saving/loading player stats to/from a file.
[Serializable]
class PlayerStats {
    public int highScore;
}

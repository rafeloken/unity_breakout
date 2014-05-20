using UnityEngine;
using System;
using System.Collections;

public class GameManager : UnitySingleton<GameManager> {
    public enum State { Initialize, MainMenu, SetupNewGame, Game, GameOver, Restart, Quit }
    public enum GameState { Playing, Dead, NextStage }

    public FiniteStateMachine<State> mainFSM;
    public FiniteStateMachine<GameState> gameFSM;

    public event EventHandler SettingUpNewGame;
    public event EventHandler GameStarted;
    public event EventHandler PlayerDied;
    public event EventHandler PlayerRespawn;
    public event EventHandler GameIsOver;
    public event EventHandler GameRestarted;

    Player player;

	protected override void Init() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        mainFSM = new FiniteStateMachine<State>();        
        mainFSM.AddTransition(State.Initialize, State.MainMenu, null, null, null);
        mainFSM.AddTransition(State.MainMenu, State.SetupNewGame, null, InitializeNewGame, OnSettingUpNewGame);
        mainFSM.AddTransition(State.SetupNewGame, State.Game, null, () => StartCoroutine(InitiateGameLoop()), OnGameStarted);
        mainFSM.AddTransition(State.Game, State.GameOver, OnGameIsOver);
        mainFSM.AddTransition(State.GameOver, State.Restart, null);
        mainFSM.AddTransition(State.Restart, State.SetupNewGame, null, InitializeNewGame, null);
        mainFSM.AddTransition(State.Restart, State.Quit, null);
        mainFSM.StateChanged += (object s, EventArgs e) => {
            Debug.Log("state: " + mainFSM.CurrentState.ToString() + " | game state: " + gameFSM.CurrentState.ToString());
        };

        gameFSM = new FiniteStateMachine<GameState>();
        gameFSM.AddTransition(GameState.Playing, GameState.NextStage, null, null, null);
        gameFSM.AddTransition(GameState.Playing, GameState.Dead, null, null, OnPlayerDied);
        gameFSM.AddTransition(GameState.Dead, GameState.Playing, null, null, OnPlayerRespawn);
        gameFSM.AddTransition(GameState.NextStage, GameState.Playing, null, null, null);
        gameFSM.StateChanged += (object s, EventArgs e) => {
            Debug.Log("state: " + mainFSM.CurrentState.ToString() + " | game state: " + gameFSM.CurrentState.ToString());
        };

        GameIsOver += (object s, EventArgs e) => { Debug.Log("oh no!"); };

        mainFSM.ChangeState(State.MainMenu);
	}

    // TODO: Determine if this is needed anymore.
    IEnumerator Start() {
        // Simulate menu selection.
        yield return new WaitForSeconds(2);
        while(true) {
            // New Game Selected
            if(mainFSM.CurrentState == State.MainMenu) {
                mainFSM.ChangeState(State.SetupNewGame);
            }
            yield return null;
        }
    }

    IEnumerator InitiateGameLoop() {
        print("initiating game loop");
        while(true) {
            if(mainFSM.CurrentState == State.GameOver) {
                // Delay allowing restart.
                yield return new WaitForSeconds(5);
                mainFSM.ChangeState(State.Restart);
                break;
            }
            if(mainFSM.CurrentState == State.Game) {
                InGame();

                if(Input.GetKeyDown(KeyCode.R)) {
                    gameFSM.ChangeState(GameState.Playing);
                }
            }
            yield return null;
        }
    }

	void Update() {        
        if(mainFSM.CurrentState == State.Restart) {
            if(Input.GetKeyDown(KeyCode.Return)) {
                mainFSM.ChangeState(State.SetupNewGame);
            }
            if(Input.GetKeyDown(KeyCode.Q)) {
                mainFSM.ChangeState(State.Quit);
            }
        }
	}

    void InGame() {
        switch(gameFSM.CurrentState) {
            case GameState.Playing:
                player.UpdatePlayer();
                break;
            case GameState.Dead:                
                break;
            case GameState.NextStage:
                break;
        }
    }

    void InitializeNewGame() {
        mainFSM.ChangeState(State.Game);
        if(gameFSM.CurrentState != GameState.Playing)
            gameFSM.ChangeState(GameState.Playing);        
    }

    void OnSettingUpNewGame(EventArgs e) {
        var settingUpNewGame = SettingUpNewGame;
        if(settingUpNewGame != null)
            settingUpNewGame(this, e);
    }

    void OnGameStarted(EventArgs e) {
        var gameStarted = GameStarted;
        if(gameStarted != null)
            gameStarted(this, e);
    }

    void OnPlayerDied(EventArgs e) {
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
}

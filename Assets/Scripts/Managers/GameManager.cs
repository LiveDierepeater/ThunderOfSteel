using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void ChangedGameState(GameState newGameState);
    public ChangedGameState OnChangedGameState;
    
    public enum GameState
    {
        MainMenu,
        PauseMenu,
        InGame
    }
    public GameState State;

    private void Awake()
    {
        State = GameState.MainMenu;
    }

    private void Start()
    {
        InputManager.Instance.SetGameManager(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) ChangeGameState(GameState.InGame);
    }

    private void ChangeGameState(GameState newState)
    {
        State = newState;
        OnChangedGameState?.Invoke(State);
    }
}

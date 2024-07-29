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
        DontDestroyOnLoad(transform.root.gameObject);
        State = GameState.InGame;
    }

    private void Start()
    {
        InputManager.Instance.SetGameManager(this);
        Debug.developerConsoleVisible = true;
    }

    // ReSharper disable once UnusedMember.Local
    private void ChangeGameState(GameState newState)
    {
        State = newState;
        OnChangedGameState?.Invoke(State);
    }
}

using System;
using UnityEngine;

public class InputManager : InputReader
{
    private static InputManager _instance;
    private static GameManager _gameManager;

    private InGamePlayerController _inGamePlayerController;

    private GameManager.GameState _gameState;

    public static InputManager Instance => _instance;

    public Player Player;

    protected override void Awake()
    {
        base.Awake();
        if (_instance is not null)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject.transform.root);
        
        Player = Instantiate(Player);
        InitializeAllPlayerControllers();
    }

    private void Start()
    {
        _gameManager.OnChangedGameState += ChangePlayerController;
    }

    private void ChangePlayerController(GameManager.GameState newGameState)
    {
        _gameState = newGameState;

        switch (_gameState)
        {
            case GameManager.GameState.MainMenu:
                break;
            
            case GameManager.GameState.PauseMenu:
                break;
            
            case GameManager.GameState.InGame:
                ActivatePlayerController(_inGamePlayerController);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void InitializeAllPlayerControllers()
    {
        _inGamePlayerController = Player.gameObject.AddComponent<InGamePlayerController>();
    }

    private void ActivatePlayerController<T>(T playerController)
    {
        if (playerController is InGamePlayerController)
        {
            _inGamePlayerController.SetLayerMaskInfo(Player);
            _inGamePlayerController.enabled = true;

            // TODO: Deactivate all other PlayerController
        }

        _inGamePlayerController.enabled = false;
    }

    private void OnDestroy()
    {
        if(_instance == this) _instance = null;
    }

    public void SetGameManager(GameManager newGameManager)
    {
        _gameManager = newGameManager;
        _gameState = _gameManager.State;
    }
}

using System;
using UnityEngine;

public class InputManager : InputReader
{
    private static InputManager _instance;
    private static GameManager _gameManager;

    private InGamePlayerController _inGamePlayerController;

    private GameManager.GameState _gameState;

    public static InputManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    [HideInInspector] public CameraSystem CameraSystem;
    [HideInInspector] public Player Player;

    public Action<float> OnCameraUpdate;

#region Initializing

    protected override void Awake()
    {
        base.Awake();
        
        if (_instance is not null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        Instance = _instance;
        
        DontDestroyOnLoad(gameObject.transform.root);
        
        Player = Instantiate(Player);
        InitializeAllPlayerControllers();
    }

    private void Start() => ActivatePlayerController(_inGamePlayerController);

#endregion

#region External Called Logic

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

    // ATTENTION: Only Call this Function once!
    public void SetGameManager(GameManager newGameManager)
    {
        _gameManager = newGameManager;
        _gameState = _gameManager.State;
        
        _gameManager.OnChangedGameState += ChangePlayerController;
    }

#endregion

#region Extracted Logic Methods

    private void InitializeAllPlayerControllers() => _inGamePlayerController = Player.gameObject.AddComponent<InGamePlayerController>();

    private void ActivatePlayerController<T>(T playerController)
    {
        if (playerController is InGamePlayerController)
        {
            _inGamePlayerController.SetLayerMaskInfo(Player);
            _inGamePlayerController.enabled = true;

            // TODO: Deactivate all other PlayerController
        }
        else
            _inGamePlayerController.enabled = false;
    }

#endregion

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}

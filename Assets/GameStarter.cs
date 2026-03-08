using UnityEngine;
using TestCardGame.Controller;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private GameController gameController;

    private void Start()
    {
        if (gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }

        if (gameController == null)
        {
            Debug.LogError("GameStarter: GameController was not found in the scene.", this);
            return;
        }

        var ok = gameController.Initialize();
        if (!ok)
        {
            Debug.LogError("GameStarter: GameController initialization failed.", this);
            return;
        }
    }
}

using UnityEngine;
using TestCardGame.Controller;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private HandView handView;

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

        if (handView == null)
        {
            handView = FindFirstObjectByType<HandView>();
        }

        if (handView == null)
        {
            Debug.LogWarning("GameStarter: HandView was not found in the scene.", this);
            return;
        }

        handView.ShowCards(gameController.GetPlayerCards());
    }
}

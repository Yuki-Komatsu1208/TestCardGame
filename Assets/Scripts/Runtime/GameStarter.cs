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
            gameController = FindAnyObjectByType<GameController>();
        }

        if (gameController == null)
        {
            Debug.LogError("GameStarter: GameController was not found in the scene.", this);
            return;
        }

        // If there is an active RunController with a definition, let it handle starting the run and stages
        var runController = FindAnyObjectByType<TestCardGame.Controller.RunController>();
        if (runController != null && runController.RunDefinition != null)
        {
            Debug.Log("GameStarter: RunController detected. Handoff control to RunController.");
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
            handView = FindAnyObjectByType<HandView>();
        }

        if (handView == null)
        {
            Debug.LogWarning("GameStarter: HandView was not found in the scene.", this);
            return;
        }

        handView.ShowCards(gameController.GetPlayerCards());
    }
}

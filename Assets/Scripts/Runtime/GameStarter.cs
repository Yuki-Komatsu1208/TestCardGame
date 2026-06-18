using UnityEngine;
using TestCardGame.Controller;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private HandView handView;

    /// <summary>
    /// シーン起動時にバトルまたはRunを開始する。
    /// </summary>
    private void Start()
    {
        if (gameController == null)
        {
            gameController = FindAnyObjectByType<GameController>();
        }

        if (gameController == null)
        {
            Debug.LogError("GameStarter: シーン内に GameController が見つかりません。", this);
            return;
        }

        // RunControllerが有効な場合は、RunControllerに開始処理を任せる。
        var runController = FindAnyObjectByType<TestCardGame.Controller.RunController>();
        if (runController != null && runController.RunDefinition != null)
        {
            Debug.Log("GameStarter: RunController を検出しました。開始処理を RunController に委譲します。");
            return;
        }

        var ok = gameController.Initialize();
        if (!ok)
        {
            Debug.LogError("GameStarter: GameController の初期化に失敗しました。", this);
            return;
        }

        if (handView == null)
        {
            handView = FindAnyObjectByType<HandView>();
        }

        if (handView == null)
        {
            Debug.LogWarning("GameStarter: シーン内に HandView が見つかりません。", this);
            return;
        }

        handView.ShowCards(gameController.GetPlayerCards());
    }
}

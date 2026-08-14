using UnityEngine;
using TestCardGame.Controller;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private HandView handView;
    [SerializeField] private TestCardGame.Controller.RunController runController;

    /// <summary>
    /// シーン起動時にバトルまたはRunを開始する。
    /// </summary>
    private void Start()
    {
        if (gameController == null)
        {
            Debug.LogError("GameStarter: GameControllerのInspector参照が設定されていません。", this);
            return;
        }

        // RunControllerが有効な場合は、RunControllerに開始処理を任せる。
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
            Debug.LogError("GameStarter: HandViewのInspector参照が設定されていません。", this);
            return;
        }

        handView.ShowCards(gameController.GetPlayerCards());
    }
}

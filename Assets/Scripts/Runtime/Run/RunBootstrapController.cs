using UnityEngine;

namespace TestCardGame.Run
{
    /// <summary>
    /// ビルドの入口シーンで、UIを構築する前にRUNを開始する。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class RunBootstrapController : MonoBehaviour
    {
        [SerializeField] private RunDefinitionSO runDefinition;

        private void Awake()
        {
            var session = RunSession.GetOrCreate();
            if (session.HasActiveRun)
            {
                return;
            }

            if (runDefinition == null)
            {
                Debug.LogError("RunBootstrapController: RunDefinitionSO が設定されていません。", this);
                return;
            }

            session.Begin(runDefinition);
            Debug.Log($"RunBootstrapController: RUNを開始しました。({runDefinition.runName})");
        }
    }
}

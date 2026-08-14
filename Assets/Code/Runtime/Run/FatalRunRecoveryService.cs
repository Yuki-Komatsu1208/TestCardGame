using System;
using TestCardGame.UIs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestCardGame.Run
{
    /// <summary>明示的に致命的と判定された不整合だけを、新規RUNとOutpostへ一元復旧する。</summary>
    public static class FatalRunRecoveryService
    {
        private const string OutpostSceneName = "OutpostScene";
        private static bool recoveryInProgress;

        public static bool IsRecoveryInProgress => recoveryInProgress;

        public static bool TryRecover(RunDefinitionSO definition, string developerContext, Exception exception = null)
        {
            if (recoveryInProgress)
                return false;

            recoveryInProgress = true;
            try
            {
                if (exception != null)
                    Debug.LogException(exception);
                Debug.LogError($"致命的エラーからRUNを復旧します: {developerContext}");
                if (definition == null)
                    throw new InvalidOperationException("新規RUNに必要なRunDefinitionSOがありません。");

                RunSession.GetOrCreate().BeginNewRun(definition);
                PlayerMessageService.PublishForScene(OutpostSceneName,
                    "致命的なエラーが発生しました", PlayerMessageKind.FatalError);
                SceneManager.sceneLoaded += OnRecoverySceneLoaded;
                SceneManager.LoadScene(OutpostSceneName);
                return true;
            }
            catch (Exception recoveryException)
            {
                SceneManager.sceneLoaded -= OnRecoverySceneLoaded;
                Debug.LogException(recoveryException);
                Debug.LogError("致命的エラーからの復旧にも失敗しました。再帰的な復旧は行いません。");
                return false;
            }
        }

        private static void OnRecoverySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnRecoverySceneLoaded;
            recoveryInProgress = false;
        }

#if UNITY_EDITOR
        /// <summary>EditMode検証で多重実行防止状態を初期化する。</summary>
        public static void ResetForValidation()
        {
            SceneManager.sceneLoaded -= OnRecoverySceneLoaded;
            recoveryInProgress = false;
        }
#endif
    }
}

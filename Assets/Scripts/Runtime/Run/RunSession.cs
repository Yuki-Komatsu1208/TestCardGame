using UnityEngine;

namespace TestCardGame.Run
{
    /// <summary>純C#のRUNライフサイクルをScene間で保持するUnityアダプタ。</summary>
    public sealed class RunSession : MonoBehaviour
    {
        private readonly RunLifecycleService lifecycle = new();

        public static RunSession Instance { get; private set; }
        public RunState State => lifecycle.State;
        public RunDefinitionSO Definition => lifecycle.Definition;
        public bool HasActiveRun => lifecycle.HasActiveRun;

        public static RunSession GetOrCreate()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("RunSession");
            Instance = go.AddComponent<RunSession>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        public void Begin(RunDefinitionSO definition)
        {
            lifecycle.Begin(definition);
        }



        public bool OpenTown()
        {
            return lifecycle.OpenTown();
        }

        public bool TryStartNextExpedition()
        {
            return lifecycle.TryStartNextExpedition();
        }

        public bool TrySelectKeystone(KeystoneId keystoneId)
        {
            return lifecycle.TrySelectKeystone(keystoneId);
        }

        public bool TryUseInn(int maxHp, float baseMultiplier, float expeditionMultiplier, out int cost)
        {
            return lifecycle.TryUseInn(maxHp, baseMultiplier, expeditionMultiplier, out cost);
        }

        public void MarkFailed()
        {
            lifecycle.MarkFailed();
        }

        public void MarkCompleted()
        {
            lifecycle.MarkCompleted();
        }
    }
}

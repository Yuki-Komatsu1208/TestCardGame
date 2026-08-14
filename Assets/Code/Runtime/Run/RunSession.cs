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

        /// <summary>終了済みRUNを破棄し、同じ定義から初期状態のRUNを明示的に開始する。</summary>
        public void BeginNewRun(RunDefinitionSO definition)
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

        public OutpostActionResult TryStartNextExpeditionDetailed() => lifecycle.TryStartNextExpeditionDetailed();

        public void EnsureShopInventory(ShopCatalogSO catalog)
        {
            lifecycle.EnsureShopInventory(catalog);
        }

        public bool TryBuyShopCard(ShopCatalogSO catalog, int offerIndex)
        {
            return lifecycle.TryBuyShopCard(catalog, offerIndex);
        }

        public OutpostActionResult TryBuyShopCardDetailed(ShopCatalogSO catalog, int offerIndex) => lifecycle.TryBuyShopCardDetailed(catalog, offerIndex);

        public bool TryBuyKeystone(ShopCatalogSO catalog, KeystoneId keystoneId)
        {
            return lifecycle.TryBuyKeystone(catalog, keystoneId);
        }

        public OutpostActionResult TryBuyKeystoneDetailed(ShopCatalogSO catalog, KeystoneId keystoneId) => lifecycle.TryBuyKeystoneDetailed(catalog, keystoneId);

        public bool TryUseInn(int maxHp, float baseMultiplier, float expeditionMultiplier, out int cost)
        {
            return lifecycle.TryUseInn(maxHp, baseMultiplier, expeditionMultiplier, out cost);
        }

        public OutpostActionResult TryUseInnDetailed(int maxHp, float baseMultiplier, float expeditionMultiplier) => lifecycle.TryUseInnDetailed(maxHp, baseMultiplier, expeditionMultiplier);

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

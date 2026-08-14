using System;

namespace TestCardGame.Run
{
    /// <summary>
    /// RUNの生成、場面遷移、終了を管理するアプリケーション層。
    /// UnityのライフサイクルやScene APIには依存しない。
    /// </summary>
    public sealed class RunLifecycleService
    {
        private readonly RunProgressService progressService;
        private readonly ShopService shopService;

        public RunDefinitionSO Definition { get; private set; }
        public RunState State { get; private set; }
        public bool HasActiveRun => State != null
            && Definition != null
            && State.phase != RunProgressPhase.Completed
            && State.phase != RunProgressPhase.Failed;

        public RunLifecycleService(RunProgressService progressService = null, ShopService shopService = null)
        {
            this.progressService = progressService ?? new RunProgressService();
            this.shopService = shopService ?? new ShopService(new SystemRandomService());
        }

        public RunState Begin(RunDefinitionSO definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            Definition = definition;
            State = progressService.StartRun(definition);
            progressService.OpenTown(State);
            return State;
        }



        public bool OpenTown()
        {
            if (State == null)
            {
                return false;
            }

            progressService.OpenTown(State);
            return true;
        }

        public bool TryStartNextExpedition()
        {
            return State != null
                && progressService.StartNextExpedition(State, Definition);
        }

        public void EnsureShopInventory(ShopCatalogSO catalog)
        {
            shopService.EnsureInventory(State, catalog);
        }

        public bool TryBuyShopCard(ShopCatalogSO catalog, int offerIndex)
        {
            return shopService.TryBuyCard(State, catalog, offerIndex);
        }

        public bool TryBuyKeystone(ShopCatalogSO catalog, KeystoneId keystoneId)
        {
            return shopService.TryBuyKeystone(State, catalog, keystoneId);
        }

        public bool TryUseInn(int maxHp, float baseMultiplier, float expeditionMultiplier, out int cost)
        {
            cost = 0;
            if (State == null || State.phase != RunProgressPhase.Town)
            {
                return false;
            }

            int missingHp = Math.Max(0, maxHp - State.currentHp);
            cost = (int)Math.Ceiling(missingHp * (Math.Max(0f, baseMultiplier)
                + Math.Max(0, State.currentExpeditionIndex) * Math.Max(0f, expeditionMultiplier)));
            if (missingHp == 0 || State.ownedGold < cost)
            {
                return false;
            }

            State.ownedGold -= cost;
            State.currentHp = maxHp;
            return true;
        }

        public void MarkFailed()
        {
            progressService.MarkRunFailed(State);
        }

        public void MarkCompleted()
        {
            progressService.MarkRunCompleted(State);
        }
    }
}

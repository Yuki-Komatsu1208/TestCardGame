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

        /// <summary>カード購入前の状態を分類し、既存購入処理と同じ規則で結果を返す。</summary>
        public OutpostActionResult TryBuyShopCardDetailed(ShopCatalogSO catalog, int offerIndex)
        {
            if (State == null || State.phase != RunProgressPhase.Town)
                return OutpostActionResult.Failure(OutpostFailureReason.InvalidRunState, "今は購入できません");
            if (catalog == null)
                return OutpostActionResult.Failure(OutpostFailureReason.InvalidDefinition, "商品情報を読み込めませんでした");
            shopService.EnsureInventory(State, catalog);
            if (offerIndex < 0 || offerIndex >= State.shopCardOffers.Count || State.shopCardOffers[offerIndex]?.card == null)
                return OutpostActionResult.Failure(OutpostFailureReason.InvalidProduct, "その商品は選べません");
            ShopCardOfferState offer = State.shopCardOffers[offerIndex];
            if (offer.isPurchased)
                return OutpostActionResult.Failure(OutpostFailureReason.SoldOut, "売り切れです");
            if (State.ownedGold < offer.price)
                return OutpostActionResult.Failure(OutpostFailureReason.InsufficientGold, "お金がたらないよ！", offer.price);
            return shopService.TryBuyCard(State, catalog, offerIndex)
                ? OutpostActionResult.Success(offer.price)
                : OutpostActionResult.Failure(OutpostFailureReason.InvalidProduct, "購入できませんでした");
        }

        public bool TryBuyKeystone(ShopCatalogSO catalog, KeystoneId keystoneId)
        {
            return shopService.TryBuyKeystone(State, catalog, keystoneId);
        }

        /// <summary>キーストーン購入の失敗理由を状態変更前に分類する。</summary>
        public OutpostActionResult TryBuyKeystoneDetailed(ShopCatalogSO catalog, KeystoneId keystoneId)
        {
            if (State == null || State.phase != RunProgressPhase.Town)
                return OutpostActionResult.Failure(OutpostFailureReason.InvalidRunState, "今は購入できません");
            if (catalog == null)
                return OutpostActionResult.Failure(OutpostFailureReason.InvalidDefinition, "商品情報を読み込めませんでした");
            if (State.selectedKeystone != KeystoneId.None)
                return OutpostActionResult.Failure(OutpostFailureReason.KeystoneAlreadyOwned, "キーストーンはひとつだけです");
            if (State.ownedGold < catalog.keystonePrice)
                return OutpostActionResult.Failure(OutpostFailureReason.InsufficientGold, "お金がたらないよ！", catalog.keystonePrice);
            return shopService.TryBuyKeystone(State, catalog, keystoneId)
                ? OutpostActionResult.Success(catalog.keystonePrice)
                : OutpostActionResult.Failure(OutpostFailureReason.InvalidProduct, "その商品は選べません");
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

        /// <summary>宿屋利用の失敗理由と価格を返し、成功時だけ既存処理で回復する。</summary>
        public OutpostActionResult TryUseInnDetailed(int maxHp, float baseMultiplier, float expeditionMultiplier)
        {
            if (State == null || State.phase != RunProgressPhase.Town)
                return OutpostActionResult.Failure(OutpostFailureReason.InvalidRunState, "今は宿屋を利用できません");
            int missingHp = Math.Max(0, maxHp - State.currentHp);
            int cost = (int)Math.Ceiling(missingHp * (Math.Max(0f, baseMultiplier)
                + Math.Max(0, State.currentExpeditionIndex) * Math.Max(0f, expeditionMultiplier)));
            if (missingHp == 0)
                return OutpostActionResult.Failure(OutpostFailureReason.FullHealth, "すでに全快です", cost);
            if (State.ownedGold < cost)
                return OutpostActionResult.Failure(OutpostFailureReason.InsufficientGold, "お金がたらないよ！", cost);
            return TryUseInn(maxHp, baseMultiplier, expeditionMultiplier, out _)
                ? OutpostActionResult.Success(cost)
                : OutpostActionResult.Failure(OutpostFailureReason.InvalidRunState, "宿屋を利用できませんでした", cost);
        }

        /// <summary>遠征開始に失敗した場合もScene遷移前に理由を返す。</summary>
        public OutpostActionResult TryStartNextExpeditionDetailed()
        {
            if (State == null || State.phase != RunProgressPhase.Town)
                return OutpostActionResult.Failure(OutpostFailureReason.InvalidRunState, "今は出発できません");
            return TryStartNextExpedition()
                ? OutpostActionResult.Success()
                : OutpostActionResult.Failure(OutpostFailureReason.NoExpedition, "出発できる遠征がありません");
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

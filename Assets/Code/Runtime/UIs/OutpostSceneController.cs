using System;
using System.Collections.Generic;
using TestCardGame.Run;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestCardGame.UIs
{
    /// <summary>事前配置済みOutpost ViewとRUNバックエンドを仲介する。</summary>
    public sealed class OutpostSceneController : MonoBehaviour
    {
        [SerializeField] private ShopCatalogSO catalog;
        [SerializeField] private RunDefinitionSO runDefinition;
        [SerializeField] private OutpostEconomyConfigSO innEconomy;
        [SerializeField] private CommonUiController commonUi;
        [SerializeField] private OutpostRootView rootView;

        private int MaxHp => runDefinition?.playerDefinition != null ? runDefinition.playerDefinition.maxHp : 100;
        private int MaxStunResist => runDefinition?.playerDefinition != null ? runDefinition.playerDefinition.maxStunResist : 100;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                Debug.LogError("OutpostSceneController: 必須UI参照が不足しているため操作を停止します。", this);
                return;
            }

            RunSession session = RunSession.GetOrCreate();
            if (!session.HasActiveRun)
            {
                Debug.LogError("OutpostSceneController: 有効なRUNがありません。", this);
                FatalRunRecoveryService.TryRecover(runDefinition, "Outpostに有効なRUN状態がありません。");
                return;
            }

            session.EnsureShopInventory(catalog);
            BindView();
            ShowFacilities();
        }

        private void OnDestroy()
        {
            if (rootView != null)
                rootView.ClearListeners();
        }

        /// <summary>事前配置済みボタンの通知先を登録する。</summary>
        private void BindView()
        {
            rootView.BindFacilityActions(ShowInn, ShowShop, () => ShowPreparing("工房"),
                () => ShowPreparing("クエストカウンター"), Depart);
            rootView.BindBackActions(ShowFacilities);
            rootView.Inn.UseButton.onClick.AddListener(UseInn);
            rootView.Shop.Bind(BuyCard, BuyKeystone);
        }

        private void ShowFacilities()
        {
            rootView.Show(OutpostPanelKind.Facilities);
            RefreshStatus();
        }

        private void ShowInn()
        {
            rootView.Show(OutpostPanelKind.Inn);
            RefreshInn();
            RefreshStatus();
        }

        private void ShowShop()
        {
            rootView.Show(OutpostPanelKind.Shop);
            RefreshShop();
            RefreshStatus();
        }

        private void ShowPreparing(string facilityName)
        {
            rootView.Preparing.Render(facilityName, "準備中");
            rootView.Show(OutpostPanelKind.Preparing);
            RefreshStatus();
        }

        /// <summary>宿屋の現在価格と利用不能理由をRUN状態から再計算する。</summary>
        private void RefreshInn()
        {
            RunState state = RunSession.GetOrCreate().State;
            float baseMultiplier = innEconomy != null ? innEconomy.innBaseMultiplier : 2f;
            float expeditionMultiplier = innEconomy != null ? innEconomy.innExpeditionMultiplier : 0f;
            int missingHp = Math.Max(0, MaxHp - state.currentHp);
            int cost = (int)Math.Ceiling(missingHp * (Math.Max(0f, baseMultiplier)
                + Math.Max(0, state.currentExpeditionIndex) * Math.Max(0f, expeditionMultiplier)));
            string reason = missingHp == 0 ? "すでに全快です" : state.ownedGold < cost ? "ゴールドが足りません" : string.Empty;
            rootView.Inn.Render(new InnDisplayModel(state.currentHp, MaxHp, MaxHp, cost, reason.Length == 0, reason));
        }

        /// <summary>固定在庫を表示専用モデルへ変換し、事前配置枠へ反映する。</summary>
        private void RefreshShop()
        {
            RunState state = RunSession.GetOrCreate().State;
            var cards = new List<ShopCardDisplayModel>(state.shopCardOffers?.Count ?? 0);
            if (state.shopCardOffers != null)
            {
                for (int i = 0; i < state.shopCardOffers.Count; i++)
                {
                    ShopCardOfferState offer = state.shopCardOffers[i];
                    bool valid = offer?.card != null;
                    cards.Add(new ShopCardDisplayModel(i, valid ? offer.card.cardName : "定義なし",
                        offer?.price ?? 0, offer?.isOnSale ?? false, offer?.isPurchased ?? false,
                        valid && !offer.isPurchased));
                }
            }

            var keystones = new List<KeystoneDisplayModel>();
            int count = catalog.keystones?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                KeystoneDefinition definition = catalog.keystones[i];
                if (definition == null || definition.id == KeystoneId.None)
                    continue;
                bool hasSelection = state.selectedKeystone != KeystoneId.None;
                bool soldOut = state.selectedKeystone == definition.id;
                bool unavailable = hasSelection && !soldOut;
                keystones.Add(new KeystoneDisplayModel(definition.id, definition.displayName,
                    definition.description, catalog.keystonePrice, soldOut, unavailable, !hasSelection));
            }

            rootView.Shop.Render(cards, keystones);
        }

        private void UseInn()
        {
            float baseMultiplier = innEconomy != null ? innEconomy.innBaseMultiplier : 2f;
            float expeditionMultiplier = innEconomy != null ? innEconomy.innExpeditionMultiplier : 0f;
            OutpostActionResult result = RunSession.GetOrCreate().TryUseInnDetailed(MaxHp, baseMultiplier, expeditionMultiplier);
            PublishResult(result, "傷が癒えた");
            RefreshInn();
            RefreshStatus();
        }

        private void BuyCard(int index)
        {
            OutpostActionResult result = RunSession.GetOrCreate().TryBuyShopCardDetailed(catalog, index);
            PublishResult(result, "カードを購入した");
            RefreshShop();
            RefreshStatus();
        }

        private void BuyKeystone(KeystoneId id)
        {
            OutpostActionResult result = RunSession.GetOrCreate().TryBuyKeystoneDetailed(catalog, id);
            PublishResult(result, "キーストーンを購入した");
            RefreshShop();
            RefreshStatus();
        }

        private void Depart()
        {
            OutpostActionResult result = RunSession.GetOrCreate().TryStartNextExpeditionDetailed();
            if (result.Succeeded)
                SceneManager.LoadScene("BattleScene");
            else
                PlayerMessageService.Publish(result.PlayerMessage, PlayerMessageKind.Warning);
        }

        /// <summary>操作結果を共通上部ログへ一度だけ通知する。</summary>
        private static void PublishResult(OutpostActionResult result, string successMessage)
        {
            PlayerMessageService.Publish(result.Succeeded ? successMessage : result.PlayerMessage,
                result.Succeeded ? PlayerMessageKind.Normal : PlayerMessageKind.Warning);
        }

        /// <summary>Outpostで常時表示する確定情報だけを共通ステータスへ渡す。</summary>
        private void RefreshStatus()
        {
            RunState state = RunSession.GetOrCreate().State;
            var items = new List<CommonStatusItemModel>
            {
                new("HP", $"{state.currentHp} / {MaxHp}"),
                new("スタン耐性", $"{state.currentStunResist} / {MaxStunResist}"),
                new("ゴールド", $"{state.ownedGold} G")
            };
            if (state.selectedKeystone != KeystoneId.None)
                items.Add(new CommonStatusItemModel("キーストーン", GetKeystoneName(state.selectedKeystone)));
            if (state.ownedItemIds != null && state.ownedItemIds.Count > 0)
                items.Add(new CommonStatusItemModel("所持品", $"{state.ownedItemIds.Count}個"));
            string expeditionName = GetExpeditionName(state.currentExpeditionIndex);
            if (!string.IsNullOrWhiteSpace(expeditionName))
                items.Add(new CommonStatusItemModel("次の遠征", expeditionName));
            items.Add(new CommonStatusItemModel("進行", $"{state.currentExpeditionIndex + 1}-{state.currentStageIndex + 1}"));
            if (state.pendingGold > 0)
                items.Add(new CommonStatusItemModel("OverHunt帰還", $"確定 {state.pendingGold} G"));
            commonUi.RenderStatus(items);
        }

        private string GetKeystoneName(KeystoneId id)
        {
            KeystoneDefinition found = catalog.keystones?.Find(entry => entry != null && entry.id == id);
            return found?.displayName ?? id.ToString();
        }

        private string GetExpeditionName(int index)
        {
            if (runDefinition?.expeditions != null && index >= 0 && index < runDefinition.expeditions.Count)
                return runDefinition.expeditions[index]?.expeditionName;
            return runDefinition?.runName;
        }

        /// <summary>Outpost固有のInspector参照と固定スロット数を検証する。</summary>
        public bool ValidateReferences()
        {
            bool valid = catalog != null && runDefinition != null && commonUi != null && rootView != null;
            if (!valid)
            {
                Debug.LogError("OutpostSceneController: Inspector参照が不足しています。", this);
                return false;
            }
            return rootView.ValidateReferences();
        }
    }
}

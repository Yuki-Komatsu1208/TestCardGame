using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TestCardGame.Character;
using TestCardGame.Character.Player;
using TestCardGame.Stage;
using TestCardGame.Run;
using TestCardGame.Cards.Core;
using TestCardGame.Rewards;

namespace TestCardGame.Controller
{
    public class BattleUI : MonoBehaviour
    {
        [Header("Pre-configured UI References")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button endTurnButton;

        // 報酬やRun終了を表示する動的UI。
        [SerializeField] private GameObject rewardsPanel;
        [SerializeField] private GameObject endRunPanel;
        [SerializeField] private TextMeshProUGUI rewardsTitleText;
        [SerializeField] private List<Button> rewardButtons = new();
        private Button townDepartureButton;

        [Header("Deck overlay & view references")]
        [SerializeField] private Button checkDeckButton;
        [SerializeField] private UIs.CardListSelectionOverlay cardListOverlay;

        private GameController gameController;
        private RunController runController;
        private RewardController rewardController;

        /// <summary>
        /// バトルUIを構築し、Run/Rewardのイベント購読を開始する。
        /// </summary>
        public void Initialize(GameController controller)
        {
            gameController = controller;
            runController = FindAnyObjectByType<RunController>();
            rewardController = runController != null
                ? runController.GetComponent<RewardController>()
                : FindAnyObjectByType<RewardController>();

            bool hasPreconfiguredUI = statusText != null && endTurnButton != null && rewardsPanel != null && endRunPanel != null;

            if (!hasPreconfiguredUI)
            {
                Debug.Log("BattleUI: 事前配置されたUIが不完全なため、動的にUIを生成します。");

                var canvas = FindAnyObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("BattleUI: シーン内に Canvas が見つかりません。");
                    return;
                }

                foreach (Transform child in canvas.transform)
                {
                    if (child.name == "BattleUIPanel" || child.name == "EndTurnButton" || child.name == "RewardsPanel" || child.name == "EndRunPanel")
                    {
                        Destroy(child.gameObject);
                    }
                }

                GameObject panelObj = new GameObject("BattleUIPanel", typeof(RectTransform));
                panelObj.transform.SetParent(canvas.transform, false);
                var panelRect = panelObj.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0f, 0.75f);
                panelRect.anchorMax = new Vector2(1f, 1f);
                panelRect.pivot = new Vector2(0.5f, 1f);
                panelRect.anchoredPosition = new Vector2(0f, -20f);
                panelRect.sizeDelta = new Vector2(0f, 150f);

                GameObject textObj = new GameObject("StatusText", typeof(RectTransform));
                textObj.transform.SetParent(panelRect, false);
                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;

                statusText = textObj.AddComponent<TextMeshProUGUI>();
                statusText.fontSize = 20;
                statusText.alignment = TextAlignmentOptions.Center;
                statusText.color = Color.white;
                statusText.text = "バトルを初期化しています...";

                GameObject buttonObj = new GameObject("EndTurnButton", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObj.transform.SetParent(canvas.transform, false);
                var buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(1f, 0f);
                buttonRect.anchorMax = new Vector2(1f, 0f);
                buttonRect.pivot = new Vector2(1f, 0f);
                buttonRect.anchoredPosition = new Vector2(-20f, 20f);
                buttonRect.sizeDelta = new Vector2(160f, 60f);

                var btnImage = buttonObj.GetComponent<Image>();
                btnImage.color = new Color(0.2f, 0.6f, 0.2f, 1.0f);

                endTurnButton = buttonObj.GetComponent<Button>();
                endTurnButton.onClick.AddListener(OnEndTurnClicked);

                GameObject btnTextObj = new GameObject("Text", typeof(RectTransform));
                btnTextObj.transform.SetParent(buttonRect, false);
                var btnTextRect = btnTextObj.GetComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.sizeDelta = Vector2.zero;

                var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
                btnText.fontSize = 20;
                btnText.alignment = TextAlignmentOptions.Center;
                btnText.color = Color.white;
                btnText.text = "ターン終了";

                CreateRewardsPanel(canvas);
                CreateEndRunPanel(canvas);
            }
            else
            {
                Debug.Log("BattleUI: 事前配置されたUIを使用します。");

                endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
                endTurnButton.onClick.AddListener(OnEndTurnClicked);

                if (rewardsPanel != null) rewardsPanel.SetActive(false);
                if (endRunPanel != null) endRunPanel.SetActive(false);
            }

            if (cardListOverlay == null)
            {
                cardListOverlay = FindAnyObjectByType<UIs.CardListSelectionOverlay>();
            }

            if (checkDeckButton == null)
            {
                var existingBtn = GameObject.Find("CheckDeckButton");
                if (existingBtn != null)
                {
                    checkDeckButton = existingBtn.GetComponent<Button>();
                }
            }

            if (checkDeckButton != null)
            {
                checkDeckButton.onClick.RemoveAllListeners();
                checkDeckButton.onClick.AddListener(() =>
                {
                    if (runController != null && runController.RunState != null && cardListOverlay != null)
                    {
                        cardListOverlay.Show("現在のデッキ一覧", runController.RunState.playerDeck);
                    }
                });
            }

            if (rewardController != null)
            {
                // 報酬UIの表示切替はRewardControllerのイベントを起点にする。
                rewardController.RewardScreenOpened -= OnRewardScreenOpened;
                rewardController.RewardScreenOpened += OnRewardScreenOpened;
            }

            if (runController != null)
            {
                runController.StageStarted -= OnStageStarted;
                runController.StageStarted += OnStageStarted;

                runController.RouteChoiceRequested -= OnRouteChoiceRequested;
                runController.RouteChoiceRequested += OnRouteChoiceRequested;

                runController.OverhuntChoiceRequested -= OnOverhuntChoiceRequested;
                runController.OverhuntChoiceRequested += OnOverhuntChoiceRequested;

                runController.TownOpened -= OnTownOpened;
                runController.TownOpened += OnTownOpened;

                runController.RunStateChanged -= OnRunStateChanged;
                runController.RunStateChanged += OnRunStateChanged;

                runController.RunWon -= OnRunWon;
                runController.RunWon += OnRunWon;

                runController.RunLost -= OnRunLost;
                runController.RunLost += OnRunLost;
            }

            if (runController != null && runController.IsTownOpen)
            {
                OpenTownMenu();
            }

            Refresh();
        }

        /// <summary>
        /// ステージ勝利後に表示するカード報酬パネルを作成する。
        /// </summary>
        private void CreateRewardsPanel(Canvas canvas)
        {
            rewardsPanel = new GameObject("RewardsPanel", typeof(RectTransform), typeof(Image));
            rewardsPanel.transform.SetParent(canvas.transform, false);
            var rect = rewardsPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.9f);
            rect.sizeDelta = Vector2.zero;

            rewardsPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(rewardsPanel.transform, false);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.8f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.sizeDelta = Vector2.zero;

            rewardsTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            rewardsTitleText.fontSize = 28;
            rewardsTitleText.alignment = TextAlignmentOptions.Center;
            rewardsTitleText.color = Color.yellow;
            rewardsTitleText.text = "ステージクリア！報酬カードを選んでください";

            GameObject cardsGrid = new GameObject("CardsGrid", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardsGrid.transform.SetParent(rewardsPanel.transform, false);
            var gridRect = cardsGrid.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.05f, 0.2f);
            gridRect.anchorMax = new Vector2(0.95f, 0.75f);
            gridRect.sizeDelta = Vector2.zero;

            var layout = cardsGrid.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 20f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            rewardButtons.Clear();
            for (int i = 0; i < 5; i++)
            {
                int index = i;
                GameObject cardBtnObj = new GameObject($"RewardBtn_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
                cardBtnObj.transform.SetParent(cardsGrid.transform, false);
                var btnImg = cardBtnObj.GetComponent<Image>();
                btnImg.color = new Color(0.25f, 0.25f, 0.35f, 1f);

                GameObject cardTxtObj = new GameObject("Text", typeof(RectTransform));
                cardTxtObj.transform.SetParent(cardBtnObj.transform, false);
                var txtRect = cardTxtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.sizeDelta = Vector2.zero;

                var txtComp = cardTxtObj.AddComponent<TextMeshProUGUI>();
                txtComp.fontSize = 18;
                txtComp.alignment = TextAlignmentOptions.Center;
                txtComp.color = Color.white;
                txtComp.text = "報酬選択";

                var btn = cardBtnObj.GetComponent<Button>();
                rewardButtons.Add(btn);
            }

            rewardsPanel.SetActive(false);
        }

        /// <summary>
        /// Run勝利または敗北時に表示する終了パネルを作成する。
        /// </summary>
        private void CreateEndRunPanel(Canvas canvas)
        {
            endRunPanel = new GameObject("EndRunPanel", typeof(RectTransform), typeof(Image));
            endRunPanel.transform.SetParent(canvas.transform, false);
            var rect = endRunPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.2f);
            rect.anchorMax = new Vector2(0.85f, 0.8f);
            rect.sizeDelta = Vector2.zero;

            endRunPanel.GetComponent<Image>().color = new Color(0.2f, 0.05f, 0.05f, 0.98f);

            GameObject textObj = new GameObject("EndText", typeof(RectTransform));
            textObj.transform.SetParent(endRunPanel.transform, false);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 32;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.red;
            txt.text = "ラン失敗\nプレイヤーのHPが0になりました。";

            endRunPanel.SetActive(false);
        }

        /// <summary>
        /// 現在のバトル状態をテキストとボタン状態に反映する。
        /// </summary>
        public void Refresh()
        {
            if (gameController == null || statusText == null) return;

            var player = gameController.PlayerUnitInstance;
            if (player == null) return;

            string stageName = gameController.CurrentStageName;

            string playerStatusStr = GetStatusEffectsString(player);

            string enemiesInfo = "";
            int livingEnemiesCount = 0;
            foreach (var enemy in gameController.Enemies)
            {
                if (enemy == null) continue;
                if (enemy.Hp.CurrentValue > 0)
                {
                    livingEnemiesCount++;
                    string enemyStatuses = GetStatusEffectsString(enemy as IUnit);
                    enemiesInfo += $"\n{enemy.Name} HP: {enemy.Hp.CurrentValue} {enemyStatuses}";
                }
                else
                {
                    enemiesInfo += $"\n<s>{enemy.Name}（撃破済み）</s>";
                }
            }

            string turnName = gameController.IsPlayerTurn ? "<color=green>プレイヤーのターン</color>" : "<color=red>敵のターン</color>";
            string runInfo = string.Empty;
            if (runController != null && runController.RunState != null)
            {
                var state = runController.RunState;
                runInfo = $"\n所持ゴールド: {state.ownedGold} / 保留ゴールド: {state.pendingGold}\n" +
                          $"進行状態: {GetPhaseLabel(state.phase)}";
            }

            statusText.text = $"<b>{stageName}</b> (残り敵: {livingEnemiesCount}体)\n" +
                             $"{turnName}\n" +
                             $"プレイヤーマナ: {player.Mana}/{player.MaxMana}\n" +
                             $"プレイヤーHP: {player.Hp.CurrentValue} {playerStatusStr}" +
                             $"{runInfo}" +
                             $"{enemiesInfo}";

            if (endTurnButton != null)
            {
                bool isBattleInteractionEnabled = gameController.CurrentBattleResult == BattleResult.None
                    && (runController == null || (!runController.IsTownOpen && !runController.IsAwaitingRouteChoice && !runController.IsAwaitingOverhuntChoice));
                endTurnButton.interactable = gameController.IsPlayerTurn && isBattleInteractionEnabled;
            }
        }

        /// <summary>
        /// ユニットに付与されている状態異常を表示用文字列に変換する。
        /// </summary>
        private string GetStatusEffectsString(IUnit unit)
        {
            if (unit == null || unit.StatusEffects == null || unit.StatusEffects.Count == 0) return "";
            List<string> list = new();
            foreach (var s in unit.StatusEffects)
            {
                if (s.Id == TestCardGame.Character.StatusEffects.StatusEffectId.Shield)
                {
                    list.Add($"{s.DisplayName}(値:{s.Value}/{s.GetDisplayMaxValue()})");
                }
                else
                {
                    list.Add($"{s.DisplayName}({s.RemainingTurns}T, 値:{s.Value})");
                }
            }
            return "[" + string.Join(", ", list) + "]";
        }

        /// <summary>
        /// ターン終了ボタンが押されたときにプレイヤーターン終了を要求する。
        /// </summary>
        private void OnEndTurnClicked()
        {
            if (gameController != null && gameController.IsPlayerTurn)
            {
                gameController.EndPlayerTurn();
            }
        }

        /// <summary>
        /// ステージ開始時に報酬/終了パネルを閉じて表示を更新する。
        /// </summary>
        private void OnStageStarted(StageDefinitionSO stageDef, RunState runState)
        {
            if (rewardsPanel != null) rewardsPanel.SetActive(false);
            HideTownDepartureButton();
            if (endRunPanel != null) endRunPanel.SetActive(false);
            Refresh();
        }

        private void OnRouteChoiceRequested(RunState state)
        {
            var choices = new List<(string label, System.Action action)>
            {
                ($"<b><color=yellow>【街へ帰る】</color></b>\nOverHuntの保留ゴールドを確定する", () => runController?.ChooseReturnToTown())
            };

            if (runController != null && runController.CanGoToOverhunt)
            {
                choices.Add(($"<b><color=red>【OverHuntへ進む】</color></b>\n強敵戦に挑む / 成功で保留ゴールド追加 / 死亡時RUN失敗", () => runController?.ChooseOverhunt()));
            }

            ShowChoicePanel("遠征ボス撃破！ 次の行動を選択してください", choices);
        }

        private void OnOverhuntChoiceRequested(RunState state)
        {
            ShowChoicePanel(
                "OverHunt戦クリア！ 続けるか、街へ帰るかを選択してください",
                new List<(string label, System.Action action)>
                {
                    ($"<b><color=yellow>【街へ帰る】</color></b>\n保留ゴールドを確定して街へ戻る", () => runController?.ChooseReturnToTown()),
                    ($"<b><color=red>【さらに続行】</color></b>\n次のOverHunt戦へ進む / 死亡時RUN失敗", () => runController?.ChooseOverhunt())
                });
        }

        private void OnTownOpened(RunState state)
        {
            OpenTownMenu();
        }

        private void OnRunStateChanged(RunState state)
        {
            Refresh();
        }

        /// <summary>
        /// 報酬候補をボタンへ割り当て、報酬選択パネルを表示する。
        /// </summary>
        private void OnRewardScreenOpened(List<RewardChoice> choices)
        {
            if (rewardsPanel == null) return;

            rewardsPanel.SetActive(true);
            rewardsTitleText.text = "ステージクリア！報酬を選択してください";

            for (int i = 0; i < rewardButtons.Count; i++)
            {
                var btn = rewardButtons[i];
                var textComp = btn.GetComponentInChildren<TextMeshProUGUI>();

                if (i < choices.Count)
                {
                    var choice = choices[i];
                    btn.gameObject.SetActive(true);

                    if (choice.Type == RewardType.Mod)
                    {
                        textComp.text = $"<b><color=cyan>【MOD】\n{choice.Title}</color></b>\n\n{choice.Description}";
                    }
                    else if (choice.Type == RewardType.Heal)
                    {
                        textComp.text = $"<b><color=green>【HP回復】\n{choice.Title}</color></b>\n\n{choice.Description}";
                    }
                    else if (choice.Type == RewardType.LevelUp)
                    {
                        textComp.text = $"<b><color=yellow>【レベルアップ】\n{choice.Title}</color></b>\n\n{choice.Description}";
                    }

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (choice.Type == RewardType.Heal)
                        {
                            rewardsPanel.SetActive(false);
                            rewardController?.ChooseHealReward();
                        }
                        else
                        {
                            // MOD/LevelUpは対象カード選択を挟んでから適用する。
                            OpenDeckCardSelection(choice);
                        }
                    });
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// プレイヤーのデッキ一覧を表示し、MOD付与やレベルアップの対象カードを選択させる。
        /// </summary>
        private void OpenDeckCardSelection(RewardChoice choice)
        {
            if (runController == null || runController.RunState == null) return;

            string title = "";
            if (choice.Type == RewardType.Mod)
            {
                title = $"MOD付与: 対象カードを選択してください\n<color=cyan>【付与するMOD: {choice.Title}】</color>";
            }
            else if (choice.Type == RewardType.LevelUp)
            {
                title = "レベルアップ: 対象カードを選択してください\n<color=yellow>【カードレベル +1】</color>";
            }

            if (rewardsPanel != null) rewardsPanel.SetActive(false);

            var overlay = cardListOverlay != null ? cardListOverlay : UIs.CardListSelectionOverlay.Instance;
            if (overlay != null)
            {
                overlay.Show(
                    title,
                    runController.RunState.playerDeck,
                    choice,
                    onSelect: (deckIndex) =>
                    {
                        rewardController?.ChooseDeckCardForReward(deckIndex, choice);
                    },
                    onCancel: () =>
                    {
                        if (rewardsPanel != null) rewardsPanel.SetActive(true);
                    }
                );
            }
        }

        private void OpenTownMenu()
        {
            if (runController == null || runController.RunState == null)
            {
                return;
            }

            if (!runController.HasSelectedKeystone)
            {
                var keystoneChoices = new List<(string label, System.Action action)>();
                foreach (KeystoneDefinition keystone in runController.AvailableKeystones)
                {
                    KeystoneDefinition capturedKeystone = keystone;
                    keystoneChoices.Add(($"<b><color=yellow>【{capturedKeystone.displayName}】</color></b>\n0G / {capturedKeystone.description}", () =>
                    {
                        if (runController.TrySelectKeystone(capturedKeystone.id)) OpenTownMenu();
                    }));
                }

                HideTownDepartureButton();
                ShowChoicePanel("遠征出発前：キーストーンを1つ選んでください（0G）", keystoneChoices, false);
                return;
            }

            ShowChoicePanel(
                $"街に到着しました。所持ゴールド: {runController.RunState.ownedGold}\n購入したい項目を選んでください",
                new List<(string label, System.Action action)>
                {
                    ($"<b><color=green>【HP回復】</color></b>\n{runController.TownHealCost}G / 最大HPの25%回復", () => { if (runController.TryBuyTownHeal()) OpenTownMenu(); }),
                    ($"<b><color=yellow>【レベルアップ】</color></b>\n{runController.TownLevelUpCost}G / ランダムな強化可能カードを+1", () => { if (runController.TryBuyTownLevelUp()) OpenTownMenu(); }),
                    ($"<b><color=cyan>【MOD付与】</color></b>\n{runController.TownModCost}G / ランダムなカードへランダムMOD付与", () => { if (runController.TryBuyTownModifier()) OpenTownMenu(); }),
                    ($"<b><color=magenta>【新カード獲得】</color></b>\n{runController.TownNewCardCost}G / 報酬プールからカードを1枚獲得", () => { if (runController.TryBuyTownNewCard()) OpenTownMenu(); })
                }, false);
            ShowTownDepartureButton();
        }

        private void ShowChoicePanel(string title, List<(string label, System.Action action)> choices, bool closeBeforeAction = true)
        {
            if (rewardsPanel == null || rewardsTitleText == null)
            {
                return;
            }

            EnsureChoiceButtonCapacity(choices?.Count ?? 0);
            rewardsPanel.SetActive(true);
            rewardsTitleText.text = title;

            for (int i = 0; i < rewardButtons.Count; i++)
            {
                var btn = rewardButtons[i];
                var textComp = btn.GetComponentInChildren<TextMeshProUGUI>();
                btn.onClick.RemoveAllListeners();

                if (choices != null && i < choices.Count)
                {
                    var choice = choices[i];
                    btn.gameObject.SetActive(true);
                    textComp.text = choice.label;
                    btn.onClick.AddListener(() =>
                    {
                        if (closeBeforeAction)
                        {
                            rewardsPanel.SetActive(false);
                        }
                        choice.action?.Invoke();
                    });
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// シーンに事前配置された3ボタンでも、街メニューなど5択を表示できるようにする。
        /// </summary>
        private void EnsureChoiceButtonCapacity(int requiredCount)
        {
            if (requiredCount <= rewardButtons.Count || rewardButtons.Count == 0)
            {
                return;
            }

            Button template = rewardButtons[0];
            for (int i = rewardButtons.Count; i < requiredCount; i++)
            {
                var extraButton = Instantiate(template, template.transform.parent);
                extraButton.name = $"RewardBtn_{i}";
                extraButton.onClick.RemoveAllListeners();
                rewardButtons.Add(extraButton);
            }
        }

        private void ShowTownDepartureButton()
        {
            if (runController == null || !runController.IsTownOpen || !runController.HasSelectedKeystone || endTurnButton == null)
            {
                return;
            }

            if (townDepartureButton == null)
            {
                townDepartureButton = Instantiate(endTurnButton, endTurnButton.transform.parent);
                townDepartureButton.name = "TownDepartureButton";
                var rect = townDepartureButton.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(20f, 20f);
                rect.sizeDelta = new Vector2(220f, 60f);
            }

            var label = townDepartureButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = runController.IsFinalExpedition ? "RUNを完了" : "次の遠征へ";
            }

            townDepartureButton.onClick.RemoveAllListeners();
            townDepartureButton.onClick.AddListener(() => runController.LeaveTown());
            townDepartureButton.gameObject.SetActive(true);
        }

        private void HideTownDepartureButton()
        {
            if (townDepartureButton != null)
            {
                townDepartureButton.gameObject.SetActive(false);
            }
        }

        private string GetPhaseLabel(RunProgressPhase phase)
        {
            return phase switch
            {
                RunProgressPhase.Expedition => "遠征中",
                RunProgressPhase.AwaitingNormalRewardResolution => "通常戦闘報酬の選択待ち",
                RunProgressPhase.AwaitingReturnOrOverhuntChoice => "ボス撃破後の選択待ち",
                RunProgressPhase.OverHunt => "OverHunt中",
                RunProgressPhase.AwaitingOverhuntDecision => "OverHunt継続選択待ち",
                RunProgressPhase.Town => "街",
                RunProgressPhase.Completed => "完了",
                RunProgressPhase.Failed => "失敗",
                _ => "進行中"
            };
        }

        /// <summary>
        /// Run勝利時の終了パネルを表示する。
        /// </summary>
        private void OnRunWon()
        {
            HideTownDepartureButton();
            if (endRunPanel == null) return;
            endRunPanel.SetActive(true);
            var imageComp = endRunPanel.GetComponent<Image>();
            imageComp.color = new Color(0.05f, 0.2f, 0.05f, 0.98f);

            var txt = endRunPanel.GetComponentInChildren<TextMeshProUGUI>();
            txt.color = Color.yellow;
            txt.text = "ランクリア！\n\nすべてのステージを突破しました。";
        }

        /// <summary>
        /// Run敗北時の終了パネルを表示する。
        /// </summary>
        private void OnRunLost()
        {
            HideTownDepartureButton();
            if (endRunPanel == null) return;
            endRunPanel.SetActive(true);
            var imageComp = endRunPanel.GetComponent<Image>();
            imageComp.color = new Color(0.2f, 0.05f, 0.05f, 0.98f);

            var txt = endRunPanel.GetComponentInChildren<TextMeshProUGUI>();
            txt.color = Color.red;
            txt.text = "ゲームオーバー\n\nプレイヤーのHPが0になりました。";
        }

        /// <summary>
        /// 破棄時にイベント購読を解除する。
        /// </summary>
        private void OnDestroy()
        {
            if (rewardController != null)
            {
                rewardController.RewardScreenOpened -= OnRewardScreenOpened;
            }

            if (runController != null)
            {
                runController.StageStarted -= OnStageStarted;
                runController.RouteChoiceRequested -= OnRouteChoiceRequested;
                runController.OverhuntChoiceRequested -= OnOverhuntChoiceRequested;
                runController.TownOpened -= OnTownOpened;
                runController.RunStateChanged -= OnRunStateChanged;
                runController.RunWon -= OnRunWon;
                runController.RunLost -= OnRunLost;
            }
        }
    }
}

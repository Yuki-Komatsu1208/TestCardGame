using System;
using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Character;
using TestCardGame.Character.Player;
using TestCardGame.Character.ValueObjects;
using TestCardGame.BoardManage;
using TestCardGame.Controller.Services;
using TestCardGame.Cards.Core;

namespace TestCardGame.Controller
{

    /// <summary>
    /// ゲーム全体の操作を受け付け、各サービスへ処理を委譲するコントローラー。
    /// </summary>
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        [SerializeField] private CellBuilder cellBuilder;

        private DamageService damageService;
        private StatusEffectService statusEffectService;
        private UnitMoveService moveService;
        private ViewMoveService viewMoveService;
        // GameControllerは操作の入口に寄せ、具体的な処理は各サービスへ分離する。
        private BoardTargetingService targetingService;
        private BoardVisualService boardVisualService;
        private CellEffectService cellEffectService;
        private CardPlayService cardPlayService;
        private TurnService turnService;
        private readonly Dictionary<UnitID, IUnit> unitsById = new();
        private readonly Dictionary<UnitID, UnitView> viewByUnitId = new();
        private readonly Dictionary<Vector2Int, RectTransform> cellRects = new();
        private bool initialized;

        public event Action<UnitID, Vector2Int, Vector2Int> MoveStarted;
        public event Action<UnitID, Vector2Int, Vector2Int> MoveCompleted;
        public event Action<UnitID, Vector2Int, string> MoveRejected;
        public event Action<BattleResult> BattleEnded;

        public string CurrentStageName { get; private set; } = "予備ステージ";
        public BattleResult CurrentBattleResult { get; private set; } = BattleResult.None;
        public BattleState CurrentBattleState { get; private set; } = BattleState.Playing;

        public IReadOnlyList<TestCardGame.Character.Enemies.IEnemy> Enemies
        {
            get
            {
                var list = new List<TestCardGame.Character.Enemies.IEnemy>();
                if (cellBuilder != null && cellBuilder.Enemies != null)
                {
                    foreach (var e in cellBuilder.Enemies)
                    {
                        list.Add(e);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// コントローラーの初期化を行う（互換用）
        /// </summary>
        public bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            if (cellBuilder == null)
            {
                cellBuilder = FindAnyObjectByType<CellBuilder>();
            }
            if (cellBuilder == null)
            {
                Debug.LogError("GameController: シーン内に CellBuilder が見つかりません。", this);
                return false;
            }

            if (!cellBuilder.Initialize())
            {
                Debug.LogError("GameController: CellBuilder の初期化に失敗しました。", this);
                return false;
            }

            return InitializeStageInternal(null, null);
        }

        /// <summary>
        /// 特定のステージ定義とラン情報を使ってゲームをセットアップします。
        /// </summary>
        public bool InitializeStage(Stage.StageDefinitionSO stageDef, Run.RunState runState)
        {
            if (cellBuilder == null)
            {
                cellBuilder = FindAnyObjectByType<CellBuilder>();
            }
            if (cellBuilder == null)
            {
                Debug.LogError("GameController: シーン内に CellBuilder が見つかりません。", this);
                return false;
            }

            if (!cellBuilder.InitializeStage(stageDef, runState))
            {
                Debug.LogError("GameController: CellBuilder のステージ初期化に失敗しました。", this);
                return false;
            }

            return InitializeStageInternal(stageDef, runState);
        }

        private bool InitializeStageInternal(Stage.StageDefinitionSO stageDef, Run.RunState runState)
        {
            CurrentStageName = stageDef != null ? stageDef.stageName : "予備ステージ";
            CurrentBattleResult = BattleResult.None;
            CurrentBattleState = BattleState.Playing;

            var board = cellBuilder.Board;
            if (board == null)
            {
                Debug.LogError("GameController: Board が初期化されていません。", this);
                return false;
            }

            // 既存の移動サービス購読があれば解除する。
            if (moveService != null)
            {
                moveService.MoveStarted -= OnMoveStarted;
                moveService.MoveCompleted -= OnMoveCompleted;
                moveService.MoveRejected -= OnMoveRejected;
            }

            unitsById.Clear();
            unitsById[cellBuilder.Player.ID] = cellBuilder.Player;
            foreach (var enemy in cellBuilder.Enemies)
            {
                unitsById[enemy.ID] = enemy;
            }

            viewByUnitId.Clear();
            viewByUnitId[cellBuilder.Player.ID] = cellBuilder.PlayerUnitView;
            foreach (var kvp in cellBuilder.EnemyUnitViews)
            {
                viewByUnitId[kvp.Key] = kvp.Value;
            }

            cellRects.Clear();
            foreach (var entry in cellBuilder.CellRects)
            {
                cellRects[entry.Key] = entry.Value;
            }

            damageService = new DamageService();
            statusEffectService = new StatusEffectService(damageService);

            moveService = new UnitMoveService(board, unitsById, statusEffectService);
            moveService.MoveStarted += OnMoveStarted;
            moveService.MoveCompleted += OnMoveCompleted;
            moveService.MoveRejected += OnMoveRejected;

            if (viewMoveService != null)
            {
                viewMoveService.Dispose();
            }
            viewMoveService = new ViewMoveService(cellRects, viewByUnitId, unitsById);
            viewMoveService.Bind(moveService);

            targetingService = new BoardTargetingService(cellRects);
            boardVisualService = new BoardVisualService(cellRects);
            cellEffectService = new CellEffectService();
            cardPlayService = new CardPlayService(moveService, targetingService, statusEffectService);
            turnService = new TurnService(board, moveService, cellEffectService, statusEffectService);

            initialized = true;
            viewMoveService.SyncAllViewsFromModel();

            // ユニットの初期位置を配置する。
            var placed = moveService.RequestMoveAbsolute(cellBuilder.Player.ID, new Vector2Int(2, 1));
            if (!placed)
            {
                Debug.LogWarning("GameController: プレイヤーを (2,1) に配置できませんでした。(0,0) に配置します。");
                moveService.RequestMoveAbsolute(cellBuilder.Player.ID, new Vector2Int(0, 0));
            }

            if (stageDef != null && stageDef.enemies != null)
            {
                for (int i = 0; i < cellBuilder.Enemies.Count; i++)
                {
                    var enemy = cellBuilder.Enemies[i];
                    var spawnPos = stageDef.enemies[i].position;
                    var placedEnemy = moveService.RequestMoveAbsolute(enemy.ID, spawnPos);
                    if (!placedEnemy)
                    {
                        Debug.LogError($"GameController: 敵 {enemy.Name} を {spawnPos} に配置できませんでした。");
                    }
                }
            }
            else
            {
                var placedEnemy = moveService.RequestMoveAbsolute(cellBuilder.Enemy.ID, new Vector2Int(2, 3));
                if (!placedEnemy)
                {
                    Debug.LogError("GameController: 敵を (2,3) に配置できませんでした。");
                }
            }

            InitializeBattleUI();

            RefreshBattleViews();

            return true;
        }

        [SerializeField] private BattleUI battleUIInstance;

        /// <summary>
        /// 戦闘盤面を開始せず、街やRUN開始時に必要なUIだけを初期化する。
        /// </summary>
        public void InitializeBattleUI()
        {
            if (battleUIInstance == null)
            {
                battleUIInstance = GetComponent<BattleUI>();
                if (battleUIInstance == null)
                {
                    battleUIInstance = FindAnyObjectByType<BattleUI>();
                }
                if (battleUIInstance == null)
                {
                    battleUIInstance = gameObject.AddComponent<BattleUI>();
                }
            }

            battleUIInstance.Initialize(this);
        }

        public bool IsPlayerTurn => turnService?.IsPlayerTurn ?? true;
        public PlayerUnit PlayerUnitInstance => cellBuilder?.Player as PlayerUnit;
        public TestCardGame.Character.Enemies.IEnemy EnemyUnitInstance => cellBuilder?.Enemy as TestCardGame.Character.Enemies.IEnemy;

        /// <summary>
        /// ドロップ位置を対象としてカードを使用する。
        /// </summary>
        public bool UseCardAtDropScreenPosition(CardBase card, Vector2 screenPosition)
        {
            var player = PlayerUnitInstance;
            if (turnService == null || cardPlayService == null || !turnService.CanPlayCard(card, player))
            {
                return false;
            }

            if (!cardPlayService.TryPlayCard(card, player, screenPosition))
            {
                return false;
            }

            // プレイヤーの攻撃アニメーションをトリガーする
            if (viewByUnitId.TryGetValue(player.ID, out var pView))
            {
                pView.PlayAttack();
            }

            turnService.MarkCardPlayed(card, player);

            CheckBattleResolution();

            RefreshBattleViews();

            return true;
        }

        /// <summary>
        /// バトル中に敵を動的に召喚する。
        /// </summary>
        public void SpawnEnemyDuringBattle(Character.Enemies.EnemyDefinitionSO enemyDefinition, Vector2Int position)
        {
            if (cellBuilder == null) return;

            // IDが重複しないようにEnemiesのカウントに下限100をオフセット
            int nextIndex = Enemies.Count + 101;
            var enemy = cellBuilder.SpawnEnemyDynamically(enemyDefinition, position, nextIndex);

            unitsById[enemy.ID] = enemy;
            if (cellBuilder.EnemyUnitViews.TryGetValue(enemy.ID, out var view))
            {
                viewByUnitId[enemy.ID] = view;
            }

            // 盤面モデルに配置してView位置を同期する
            moveService.RequestMoveAbsolute(enemy.ID, position);
            viewMoveService?.SyncAllViewsFromModel();

            RefreshBattleViews();
        }

        /// <summary>
        /// 指定したユニットの攻撃アニメーションを再生する。
        /// </summary>
        public void PlayUnitAttackAnimation(UnitID unitId)
        {
            if (viewByUnitId.TryGetValue(unitId, out var view))
            {
                view.PlayAttack();
            }
        }

        /// <summary>
        /// プレイヤーターンを終了し、生存している敵の行動を実行する。
        /// </summary>
        public void EndPlayerTurn()
        {
            if (turnService == null)
            {
                return;
            }

            var activeEnemiesList = new List<TestCardGame.Character.Enemies.IEnemy>();
            foreach (var enemy in Enemies)
            {
                if (enemy != null && enemy.Hp.CurrentValue > 0)
                {
                    activeEnemiesList.Add(enemy);
                }
            }

            if (!turnService.EndPlayerTurn(activeEnemiesList, PlayerUnitInstance))
            {
                return;
            }

            CheckBattleResolution();

            RefreshBattleViews();
        }

        /// <summary>
        /// 現在のHP状況からバトルの勝敗を判定する。
        /// </summary>
        public void CheckBattleResolution()
        {
            if (CurrentBattleResult != BattleResult.None)
            {
                return;
            }

            if (PlayerUnitInstance == null || PlayerUnitInstance.Hp.CurrentValue <= 0)
            {
                CurrentBattleResult = BattleResult.Lose;
                CurrentBattleState = BattleState.Lost;
                BattleEnded?.Invoke(BattleResult.Lose);
                return;
            }

            bool anyEnemiesAlive = false;
            foreach (var enemy in Enemies)
            {
                if (enemy != null && enemy.Hp.CurrentValue > 0)
                {
                    anyEnemiesAlive = true;
                    break;
                }
            }

            if (!anyEnemiesAlive)
            {
                CurrentBattleResult = BattleResult.Win;
                CurrentBattleState = BattleState.Won;
                BattleEnded?.Invoke(BattleResult.Win);
            }
        }

        /// <summary>
        /// HPが0以下になった敵を盤面と表示から取り除く。
        /// </summary>
        private void HandleUnitDeaths()
        {
            foreach (var enemy in Enemies)
            {
                if (enemy != null && enemy.Hp.CurrentValue <= 0)
                {
                    var pos = enemy.Position;
                    if (cellBuilder.Board.IsInside(pos.x, pos.y))
                    {
                        var cell = cellBuilder.Board.GetCell(pos.x, pos.y);
                        if (cell.Occupant == enemy)
                        {
                            cell.Clear();
                        }
                    }

                    if (viewByUnitId.TryGetValue(enemy.ID, out var view))
                    {
                        if (view != null && view.gameObject.activeSelf)
                        {
                            view.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 盤面セルの表示を現在のモデル状態に同期する。
        /// </summary>
        public void SyncCellVisuals()
        {
            boardVisualService?.SyncCellVisuals(cellBuilder?.Board);
        }

        /// <summary>
        /// ユニットの移動をリクエストする
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="relativeDelta">現在座標からの相対移動量</param>
        /// <returns></returns>
        public bool RequestMove(UnitID unitId, Vector2Int relativeDelta)
        {
            if (!initialized)
            {
                MoveRejected?.Invoke(unitId, relativeDelta, "コントローラーが初期化されていません。");
                return false;
            }

            if (moveService == null)
            {
                MoveRejected?.Invoke(unitId, relativeDelta, "移動サービスが初期化されていません。");
                return false;
            }

            return moveService.RequestMoveRelative(unitId, relativeDelta);
        }

        /// <summary>
        /// 画面上の座標から、プレイヤーの移動をリクエストする。最も近いセルに向かって移動する。
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <param name="moveCount"></param>
        /// <returns></returns>
        public bool RequestPlayerMoveByDropScreenPosition(Vector2 screenPosition, int moveCount = 1)
        {
            if (!initialized || cellBuilder?.Player == null)
            {
                return false;
            }

            if (targetingService == null || !targetingService.TryGetClosestCellPosition(screenPosition, out var targetCellPosition))
            {
                return false;
            }

            var playerPosition = cellBuilder.Player.Position;
            var direction = targetCellPosition - playerPosition;
            return RequestMoveByDirection(cellBuilder.Player.ID, direction, moveCount);
        }
        /// <summary>
        /// ユニットの移動をリクエストする（方向指定版）
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="direction"></param>
        /// <param name="moveCount"></param>
        /// <returns></returns>
        public bool RequestMoveByDirection(UnitID unitId, Vector2Int direction, int moveCount = 1)
        {
            if (!initialized)
            {
                MoveRejected?.Invoke(unitId, default, "コントローラーが初期化されていません。");
                return false;
            }

            if (moveCount <= 0)
            {
                MoveRejected?.Invoke(unitId, default, "移動数は1以上である必要があります。");
                return false;
            }

            var offset = BoardTargetingService.NormalizeCardinalDirection(direction);
            if (offset == Vector2Int.zero)
            {
                MoveRejected?.Invoke(unitId, offset, "移動方向を指定してください。");
                return false;
            }

            return RequestMove(unitId, offset * moveCount);
        }
       

        /// <summary>
        /// 現在のプレイヤーが所持しているカード一覧を返す。
        /// </summary>
        public IReadOnlyList<CardBase> GetPlayerCards()
        {
            if (cellBuilder?.Player is PlayerUnit player)
            {
                return player.Cards;
            }

            return Array.Empty<CardBase>();
        }
        /// <summary>
        /// 戦闘関連の表示をまとめて更新する。
        /// </summary>
        private void RefreshBattleViews()
        {
            HandleUnitDeaths();
            SyncCellVisuals();
            if (battleUIInstance != null)
            {
                battleUIInstance.Refresh();
            }
        }
        /// <summary>
        /// ユニットの移動開始を通知する
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void OnMoveStarted(UnitID unitId, Vector2Int from, Vector2Int to)
        {
            MoveStarted?.Invoke(unitId, from, to);
        }
        /// <summary>
        /// ユニットの移動完了を通知する
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void OnMoveCompleted(UnitID unitId, Vector2Int from, Vector2Int to)
        {
            MoveCompleted?.Invoke(unitId, from, to);
        }
        /// <summary>
        /// ユニットの移動拒否を通知する
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="to"></param>
        /// <param name="reason"></param>
        private void OnMoveRejected(UnitID unitId, Vector2Int to, string reason)
        {
            MoveRejected?.Invoke(unitId, to, reason);
        }
        
        /// <summary>
        /// コントローラーが破棄される際のクリーンアップ処理
        /// </summary>
        private void OnDestroy()
        {
            if (moveService != null)
            {
                moveService.MoveStarted -= OnMoveStarted;
                moveService.MoveCompleted -= OnMoveCompleted;
                moveService.MoveRejected -= OnMoveRejected;
            }

            if (viewMoveService != null)
            {
                viewMoveService.Dispose();
            }
        }
    }
}

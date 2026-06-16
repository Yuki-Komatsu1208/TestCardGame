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
        [SerializeField] private CellBuilder cellBuilder;

        private UnitMoveService moveService;
        private ViewMoveService viewMoveService;
        // GameControllerは操作の入口に寄せ、具体的な処理は各サービスへ分離する。
        private BoardTargetingService targetingService;
        private BoardVisualService boardVisualService;
        private CellEffectService cellEffectService;
        private CardPlayService cardPlayService;
        private TurnService turnService;
        private readonly Dictionary<UnitID, IUnit> unitsById = new();
        private readonly Dictionary<int, UnitView> viewByCharacterCode = new();
        private readonly Dictionary<Vector2Int, RectTransform> cellRects = new();
        private bool initialized;

        public event Action<UnitID, Vector2Int, Vector2Int> MoveStarted;
        public event Action<UnitID, Vector2Int, Vector2Int> MoveCompleted;
        public event Action<UnitID, Vector2Int, string> MoveRejected;

        /// <summary>
        /// コントローラーの初期化を行う
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            if (cellBuilder == null)
            {
                cellBuilder = FindFirstObjectByType<CellBuilder>();
            }
            if (cellBuilder == null)
            {
                Debug.LogError("GameController: CellBuilder was not found in scene.", this);
                return false;
            }

            if (!cellBuilder.Initialize())
            {
                Debug.LogError("GameController: CellBuilder initialization failed.", this);
                return false;
            }

            var board = cellBuilder.Board;
            if (board == null)
            {
                Debug.LogError("GameController: Board was not initialized.", this);
                return false;
            }

            unitsById.Clear();
            unitsById[cellBuilder.Player.ID] = cellBuilder.Player;
            unitsById[cellBuilder.Enemy.ID] = cellBuilder.Enemy;

            viewByCharacterCode.Clear();
            viewByCharacterCode[cellBuilder.Player.ID.CharaID.Code] = cellBuilder.PlayerUnitView;
            viewByCharacterCode[cellBuilder.Enemy.ID.CharaID.Code] = cellBuilder.EnemyUnitView;

            cellRects.Clear();
            foreach (var entry in cellBuilder.CellRects)
            {
                cellRects[entry.Key] = entry.Value;
            }

            moveService = new UnitMoveService(board, unitsById);
            moveService.MoveStarted += OnMoveStarted;
            moveService.MoveCompleted += OnMoveCompleted;
            moveService.MoveRejected += OnMoveRejected;

            viewMoveService = new ViewMoveService(cellRects, viewByCharacterCode, unitsById);
            viewMoveService.Bind(moveService);

            targetingService = new BoardTargetingService(cellRects);
            boardVisualService = new BoardVisualService(cellRects);
            cellEffectService = new CellEffectService();
            cardPlayService = new CardPlayService(moveService, targetingService);
            turnService = new TurnService(board, moveService, cellEffectService);

            initialized = true;
            viewMoveService.SyncAllViewsFromModel();

            var placed = moveService.RequestMoveAbsolute(cellBuilder.Player.ID, new Vector2Int(2, 1));
            if (!placed)
            {
                Debug.LogError("GameController: failed to place player at initial position (2,1).", this);
            }

            var placedEnemy = moveService.RequestMoveAbsolute(cellBuilder.Enemy.ID, new Vector2Int(2, 3));
            if (!placedEnemy)
            {
                Debug.LogError("GameController: failed to place enemy at initial position (2,3).", this);
            }

            battleUIInstance = gameObject.AddComponent<BattleUI>();
            battleUIInstance.Initialize(this);

            return true;
        }

        private BattleUI battleUIInstance;

        public bool IsPlayerTurn => turnService?.IsPlayerTurn ?? true;
        public PlayerUnit PlayerUnitInstance => cellBuilder?.Player as PlayerUnit;
        public TestCardGame.Character.Enemies.IEnemy EnemyUnitInstance => cellBuilder?.Enemy as TestCardGame.Character.Enemies.IEnemy;

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

            turnService.MarkCardPlayed(card, player);

            RefreshBattleViews();

            return true;
        }

        public void EndPlayerTurn()
        {
            if (turnService == null)
            {
                return;
            }

            if (!turnService.EndPlayerTurn(EnemyUnitInstance, PlayerUnitInstance))
            {
                return;
            }

            RefreshBattleViews();
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
                MoveRejected?.Invoke(unitId, relativeDelta, "Controller is not initialized.");
                return false;
            }

            if (moveService == null)
            {
                MoveRejected?.Invoke(unitId, relativeDelta, "Move service is not initialized.");
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
                MoveRejected?.Invoke(unitId, default, "Controller is not initialized.");
                return false;
            }

            if (moveCount <= 0)
            {
                MoveRejected?.Invoke(unitId, default, "Move count must be positive.");
                return false;
            }

            var offset = BoardTargetingService.NormalizeCardinalDirection(direction);
            if (offset == Vector2Int.zero)
            {
                MoveRejected?.Invoke(unitId, offset, "Direction must not be zero.");
                return false;
            }

            return RequestMove(unitId, offset * moveCount);
        }
       

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

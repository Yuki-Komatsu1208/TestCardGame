using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestCardGame.Character;
using TestCardGame.Character.Player;
using TestCardGame.Character.Enemies;
using TestCardGame.Character.ValueObjects;
using TestCardGame.Character.StatusVO;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.VOs;
using TestCardGame.Stage;

namespace TestCardGame.BoardManage
{
    public class CellBuilder : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private RectTransform playerView;
        [SerializeField] private PlayerDefinitionSO playerDefinition;
        [SerializeField] private EnemyDefinitionSO enemyDefinition;

        private Board board;
        private UnitView playerUnitView;

        private readonly Dictionary<Vector2Int, RectTransform> cellRects = new();
        private PlayerUnit playerUnit;
        private bool initialized;
        public event System.Action<int, int> CellClicked;

        private readonly List<EnemyUnit> enemies = new();
        private readonly Dictionary<UnitID, UnitView> enemyUnitViews = new();

        public IReadOnlyList<EnemyUnit> Enemies => enemies;
        public IReadOnlyDictionary<UnitID, UnitView> EnemyUnitViews => enemyUnitViews;

        public bool IsInitialized => initialized;
        public Board Board => board;
        public IReadOnlyDictionary<Vector2Int, RectTransform> CellRects => cellRects;
        public IUnit Player => playerUnit;
        public UnitView PlayerUnitView => playerUnitView;
        public IUnit Enemy => enemies.Count > 0 ? enemies[0] : null;
        public UnitView EnemyUnitView => enemies.Count > 0 && enemyUnitViews.TryGetValue(enemies[0].ID, out var view) ? view : null;

        /// <summary>
        /// シリアライズされた単体敵情報から互換用の初期ステージを作成して初期化する。
        /// </summary>
        public bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            if (cellPrefab == null || playerView == null || playerDefinition == null || enemyDefinition == null)
            {
                Debug.LogError("CellBuilder: 必要なシリアライズ項目が設定されていません。", this);
                return false;
            }

            // シリアライズされた単体敵から、互換用のデフォルトステージを作成する。
            var defaultStage = ScriptableObject.CreateInstance<StageDefinitionSO>();
            defaultStage.stageName = "予備ステージ";
            defaultStage.boardSize = new Vector2Int(5, 5);
            defaultStage.enemies = new List<EnemySpawnDefinition>
            {
                new EnemySpawnDefinition { enemy = enemyDefinition, position = new Vector2Int(2, 3) }
            };

            return InitializeStage(defaultStage, null);
        }

        /// <summary>
        /// ステージ定義とRun状態をもとに盤面、プレイヤー、敵ビューを再構築する。
        /// </summary>
        public bool InitializeStage(StageDefinitionSO stageDef, Run.RunState runState)
        {
            // 以前のセルと敵ビューを破棄する。
            foreach (var cellObj in cellRects.Values)
            {
                if (cellObj != null)
                {
                    Destroy(cellObj.gameObject);
                }
            }
            cellRects.Clear();

            foreach (var view in enemyUnitViews.Values)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }
            enemyUnitViews.Clear();
            enemies.Clear();

            // プレイヤーユニットを作成する。
            if (runState != null)
            {
                var cards = new List<CardBase>();
                foreach (var entry in runState.playerDeck)
                {
                    if (entry != null && entry.card != null)
                    {
                        cards.Add(new CardBase(entry.card, new CardLevel(entry.level)));
                    }
                }
                playerUnit = new PlayerUnit(UnitID.defaultPlayerUnit, playerDefinition.playerName, new HP(playerDefinition.maxHp), new Vector2Int(0, 0), cards);
                playerUnit.Hp.TakeDamage(playerDefinition.maxHp - runState.currentHp);
            }
            else
            {
                playerUnit = new PlayerUnit(UnitID.defaultPlayerUnit, playerDefinition, new Vector2Int(0, 0));
            }

            // 盤面を作成する。
            int boardW = stageDef != null ? stageDef.boardSize.x : 5;
            int boardH = stageDef != null ? stageDef.boardSize.y : 5;
            board = new Board(boardW, boardH);

            for (int y = 0; y < boardH; y++)
            {
                for (int x = 0; x < boardW; x++)
                {
                    var obj = Instantiate(cellPrefab, transform);
                    var position = new Vector2Int(x, boardH - 1 - y);
                    obj.name = $"Cell {position.x},{position.y}";

                    if (obj.TryGetComponent<RectTransform>(out var rect))
                    {
                        cellRects[position] = rect;
                    }
                    else
                    {
                        Debug.LogError($"CellBuilder: セル {position} に RectTransform がありません。", obj);
                    }

                    RegisterCellClick(obj, position.x, position.y);
                }
            }

            PreparePlayerView();

            // ステージ定義に基づいて敵を作成する。
            if (stageDef != null && stageDef.enemies != null)
            {
                int enemyIndex = 1;
                foreach (var spawn in stageDef.enemies)
                {
                    if (spawn == null || spawn.enemy == null) continue;

                    CharacterID charID = CharacterID.Slime;
                    if (CharacterID.characterIDs.TryGetValue(spawn.enemy.characterCode, out var cid))
                    {
                        charID = cid;
                    }

                    var unitID = new UnitID(enemyIndex++, charID);
                    var enemy = new EnemyUnit(unitID, spawn.enemy, spawn.position);
                    enemies.Add(enemy);

                    var eview = Instantiate(playerView, playerView.parent);
                    eview.gameObject.name = $"EnemyView_{enemy.Name}_{unitID.Code}";
                    eview.gameObject.SetActive(true);

                    if (!eview.TryGetComponent<UnitView>(out var eUnitView))
                    {
                        eUnitView = eview.gameObject.AddComponent<UnitView>();
                    }

                    if (eview.TryGetComponent<Image>(out var image))
                    {
                        image.enabled = true;
                        image.color = spawn.enemy.enemyColor;
                    }

                    var textComp = eview.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = spawn.enemy.enemyName[0].ToString();
                    }

                    eUnitView.Initialize(null);
                    enemyUnitViews[unitID] = eUnitView;
                }
            }
            else
            {
                // 予備処理として単体敵を作成する。
                CharacterID charID = CharacterID.Slime;
                if (CharacterID.characterIDs.TryGetValue(enemyDefinition.characterCode, out var cid))
                {
                    charID = cid;
                }
                var unitID = new UnitID(1, charID);
                var enemy = new EnemyUnit(unitID, enemyDefinition, new Vector2Int(0, 0));
                enemies.Add(enemy);

                PrepareEnemyView_Fallback();
            }

            initialized = true;
            return true;
        }

        /// <summary>
        /// 互換用の単体敵ビューを作成する。
        /// </summary>
        private void PrepareEnemyView_Fallback()
        {
            var eview = Instantiate(playerView, playerView.parent);
            eview.gameObject.name = "EnemyView";
            eview.gameObject.SetActive(true);

            if (!eview.TryGetComponent<UnitView>(out var eUnitView))
            {
                eUnitView = eview.gameObject.AddComponent<UnitView>();
            }

            if (eview.TryGetComponent<Image>(out var image))
            {
                image.enabled = true;
                if (enemyDefinition != null)
                {
                    image.color = enemyDefinition.enemyColor;
                }
                else
                {
                    image.color = new Color(0.957f, 0.263f, 0.212f, 1f);
                }
            }

            var textComp = eview.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                if (enemyDefinition != null && !string.IsNullOrEmpty(enemyDefinition.enemyName))
                {
                    textComp.text = enemyDefinition.enemyName[0].ToString();
                }
                else
                {
                    textComp.text = "E";
                }
            }

            eUnitView.Initialize(null);
            enemyUnitViews[enemies[0].ID] = eUnitView;
        }

        /// <summary>
        /// セルのButtonクリックを座標イベントへ変換する。
        /// </summary>
        private void RegisterCellClick(GameObject cellObject, int x, int y)
        {
            if (!cellObject.TryGetComponent<Button>(out var button))
            {
                Debug.LogError("CellBuilder: セルPrefabに Button コンポーネントがありません。", cellObject);
                return;
            }

            button.onClick.AddListener(() =>
            {
                if (!initialized)
                {
                    return;
                }
                CellClicked?.Invoke(x, y);
            });
        }

        /// <summary>
        /// プレイヤー用ビューを初期化し、画像と表示文字を設定する。
        /// </summary>
        private void PreparePlayerView()
        {
            playerView.gameObject.SetActive(true);

            if (!playerView.TryGetComponent<UnitView>(out playerUnitView))
            {
                playerUnitView = playerView.gameObject.AddComponent<UnitView>();
            }

            if (playerView.TryGetComponent<Image>(out var image))
            {
                image.enabled = true;
                var color = image.color;
                if (color.a <= 0f)
                {
                    color.a = 1f;
                    image.color = color;
                }
                if (image.sprite == null)
                {
                    image.sprite = playerDefinition != null ? playerDefinition.playerSprite : null;
                }
            }

            var textComp = playerView.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = "P";
            }

            playerUnitView.Initialize(playerDefinition != null ? playerDefinition.playerSprite : null);
        }
    }
}

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

    private RectTransform enemyView;
    private UnitView enemyUnitView;
    private EnemyUnit enemyUnit;

    private readonly Dictionary<Vector2Int, RectTransform> cellRects = new();
    private PlayerUnit playerUnit;
    private bool initialized;
    public event System.Action<int, int> CellClicked;

    private const int W = 5;
    private const int H = 5;

    public bool IsInitialized => initialized;
    public Board Board => board;
    public IReadOnlyDictionary<Vector2Int, RectTransform> CellRects => cellRects;
    public IUnit Player => playerUnit;
    public UnitView PlayerUnitView => playerUnitView;
    public IUnit Enemy => enemyUnit;
    public UnitView EnemyUnitView => enemyUnitView;

    public bool Initialize()
    {
        if (initialized)
        {
            return true;
        }

        if (cellPrefab == null)
        {
            Debug.LogError("CellBuilder: cellPrefab is not assigned. Set it in the Inspector.", this);
            return false;
        }
        if (playerView == null)
        {
            Debug.LogError("CellBuilder: playerView is not assigned. Set it in the Inspector.", this);
            return false;
        }
        if (playerDefinition == null)
        {
            Debug.LogError("CellBuilder: playerDefinition is not assigned. Set it in the Inspector.", this);
            return false;
        }
        if (enemyDefinition == null)
        {
            Debug.LogError("CellBuilder: enemyDefinition is not assigned. Set it in the Inspector.", this);
            return false;
        }

        playerUnit = new PlayerUnit(UnitID.defaultPlayerUnit, playerDefinition, new Vector2Int(0, 0));

        CharacterID charID = CharacterID.Slime;
        if (CharacterID.characterIDs.TryGetValue(enemyDefinition.characterCode, out var cid))
        {
            charID = cid;
        }
        var unitID = new UnitID(1, charID);
        enemyUnit = new EnemyUnit(unitID, enemyDefinition, new Vector2Int(0, 0));

        board = new Board(W, H);
        BuildBoard();
        PreparePlayerView();
        PrepareEnemyView();
        initialized = true;
        return true;
    }

    private void PrepareEnemyView()
    {
        enemyView = Instantiate(playerView, playerView.parent);
        enemyView.gameObject.name = "EnemyView";
        enemyView.gameObject.SetActive(true);

        if (!enemyView.TryGetComponent<UnitView>(out enemyUnitView))
        {
            enemyUnitView = enemyView.gameObject.AddComponent<UnitView>();
        }

        if (enemyView.TryGetComponent<Image>(out var image))
        {
            image.enabled = true;
            if (enemyDefinition != null)
            {
                image.color = enemyDefinition.enemyColor;
            }
            else
            {
                image.color = new Color(0.957f, 0.263f, 0.212f, 1f); // Fallback Red
            }
        }

        var textComp = enemyView.GetComponentInChildren<TextMeshProUGUI>();
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

        enemyUnitView.Initialize(null);
    }

    private void BuildBoard()
    {
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                var obj = Instantiate(cellPrefab, transform);
                var position = new Vector2Int(x, H - 1 - y);
                obj.name = $"Cell {position.x},{position.y}";


                if (obj.TryGetComponent<RectTransform>(out var rect))
                {
                    cellRects[position] = rect;
                }
                else
                {
                    Debug.LogError($"CellBuilder: RectTransform is missing on cell {position}.", obj);
                }
            }
        }
    }

    private void RegisterCellClick(GameObject cellObject, int x, int y)
    {
        if (!cellObject.TryGetComponent<Button>(out var button))
        {
            Debug.LogError("CellBuilder: Cell prefab is missing a Button component.", cellObject);
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

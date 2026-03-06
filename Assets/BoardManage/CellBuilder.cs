using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TestCardGame.Charactor;
using TestCardGame.Charactor.Player;
namespace TestCardGame.BoardManage
{

public class CellBuilder : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private RectTransform playerView;
    [SerializeField] private Sprite playerSprite;

    private Board board;
    private UnitView playerUnitView;

    private readonly Dictionary<Vector2Int, RectTransform> cellRects = new();
    private readonly PlayerUnit playerUnit = PlayerUnit.defaultPlayer;
    private bool initialized;
    public event System.Action<int, int> CellClicked;

    private const int W = 5;
    private const int H = 5;

    public bool IsInitialized => initialized;
    public Board Board => board;
    public IReadOnlyDictionary<Vector2Int, RectTransform> CellRects => cellRects;
    public IUnit Player => playerUnit;
    public UnitView PlayerUnitView => playerUnitView;

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

        board = new Board(W, H);
        BuildBoard();
        PreparePlayerView();

        PlacePlayerAt(2, 2);
        initialized = true;
        return true;
    }

    private void BuildBoard()
    {
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                var obj = Instantiate(cellPrefab, transform);
                obj.name = $"Cell {x},{y}";

                RegisterCellClick(obj, x, y);

                var position = new Vector2Int(x, y);
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

    public void PlacePlayerAt(int x, int y)
    {
        if (!board.TryMoveUnit(playerUnit, x, y))
        {
            Debug.LogError($"CellBuilder: failed to place player at ({x},{y}).", this);
            return;
        }

        MoveUnitView(playerUnit);
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
                image.sprite = playerSprite;
            }
        }

        playerUnitView.Initialize(playerSprite);
    }

    public void MoveUnitView(IUnit unit)
    {
        if (unit == null)
        {
            return;
        }

        if (!cellRects.TryGetValue(unit.Position, out var cellRect))
        {
            return;
        }

        playerUnitView.MoveToCell(cellRect);
    }
}
}
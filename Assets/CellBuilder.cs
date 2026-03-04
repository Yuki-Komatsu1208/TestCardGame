using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CellBuilder : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Sprite playerSprite;

    private Board board;
    private readonly Dictionary<Vector2Int, GameObject> cellObjects = new();
    private readonly PlayerUnit playerUnit = new();
    private Vector2Int playerPosition;

    private const int W = 5;
    private const int H = 5;
    private const int PlayerX = 0;
    private const int PlayerY = 2;

    /// <summary>
    /// ゲーム開始時にボードを構築し、プレイヤーを初期位置に配置する。
    /// </summary>
    void Start()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("CellBuilder: cellPrefab is not assigned. Set it in the Inspector.", this);
            return;
        }

        board = new Board(W, H);

        BuildBoard();
        PlaceInitialPlayer();
    }

    /// <summary>
    /// セルのプレハブをインスタンス化してボードを構築する。
    /// </summary>
    void BuildBoard()
    {
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                var obj = Instantiate(cellPrefab, transform);
                obj.name = $"Cell {x},{y}";
                InitializeCellView(obj);

                var cellX = x;
                var cellY = y;
                RegisterCellClick(obj, cellX, cellY);

                var position = new Vector2Int(x, y);
                cellObjects[position] = obj;
            }
        }
    }

    /// <summary>
    /// セルの見た目を初期化する。テキストを空にし、占有者の表示を非アクティブにする。
    /// </summary>
    /// <param name="cellObject"></param>
    private static void InitializeCellView(GameObject cellObject)
    {

        var occupant = cellObject.transform.Find("Occupant");
        if (occupant != null)
        {
            occupant.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// セルのクリックイベントを登録する。クリックされたときにプレイヤーをそのセルに移動させる。
    /// </summary>
    /// <param name="cellObject"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void RegisterCellClick(GameObject cellObject, int x, int y)
    {
        var button = cellObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("CellBuilder: Cell prefab is missing a Button component.", cellObject);
            return;
        }

        button.onClick.AddListener(() => MovePlayerTo(x, y));
    }

    /// <summary>
    /// プレイヤーを初期位置に配置する。ボード上のセルにプレイヤーユニットを配置し、そのセルの表示を更新する。
    /// </summary>
    private void PlaceInitialPlayer()
    {
        playerPosition = new Vector2Int(PlayerX, PlayerY);
        board.PlaceUnit(playerUnit, playerPosition.x, playerPosition.y);
        SetPlayerVisible(playerPosition, true);
    }

    private void MovePlayerTo(int x, int y)
    {
        var target = new Vector2Int(x, y);
        if (target == playerPosition)
        {
            return;
        }

        board.GetCell(playerPosition.x, playerPosition.y).Clear();
        SetPlayerVisible(playerPosition, false);

        board.PlaceUnit(playerUnit, target.x, target.y);
        SetPlayerVisible(target, true);
        playerPosition = target;
    }

    private void SetPlayerVisible(Vector2Int position, bool visible)
    {
        if (!cellObjects.TryGetValue(position, out var cellObject))
        {
            Debug.LogError($"CellBuilder: Cell object not found for [{position.x},{position.y}].", this);
            return;
        }

        var occupant = cellObject.transform.Find("Occupant");
        if (occupant == null)
        {
            Debug.LogError("CellBuilder: 'Occupant' child was not found.", cellObject);
            return;
        }

        var image = occupant.GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("CellBuilder: 'Occupant' is missing an Image component.", occupant);
            return;
        }

        if (visible && playerSprite != null)
        {
            image.sprite = playerSprite;
        }

        occupant.gameObject.SetActive(visible);
    }

    private sealed class PlayerUnit : IUnit
    {
    }
}

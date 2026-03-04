using UnityEngine;

public class CellBuilder : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Sprite playerSprite;

    private const int W = 5;
    private const int H = 5;
    private Cell[,] cells;

    void Start()
    {
        cells = new Cell[W, H];

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                cells[x,y] = new Cell(x, y);
            }
        }

        // 左端中央にプレイヤー配置
        cells[0, 2].SetOccupation(playerSprite);
    }
}
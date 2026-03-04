/// <summary>
/// セルをまとめたボードを表すクラス。
/// </summary>
public class Board
{
    public int Width { get; }
    public int Height { get; }

    private readonly Cell[,] cells;

    public Board(int width, int height)
    {
        Width = width;
        Height = height;

        cells = new Cell[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                cells[x, y] = new Cell(x, y);
            }
        }
    }
    /// <summary>
    /// 指定した座標のセルを取得する。
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException"></exception>
    public Cell GetCell(int x, int y)
    {
        if (!IsInside(x, y))
            throw new System.ArgumentOutOfRangeException();

        return cells[x, y];
    }

    /// <summary>
    /// 指定した座標が座標内にあるかを判定する
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < Width &&
               y >= 0 && y < Height;
    }

    /// <summary>
    /// 指定したユニットを指定した座標のセルに配置する。
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <exception cref="System.InvalidOperationException"></exception>
    public void PlaceUnit(IUnit unit, int x, int y)
    {
        var cell = GetCell(x, y);

        if (!cell.IsEmpty)
            throw new System.InvalidOperationException("Cell is occupied.");

        cell.Place(unit);
    }
}

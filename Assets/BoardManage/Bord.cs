using TestCardGame.Charactor;
using UnityEngine;
namespace TestCardGame.BoardManage
{

/// <summary>
/// セルをまとめたボードを表すクラス。
/// </summary>
public class Board
{
    public int Width { get; }
    public int Height { get; }

    private readonly Cell[,] cells;

    /// <summary>
    /// 指定した幅と高さでボードを初期化する。
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
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
    public bool TryMoveUnit(IUnit unit, int x, int y)
    {
        if (!IsInside(x, y))
        {
            return false;
        }

        var from = unit.Position;
        if (from.x == x && from.y == y)
        {
            return false;
        }

        var targetCell = GetCell(x, y);
        if (!targetCell.CanMove)
        {
            return false;
        }

        if (IsInside(from.x, from.y))
        {
            var currentCell = GetCell(from.x, from.y);
            if (currentCell.Occupant == unit)
            {
                currentCell.Clear();
            }
        }

        targetCell.Place(unit);
        unit.MoveTo(x, y);
        return true;
    }
}
}
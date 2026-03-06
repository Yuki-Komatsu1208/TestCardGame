using TestCardGame.Charactor;

namespace TestCardGame.BoardManage
{

/// <summary>
/// 各セルを表すクラス。
/// </summary>
public class Cell
{
    public int X { get; }
    public int Y { get; }

    public IUnit Occupant { get; private set; }

    public Cell(int x, int y)
    {
        X = x;
        Y = y;
    }
    /// <summary>
    /// セルにユニットが配置されていないか
    /// </summary>
    public bool IsEmpty => Occupant == null;
    /// <summary>
    /// セルが壁かどうか
    /// </summary>
    public bool IsWall {get;set;}
    public bool CanMove => !IsWall && IsEmpty;

    /// <summary>
    /// セルにユニットを配置
    /// </summary>
    /// <param name="unit"></param>
    public void Place(IUnit unit)
    {
        if(!IsEmpty)
        {
            throw new System.InvalidOperationException("セルにはユニットが既に配置されています。");
        }

        Occupant = unit;
    }
    /// <summary>
    /// セルからユニットを取り除く
    /// </summary>
    public void Clear()
    {
        Occupant = null;
    }

}
}
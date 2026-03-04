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

    public bool IsEmpty => Occupant == null;

    public void Place(IUnit unit)
    {
        Occupant = unit;
    }

    public void Clear()
    {
        Occupant = null;
    }
}

using TestCardGame.Character;
using UnityEngine;

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

    public int FireTurns { get; private set; }
    public int FireDamage { get; private set; }
    public bool IsOnFire => FireTurns > 0;

    public void ApplyFire(int turns, int damage)
    {
        FireTurns = turns;
        FireDamage = damage;
    }

    public void TickFire(out int damageToOccupant)
    {
        damageToOccupant = 0;
        if (FireTurns > 0)
        {
            FireTurns--;
            if (Occupant != null)
            {
                damageToOccupant = FireDamage;
            }
        }
        if (FireTurns == 0)
        {
            FireDamage = 0;
        }
    }

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
    /// セルのツールチップテキストを取得
    /// </summary>
    public string TooltipText
    {
        get
        {
            if (IsWall)
            {
                return "壁";
            }
            else if (IsEmpty)
            {
                return "空きセル";
            }
            else
            {
                return $"ユニット: {Occupant.Name}";
            }
        }
    }

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
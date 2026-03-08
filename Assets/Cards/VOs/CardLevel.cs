using System;
using System.Collections.Generic;
using TestCardGame.CommonBase;
namespace TestCardGame.Cards.VOs
{

/// <summary>
/// カードのレベルを表すValueObject
/// </summary>
public class CardLevel: VOBase
{
    public int Level { get; }
    private const int _maxLevel = 3;
    private const int _minLevel = 1;
    
    /// <summary>
    /// カードレベルを指定してCardLevelを作成するコンストラクタ
    /// </summary>
    public CardLevel(int level)
    {
        if (level < _minLevel || level > _maxLevel)
        {
            throw new ArgumentOutOfRangeException(nameof(level), $"カードレベルは{_minLevel} ～ {_maxLevel}の範囲でなければなりません。");
        }
        Level = level;
    }
    public bool CanUpgrade => Level < _maxLevel;
    public bool CanDowngrade => Level > _minLevel;
    /// <summary>
    /// カードレベルを1上げた新しいCardLevelを返す
    /// </summary>
    public CardLevel Upgrade()
    {
        if (!CanUpgrade)
        {
            throw new InvalidOperationException("これ以上カードレベルを上げることはできません。");
        }
        return new CardLevel(Level + 1);
    }
    /// <summary>
    /// カードレベルを1下げた新しいCardLevelを返す
    /// </summary>
    public CardLevel Downgrade()
    {
        if (!CanDowngrade)
        {
            throw new InvalidOperationException("これ以上カードレベルを下げることはできません。");
        }
        return new CardLevel(Level - 1);
    }

    /// <summary>レベル1のCardLevelを返すスタティックプロパティ</summary>
    public static CardLevel one => new CardLevel(1);
    /// <summary>レベル2のCardLevelを返すスタティックプロパティ</summary>
    public static CardLevel two => new CardLevel(2);
    /// <summary>レベル3のCardLevelを返すスタティックプロパティ</summary>
    public static CardLevel three => new CardLevel(3);

    public static implicit operator int(CardLevel level)
    {
        return level.Level;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Level;
    }
    }
}
using TestCardGame.BoardManage;
using TestCardGame.Charactor;
using TestCardGame.Charactor.ValueObjects;
using TestCardGame.Controller;
using UnityEngine;

namespace TestCardGame.Cards
{

/// <summary>
/// カード効果の実行に必要な情報をまとめたクラス
/// </summary>
public class CardContext
{
    /// <summary>
    /// コントローラ
    /// </summary>
    public GameController Controller { get; }
    /// <summary>
    /// カードを使用するユニットのID
    /// </summary>
    public IUnit User { get; }
    /// <summary>
    /// カードの効果が対象とする位置,方向（例: 移動先、攻撃対象など）
    /// </summary>
    public Vector2Int TargetPosition { get; }

    public CardContext(GameController controller, IUnit user, Vector2Int targetPosition)
    {
        Controller = controller;
        User = user;
        TargetPosition = targetPosition;
    }
}
}
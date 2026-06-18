

using UnityEngine;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// ユニットの移動を変更するカード修飾子
    /// </summary>
    public class mMove : CardModifier
    {
        public int _moveBonus { get; }

        public mMove(Vector2Int delta)
        {
            _moveBonus = delta.x + delta.y;
        }
        
        public mMove(int x, int y)
        {
            _moveBonus = x + y;
        }
    }
}
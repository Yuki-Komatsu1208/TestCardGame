using System.Numerics;
using TestCardGame.Charactor.StatusVO;
using TestCardGame.Charactor.ValueObjects;
using UnityEngine;

namespace TestCardGame.Charactor
{
    /// <summary>
    /// ゲーム内のユニットを表すインターフェース
    /// </summary>
    public interface IUnit
    {
        /// <summary>
        /// ユニットのID
        /// </summary>
        UnitID ID { get; }
        /// <summary>
        /// ユニットの名前
        /// </summary>
        string Name { get; }
        /// <summary>
        /// ユニットのHP
        /// </summary>
        HP Hp { get; }
        /// <summary>
        /// ユニットの位置
        /// </summary>
        Vector2Int Position { get; set; }
        /// <summary>
        /// ユニットを指定した座標に移動する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void MoveTo(int x, int y);

    }
}
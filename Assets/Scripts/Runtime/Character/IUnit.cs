using TestCardGame.Character.StatusVO;
using TestCardGame.Character.ValueObjects;
using TestCardGame.Character.StatusEffects;
using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.Character
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

        /// <summary>
        /// 現在ユニットに付与されているすべての状態異常。
        /// </summary>
        IReadOnlyList<StatusEffectInstance> StatusEffects { get; }

        /// <summary>
        /// ユニットに状態異常を付与する。
        /// </summary>
        void ApplyStatusEffect(StatusEffectInstance effect);

        /// <summary>
        /// 期限切れの状態異常をリストから除外する。
        /// </summary>
        void CleanExpiredStatusEffects();

        /// <summary>
        /// ユニットが現在行動可能かどうかを状態異常から判定する。
        /// </summary>
        bool CanAct { get; }
    }
}
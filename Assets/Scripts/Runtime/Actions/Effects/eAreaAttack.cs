using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public sealed class eAreaAttack : ActionEffect
    {
        private readonly int damage;
        private readonly int radius;

        /// <summary>
        /// ダメージ量と範囲半径を指定して範囲攻撃を作成する。
        /// </summary>
        public eAreaAttack(int damage, int radius)
        {
            this.damage = damage;
            this.radius = radius;
        }

        /// <summary>
        /// 対象マスが盤面上に存在するか判定する。
        /// </summary>
        public override bool CanExecute(ActionContext context)
        {
            return context.MoveService.GetCellAt(context.TargetPosition) != null;
        }

        /// <summary>
        /// 対象地点を中心に、マンハッタン距離内のユニットへダメージを与える。
        /// </summary>
        public override void Execute(ActionContext context)
        {
            var targetPos = context.TargetPosition;
            Debug.Log($"範囲攻撃: {targetPos} を中心に半径 {radius}、ダメージ {damage} の攻撃を実行します。");

            for (int dx = -radius; x_offset(dx) <= radius; dx++)
            {
                for (int dy = -radius; y_offset(dy) <= radius; dy++)
                {
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) <= radius)
                    {
                        var checkPos = targetPos + new Vector2Int(dx, dy);
                        var unit = context.MoveService.GetUnitAt(checkPos);
                        if (unit != null)
                        {
                            if (context.StatusEffectService?.DamageService != null)
                            {
                                context.StatusEffectService.DamageService.DealDamage(context.User, unit, damage, TestCardGame.Controller.Services.DamageType.Normal);
                            }
                            else
                            {
                                unit.Hp.TakeDamage(damage);
                                Debug.Log($"範囲攻撃: {checkPos} の {unit.Name} に {damage} ダメージを与えました。残りHP: {unit.Hp.CurrentValue}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// X方向の距離を返す。
        /// </summary>
        private int x_offset(int dx) => Mathf.Abs(dx);

        /// <summary>
        /// Y方向の距離を返す。
        /// </summary>
        private int y_offset(int dy) => Mathf.Abs(dy);
    }
}

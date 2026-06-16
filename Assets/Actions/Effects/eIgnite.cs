using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 指定したマスに、一定ターン継続してダメージを与える炎上効果を付与する。
    /// </summary>
    public sealed class eIgnite : ActionEffect
    {
        // 炎上効果が継続するターン数。
        private readonly int duration;

        // 1ターンごとに与えるダメージ量。
        private readonly int damage;

        /// <summary>
        /// 炎上効果の継続ターン数とターンごとのダメージ量を指定して生成する。
        /// </summary>
        /// <param name="duration">炎上効果が継続するターン数。</param>
        /// <param name="damage">1ターンごとに与えるダメージ量。</param>
        public eIgnite(int duration, int damage)
        {
            this.duration = duration;
            this.damage = damage;
        }

        /// <summary>
        /// 使用者からマンハッタン距離2以内の対象マスに炎上効果を付与する。
        /// </summary>
        /// <param name="context">使用者、対象位置、マス取得処理を含む実行コンテキスト。</param>
        public override void Execute(ActionContext context)
        {
            Vector2Int userPos = context.User.Position;
            Vector2Int targetPos = context.TargetPosition;

            // 斜め方向を含めず、縦横の移動量の合計で対象までの距離を判定する。
            int dist = Mathf.Abs(targetPos.x - userPos.x) + Mathf.Abs(targetPos.y - userPos.y);
            if (dist <= 2)
            {
                var cell = context.MoveService.GetCellAt(targetPos);
                if (cell != null)
                {
                    // 対象マスに継続ターン数とターンごとのダメージ量を設定する。
                    cell.ApplyFire(duration, damage);
                    Debug.Log($"座標（{targetPos.x}, {targetPos.y}）を炎上状態にしました。持続ターン数: {duration}、毎ターンのダメージ: {damage}。");
                }
            }
            else
            {
                Debug.LogWarning("炎上効果の対象が遠すぎます。射程は2マス以内です。");
            }
        }
    }
}

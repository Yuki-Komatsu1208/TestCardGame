using TestCardGame.Actions.Core;
using TestCardGame.Character.StatusEffects;
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
        public override bool CanExecute(ActionContext context)
        {
            Vector2Int userPos = context.User.Position;
            Vector2Int targetPos = context.TargetPosition;

            int dist = Mathf.Abs(targetPos.x - userPos.x) + Mathf.Abs(targetPos.y - userPos.y);
            return dist <= 2 && context.MoveService.GetCellAt(targetPos) != null;
        }

        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context))
            {
                Debug.LogWarning("炎上効果の対象が盤面外、または遠すぎます。射程は2マス以内です。");
                return;
            }

            Vector2Int targetPos = context.TargetPosition;
            var cell = context.MoveService.GetCellAt(targetPos);
            cell.ApplyFire(duration, damage);
            Debug.Log($"座標（{targetPos.x}, {targetPos.y}）を炎上状態にしました。持続ターン数: {duration}、毎ターンのダメージ: {damage}。");

            // 対象マスにユニットが既に立っている場合、その場で炎上状態異常を付与する。
            if (cell.Occupant != null)
            {
                context.StatusEffectService?.ApplyBurn(cell.Occupant, duration, damage);
                Debug.Log($"炎上効果: 対象マスにユニットが既に立っていたため、{cell.Occupant.Name}にその場で炎上状態（{duration}ターン、ダメージ: {damage}）を適用しました。");
            }
        }
    }
}

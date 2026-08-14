using System;
using System.Collections.Generic;
using TestCardGame.Actions.Core;
using TestCardGame.Actions.Preview;
using TestCardGame.Cards.Core;
using TestCardGame.Character;
using TestCardGame.Character.Player;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 弾丸を消費し、弾種効果、ノックバック、脱力を与える牽制射撃。
    /// </summary>
    public sealed class eSuppressingShot : ActionEffect, IActionPreviewProvider
    {
        private readonly IReadOnlyDictionary<CardDefinitionSO, GunshotBulletType> bulletTypes;
        private readonly int damage;
        private readonly int knockbackDistance;
        private readonly int weakenedDuration;
        private readonly int forceMoveStunDamage;
        private readonly int range;

        public eSuppressingShot(
            IReadOnlyDictionary<CardDefinitionSO, GunshotBulletType> bulletTypes,
            int damage,
            int knockbackDistance,
            int weakenedDuration,
            int range,
            int forceMoveStunDamage)
        {
            this.bulletTypes = bulletTypes ?? throw new ArgumentNullException(nameof(bulletTypes));
            this.damage = Mathf.Max(1, damage);
            this.knockbackDistance = Mathf.Max(1, knockbackDistance);
            this.weakenedDuration = Mathf.Max(1, weakenedDuration);
            this.range = Mathf.Max(1, range);
            this.forceMoveStunDamage = Mathf.Max(0, forceMoveStunDamage);
        }

        public override bool CanExecute(ActionContext context)
        {
            if (context?.User is not PlayerUnit player || context.CardTargetSelectionService == null) return false;

            int distance = Mathf.Abs(context.TargetPosition.x - player.Position.x) + Mathf.Abs(context.TargetPosition.y - player.Position.y);
            return distance > 0
                && distance <= range
                && context.MoveService.GetUnitAt(context.TargetPosition) != null
                && player.Cards.Exists(card => card != null && bulletTypes.ContainsKey(card.Definition));
        }

        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context)) return;

            var player = (PlayerUnit)context.User;
            context.CardTargetSelectionService.RequestSelection(
                player.Cards,
                card => card != null && bulletTypes.ContainsKey(card.Definition),
                bullet => ResolveShot(context, player, bullet),
                "牽制に使用する弾丸を選択");
        }

        private void ResolveShot(ActionContext context, PlayerUnit player, CardBase bullet)
        {
            if (bullet == null || !bulletTypes.TryGetValue(bullet.Definition, out GunshotBulletType bulletType) || !player.Cards.Contains(bullet)) return;

            var target = context.MoveService.GetUnitAt(context.TargetPosition);
            if (target == null) return;

            DealDamage(context, target.Position, damage);
            ApplyBulletEffect(context, player, target, bullet, bulletType);
            context.StatusEffectService?.ApplyStatus(target, StatusEffectId.Weakened, weakenedDuration);
            context.MoveService.ForceMove(context.User, target, knockbackDistance, ForceMoveDirection.AwayFromUser, forceMoveStunDamage);
            player.Cards.Remove(bullet);
        }

        private void ApplyBulletEffect(ActionContext context, PlayerUnit player, IUnit target, CardBase bullet, GunshotBulletType bulletType)
        {
            switch (bulletType)
            {
                case GunshotBulletType.Piercing:
                    Vector2Int direction = Normalize(target.Position - player.Position);
                    DealDamage(context, target.Position + direction, damage);
                    break;
                case GunshotBulletType.Fire:
                    context.StatusEffectService?.ApplyBurn(target, bullet.Level.Level, 10);
                    break;
                case GunshotBulletType.Frost:
                    context.StatusEffectService?.ApplyFrostbite(target, bullet.Level.Level);
                    break;
            }
        }

        private static void DealDamage(ActionContext context, Vector2Int position, int damage)
        {
            var target = context.MoveService.GetUnitAt(position);
            if (target == null) return;
            context.StatusEffectService?.DamageService?.DealDamage(context.User, target, damage, DamageType.Normal);
        }

        private static Vector2Int Normalize(Vector2Int offset)
        {
            return Mathf.Abs(offset.x) >= Mathf.Abs(offset.y)
                ? new Vector2Int(Math.Sign(offset.x), 0)
                : new Vector2Int(0, Math.Sign(offset.y));
        }

        /// <summary>弾丸を消費せず、対象と貫通時に届き得る奥1セルを返す。</summary>
        public ActionPreviewResult GetPreview(ActionPreviewContext context)
        {
            if (context?.User is not PlayerUnit player || context.MoveService == null) return ActionPreviewResult.None;
            int distance = Mathf.Abs(context.CandidatePosition.x - player.Position.x) + Mathf.Abs(context.CandidatePosition.y - player.Position.y);
            bool hasBullet = player.Cards.Exists(card => card != null && bulletTypes.ContainsKey(card.Definition));
            bool valid = distance > 0 && distance <= range && context.MoveService.GetUnitAt(context.CandidatePosition) != null && hasBullet;
            if (!valid) return new ActionPreviewResult(true, Array.Empty<Vector2Int>(), Array.Empty<Vector2Int>());
            var cells = new List<Vector2Int> { context.CandidatePosition };
            Vector2Int behind = context.CandidatePosition + Normalize(context.CandidatePosition - player.Position);
            if (context.MoveService.GetCellAt(behind) != null) cells.Add(behind);
            return new ActionPreviewResult(true, new[] { context.CandidatePosition }, cells);
        }
    }
}

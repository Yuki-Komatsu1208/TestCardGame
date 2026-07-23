using System;
using System.Collections.Generic;
using TestCardGame.Actions.Core;
using TestCardGame.Cards.Core;
using TestCardGame.Character.Player;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public enum GunshotBulletType { Piercing, Fire, Frost }

    public sealed class eGunshot : ActionEffect
    {
        private readonly IReadOnlyDictionary<CardDefinitionSO, GunshotBulletType> bulletTypes;
        private readonly int range;
        private readonly int gunshotLevel;

        public eGunshot(IReadOnlyDictionary<CardDefinitionSO, GunshotBulletType> bulletTypes, int range, int gunshotLevel)
        {
            this.bulletTypes = bulletTypes ?? throw new ArgumentNullException(nameof(bulletTypes));
            this.range = Mathf.Max(1, range);
            this.gunshotLevel = Mathf.Clamp(gunshotLevel, 1, 3);
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
                "発動する弾丸を選択");
        }

        private void ResolveShot(ActionContext context, PlayerUnit player, CardBase bullet)
        {
            if (bullet == null || !bulletTypes.TryGetValue(bullet.Definition, out var bulletType) || !player.Cards.Contains(bullet)) return;

            int damage = gunshotLevel * 10 + bullet.Level.Level * 10;
            DealDamage(context, context.TargetPosition, damage);

            if (bulletType == GunshotBulletType.Piercing)
            {
                var direction = Normalize(context.TargetPosition - player.Position);
                DealDamage(context, context.TargetPosition + direction, damage);
            }
            else if (bulletType == GunshotBulletType.Fire)
            {
                var target = context.MoveService.GetUnitAt(context.TargetPosition);
                context.StatusEffectService?.ApplyBurn(target, bullet.Level.Level, 10);
            }
            else if (bulletType == GunshotBulletType.Frost)
            {
                var target = context.MoveService.GetUnitAt(context.TargetPosition);
                context.StatusEffectService?.ApplyFrostbite(target, bullet.Level.Level);
            }

            player.Cards.Remove(bullet);
            new eMove(1, true).Execute(context);
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
    }
}

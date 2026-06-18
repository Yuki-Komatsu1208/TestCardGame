using TestCardGame.Actions.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    public sealed class eAreaAttack : ActionEffect
    {
        private readonly int damage;
        private readonly int radius;

        public eAreaAttack(int damage, int radius)
        {
            this.damage = damage;
            this.radius = radius;
        }

        public override bool CanExecute(ActionContext context)
        {
            return context.MoveService.GetCellAt(context.TargetPosition) != null;
        }

        public override void Execute(ActionContext context)
        {
            var targetPos = context.TargetPosition;
            Debug.Log($"AreaAttack: Executing attack with damage {damage} and radius {radius} around {targetPos}");

            // Iterate over all cells in the board to find units in radius
            // We can get the board width and height by querying our cell helper or coordinates
            // Wait, we can get cells within the radius of targetPos
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
                            // Apply damage!
                            if (context.StatusEffectService?.DamageService != null)
                            {
                                context.StatusEffectService.DamageService.DealDamage(context.User, unit, damage, TestCardGame.Controller.Services.DamageType.Normal);
                            }
                            else
                            {
                                unit.Hp.TakeDamage(damage);
                                Debug.Log($"AreaAttack: {unit.Name} took {damage} damage at {checkPos}. Remaining HP: {unit.Hp.CurrentValue}");
                            }
                        }
                    }
                }
            }
        }

        private int x_offset(int dx) => Mathf.Abs(dx);
        private int y_offset(int dy) => Mathf.Abs(dy);
    }
}
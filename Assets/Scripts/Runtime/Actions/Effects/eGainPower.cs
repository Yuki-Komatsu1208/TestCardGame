using TestCardGame.Actions.Core;
using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 経過ターンなどにより使用者自身の攻撃ダメージ上昇（力上昇）を付与する効果。
    /// </summary>
    public sealed class eGainPower : ActionEffect
    {
        private readonly int amount;

        public eGainPower(int amount)
        {
            this.amount = amount;
        }

        public override bool CanExecute(ActionContext context) => context.User != null;

        public override void Execute(ActionContext context)
        {
            if (!CanExecute(context)) return;
            context.StatusEffectService?.ApplyStatus(context.User, StatusEffectId.Power, 999, amount);
            Debug.Log($"{context.User.Name}はターン経過により力を蓄積した！ (力上昇: +{amount})");
        }
    }
}
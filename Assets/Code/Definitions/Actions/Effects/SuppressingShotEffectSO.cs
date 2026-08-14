using System.Collections.Generic;
using TestCardGame.Cards.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 弾丸を消費して牽制射撃を行う効果の設定を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewSuppressingShotEffect", menuName = "Card Game/Effects/Suppressing Shot")]
    public sealed class SuppressingShotEffectSO : ActionEffectSO
    {
        [SerializeField] private CardDefinitionSO piercingBullet;
        [SerializeField] private CardDefinitionSO fireBullet;
        [SerializeField] private CardDefinitionSO frostBullet;

        public override string[] ParameterFields => new[] { "damage", "distance", "duration", "range", "forceMoveStunDamage" };

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var types = new Dictionary<CardDefinitionSO, GunshotBulletType>();
            if (piercingBullet != null) types[piercingBullet] = GunshotBulletType.Piercing;
            if (fireBullet != null) types[fireBullet] = GunshotBulletType.Fire;
            if (frostBullet != null) types[frostBullet] = GunshotBulletType.Frost;

            return new eSuppressingShot(
                types,
                Mathf.Max(1, parameters?.damage ?? 5),
                Mathf.Max(1, parameters?.distance ?? 1),
                Mathf.Max(1, parameters?.duration ?? 1),
                Mathf.Max(1, parameters?.range ?? 5),
                Mathf.Max(0, parameters?.forceMoveStunDamage ?? 10));
        }
    }
}

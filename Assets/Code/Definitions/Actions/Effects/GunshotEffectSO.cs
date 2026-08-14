using System.Collections.Generic;
using TestCardGame.Cards.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewGunshotEffect", menuName = "Card Game/Effects/Gunshot")]
    public sealed class GunshotEffectSO : ActionEffectSO
    {
        [SerializeField] private CardDefinitionSO piercingBullet;
        [SerializeField] private CardDefinitionSO fireBullet;
        [SerializeField] private CardDefinitionSO frostBullet;

        public override string[] ParameterFields => new[] { "range" };

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var types = new Dictionary<CardDefinitionSO, GunshotBulletType>();
            if (piercingBullet != null) types[piercingBullet] = GunshotBulletType.Piercing;
            if (fireBullet != null) types[fireBullet] = GunshotBulletType.Fire;
            if (frostBullet != null) types[frostBullet] = GunshotBulletType.Frost;
            return new eGunshot(types, Mathf.Max(1, parameters?.range ?? 10), level);
        }
    }
}
